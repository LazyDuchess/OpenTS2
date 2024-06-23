//
// Copyright (C) 2003 Jean-Marc Valin
// Copyright (C) 2009-2010 Christoph Fröschl
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 
// - Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// 
// - Neither the name of the Xiph.org Foundation nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System.Diagnostics;
using System;

namespace NSpeex
{
    /// <summary>
    /// Jitter buffer implemenation.
    /// </summary>
    public class JitterBuffer
    {
        private static int RoundDown(int x, int step)
        {
            if (x < 0)
            {
                return (x - step + 1) / step * step;
            }
            else
            {
                return x / step * step;
            }
        }
        
        private const int MAX_BUFFER_SIZE = 200;
        private const int MAX_TIMINGS = 40;
        private const int MAX_BUFFERS = 3;
        private const int TOP_DELAY = 40;

        class TimingBuffer
        {
            public int filled;
            public int curr_count;
            public int[] timing = new int[MAX_TIMINGS];
            public short[] counts = new short[MAX_TIMINGS];

            internal void Init()
            {
                filled = 0;
                curr_count = 0;
            }

            internal void Add(short timing)
            {
                int pos;
                /* Discard packet that won't make it into the list because they're too early */
                if (filled >= MAX_TIMINGS && timing >= this.timing[filled - 1])
                {
                    curr_count++;
                    return;
                }

                /* Find where the timing info goes in the sorted list */
                pos = 0;
                /* FIXME: Do bisection instead of linear search */
                while (pos < filled && timing >= this.timing[pos])
                {
                    pos++;
                }

                Debug.Assert(pos <= filled && pos < MAX_TIMINGS);

                /* Shift everything so we can perform the insertion */
                if (pos < filled)
                {
                    int move_size = filled - pos;
                    if (filled == MAX_TIMINGS)
                        move_size -= 1;
                    
                    Array.Copy(this.timing, pos, this.timing, pos + 1, move_size);
                    //SPEEX_MOVE(&tb->timing[pos + 1], &tb->timing[pos], move_size);
                    Array.Copy(counts, pos, counts, pos + 1, move_size);
                    //SPEEX_MOVE(&tb->counts[pos + 1], &tb->counts[pos], move_size);
                }
                /* Insert */
                this.timing[pos] = timing;
                counts[pos] = (short)curr_count;

                curr_count++;
                if (filled < MAX_TIMINGS)
                    filled++;
            }
        }

        /// <summary>
        /// Represents the container for one packte in the buffer.
        /// </summary>
        public struct JitterBufferPacket
        {
            /// <summary>
            /// Data bytes contained in the packet
            /// </summary>
            public byte[] data;
            
            /// <summary>
            /// Length of the packet in bytes
            /// </summary>
            public int len;

            /// <summary>
            /// Timestamp for the packet
            /// </summary>
            public long timestamp;

            /// <summary>
            /// Time covered by the packet (same units as timestamp)
            /// </summary>
            public long span;

            /// <summary>
            /// RTP Sequence number if available (0 otherwise)
            /// </summary>
            public long sequence;

            /// <summary>
            /// Put whatever data you like here (it's ignored by the jitter buffer)
            /// </summary>
            public long user_data;
        }


        /// <summary>
        /// Timestamp of what we will *get* next
        /// </summary>
        long pointer_timestamp;

        /// <summary>
        /// Useful for getting the next packet with the same timestamp (for fragmented media)
        /// </summary>
        long last_returned_timestamp;

        /// <summary>
        /// Estimated time the next get() will be called
        /// </summary>
        long next_stop;

        /// <summary>
        /// Amount of data we think is still buffered by the application (timestamp units)
        /// </summary>
        long buffered;

        /// <summary>
        /// Packets stored in the buffer
        /// </summary>
        JitterBufferPacket[] packets = new JitterBufferPacket[MAX_BUFFER_SIZE];

        /// <summary>
        /// Packet arrival time (0 means it was late, even though it's a valid timestamp)
        /// </summary>
        long[] arrival = new long[MAX_BUFFER_SIZE];

        /// <summary>
        /// Callback for destroying a packet
        /// </summary>
        public Action<byte[]> DestroyBufferCallback;

        /// <summary>
        /// Size of the steps when adjusting buffering (timestamp units)
        /// </summary>
        public int delay_step;
        /// <summary>
        /// Size of the packet loss concealment "units"
        /// </summary>
        int concealment_size;
        /// <summary>
        /// True if state was just reset
        /// </summary>
        bool reset_state;
        /// <summary>
        /// How many frames we want to keep in the buffer (lower bound)
        /// </summary>
        int buffer_margin;
        /// <summary>
        /// How late must a packet be for it not to be considered at all
        /// </summary>
        int late_cutoff;

        /// <summary>
        /// An interpolation is requested by <see cref="UpdateDelay"/>
        /// </summary>
        int interp_requested;

        /// <summary>
        /// Whether to automatically adjust the delay at any time
        /// </summary>
        bool auto_adjust;


        /// <summary>
        /// Don't use those directly
        /// </summary>
        TimingBuffer[] _tb = new TimingBuffer[MAX_BUFFERS];
        /// <summary>
        /// Storing arrival time of latest frames so we can compute some stats
        /// </summary>
        TimingBuffer[] timeBuffers = new TimingBuffer[MAX_BUFFERS];
        /// <summary>
        /// Total window over which the late frames are counted
        /// </summary>
        public int window_size;
        /// <summary>
        /// Sub-window size for faster computation
        /// </summary>
        int subwindow_size;
        /// <summary>
        /// Absolute maximum amount of late packets tolerable (in percent)
        /// </summary>
        int max_late_rate;
        /// <summary>
        /// Latency equivalent of losing one percent of packets
        /// </summary>
        public int latency_tradeoff;
        /// <summary>
        /// Latency equivalent of losing one percent of packets (automatic default)
        /// </summary>
        public int auto_tradeoff;

        /// <summary>
        /// Number of consecutive lost packets
        /// </summary>
        int lost_count;

        private void FreeBuffer(byte[] buffer)
        {
        }

        private byte[] AllocBuffer(long size)
        {
            return new byte[size];
        }

        /// <summary>
        /// Initializes the jitterbuffer with a given <paramref name="step_size"/>.
        /// </summary>
        /// <param name="step_size"></param>
        public void Init(int step_size)
        {
            if (step_size <= 0)
                throw new ArgumentOutOfRangeException("step_size");

            int i;
            int tmp;
            for (i = 0; i < MAX_BUFFER_SIZE; i++)
                packets[i].data = null;

            for (i = 0; i < MAX_BUFFERS; i++)
                _tb[i] = new TimingBuffer();

            delay_step = step_size;
            concealment_size = step_size;
            /*FIXME: Should this be 0 or 1?*/
            buffer_margin = 0;
            late_cutoff = 50;
            DestroyBufferCallback = null;
            latency_tradeoff = 0;
            auto_adjust = true;
            tmp = 4;
            SetMaxLateRate(tmp);
            Reset();
        }

        void SetMaxLateRate(int maxLateRate)
        {
            max_late_rate = maxLateRate;
            window_size = 100 * TOP_DELAY / max_late_rate;
            subwindow_size = window_size / MAX_BUFFERS;
        }

        void Reset()
        {
            int i;
            for (i = 0; i < MAX_BUFFER_SIZE; i++)
            {
                if (packets[i].data != null)
                {
                    if (DestroyBufferCallback != null)
                        DestroyBufferCallback(packets[i].data);
                    else
                        FreeBuffer(packets[i].data);
                    packets[i].data = null;
                }
            }
            /* Timestamp is actually undefined at this point */
            pointer_timestamp = 0;
            next_stop = 0;
            reset_state = true;
            lost_count = 0;
            buffered = 0;
            auto_tradeoff = 32000;

            for (i = 0; i < MAX_BUFFERS; i++)
            {
                _tb[i].Init();
                timeBuffers[i] = _tb[i];
            }
            /*fprintf (stderr, "reset\n");*/
        }

        /// <summary>
        /// Based on available data, this computes the optimal delay for the jitter buffer. 
        /// The optimised function is in timestamp units and is:
        /// cost = delay + late_factor*[number of frames that would be late if we used that delay]
        /// </summary>
        /// <returns></returns>
        short ComputeOptDelay()
        {
            int i;
            short opt = 0;
            int best_cost = 0x7fffffff;
            int late = 0;
            int[] pos = new int[MAX_BUFFERS];
            int tot_count;
            float late_factor;
            bool penalty_taken = false;
            int best = 0;
            int worst = 0;
            int deltaT;
            TimingBuffer[] tb = _tb;

            /* Number of packet timings we have received (including those we didn't keep) */
            tot_count = 0;
            for (i = 0; i < MAX_BUFFERS; i++)
                tot_count += tb[i].curr_count;
            if (tot_count == 0)
                return 0;

            /* Compute cost for one lost packet */
            if (latency_tradeoff != 0)
                late_factor = latency_tradeoff * 100.0f / tot_count;
            else
                late_factor = auto_tradeoff * window_size / tot_count;

            /*fprintf(stderr, "late_factor = %f\n", late_factor);*/
            for (i = 0; i < MAX_BUFFERS; i++)
                pos[i] = 0;

            /* Pick the TOP_DELAY "latest" packets (doesn't need to actually be late 
               for the current settings) */
            for (i = 0; i < TOP_DELAY; i++)
            {
                int j;
                int next = -1;
                int latest = 32767;
                /* Pick latest amoung all sub-windows */
                for (j = 0; j < MAX_BUFFERS; j++)
                {
                    if (pos[j] < tb[j].filled && tb[j].timing[pos[j]] < latest)
                    {
                        next = j;
                        latest = tb[j].timing[pos[j]];
                    }
                }
                if (next != -1)
                {
                    int cost;

                    if (i == 0)
                        worst = latest;
                    best = latest;
                    latest = RoundDown(latest, delay_step);
                    pos[next]++;

                    /* Actual cost function that tells us how bad using this delay would be */
                    cost = (int)(-latest + late_factor * late);
                    /*fprintf(stderr, "cost %d = %d + %f * %d\n", cost, -latest, late_factor, late);*/
                    if (cost < best_cost)
                    {
                        best_cost = cost;
                        opt = (short)latest;
                    }
                }
                else
                {
                    break;
                }

                /* For the next timing we will consider, there will be one more late packet to count */
                late++;
                /* Two-frame penalty if we're going to increase the amount of late frames (hysteresis) */
                if (latest >= 0 && !penalty_taken)
                {
                    penalty_taken = true;
                    late += 4;
                }
            }

            deltaT = best - worst;
            /* This is a default "automatic latency tradeoff" when none is provided */
            auto_tradeoff = 1 + deltaT / TOP_DELAY;
            /*fprintf(stderr, "auto_tradeoff = %d (%d %d %d)\n", auto_tradeoff, best, worst, i);*/

            /* FIXME: Compute a short-term estimate too and combine with the long-term one */

            /* Prevents reducing the buffer size when we haven't really had much data */
            if (tot_count < TOP_DELAY && opt > 0)
                return 0;
            return opt;
        }

        /// <summary>
        /// Take the following timing into consideration for future calculations
        /// </summary>
        /// <param name="timing"></param>
        void UpdateTimings(int timing)
        {
            if (timing < short.MinValue)
                timing = short.MinValue;
            if (timing > short.MaxValue)
                timing = short.MaxValue;
            short localTiming = (short)timing;
            /* If the current sub-window is full, perform a rotation and discard oldest sub-widow */
            if (timeBuffers[0].curr_count >= subwindow_size)
            {
                int i;
                /*fprintf(stderr, "Rotate buffer\n");*/
                TimingBuffer tmp = timeBuffers[MAX_BUFFERS - 1];
                for (i = MAX_BUFFERS - 1; i >= 1; i--)
                    timeBuffers[i] = timeBuffers[i - 1];
                timeBuffers[0] = tmp;
                timeBuffers[0].Init();
            }
            timeBuffers[0].Add(localTiming);
        }

        /// <summary>
        /// Put one packet into the jitter buffer
        /// </summary>
        /// <param name="packet"></param>
        public void Put(JitterBufferPacket packet)
        {
            int i, j;
            bool late;
            /*fprintf (stderr, "put packet %d %d\n", timestamp, span);*/

            // Cleanup buffer (remove old packets that weren't played)
            if (!reset_state)
            {
                for (i = 0; i < MAX_BUFFER_SIZE; i++)
                {
                    /* Make sure we don't discard a "just-late" packet in case we want to play it next (if we interpolate). */
                    if (packets[i].data != null && (packets[i].timestamp + packets[i].span) <= pointer_timestamp)
                    {
                        /*fprintf (stderr, "cleaned (not played)\n");*/
                        if (DestroyBufferCallback != null)
                            DestroyBufferCallback(packets[i].data);
                        else
                            FreeBuffer(packets[i].data);
                        packets[i].data = null;
                    }
                }
            }

            /*fprintf(stderr, "arrival: %d %d %d\n", packet.timestamp, next_stop, pointer_timestamp);*/
            /* Check if packet is late (could still be useful though) */
            if (!reset_state && packet.timestamp < next_stop)
            {
                UpdateTimings(((int)packet.timestamp) - ((int)next_stop) - buffer_margin);
                late = true;
            }
            else
            {
                late = false;
            }

            /* For some reason, the consumer has failed the last 20 fetches. Make sure this packet is
             * used to resync. */
            if (lost_count > 20)
            {
                Reset();
            }

            /* Only insert the packet if it's not hopelessly late (i.e. totally useless) */
            if (reset_state || (packet.timestamp + packet.span + delay_step) >= pointer_timestamp)
            {

                /*Find an empty slot in the buffer*/
                for (i = 0; i < MAX_BUFFER_SIZE; i++)
                {
                    if (packets[i].data == null)
                        break;
                }

                /*No place left in the buffer, need to make room for it by discarding the oldest packet */
                if (i == MAX_BUFFER_SIZE)
                {
                    long earliest = packets[0].timestamp;
                    i = 0;
                    for (j = 1; j < MAX_BUFFER_SIZE; j++)
                    {
                        if (packets[i].data == null || packets[j].timestamp < earliest)
                        {
                            earliest = packets[j].timestamp;
                            i = j;
                        }
                    }
                    if (DestroyBufferCallback != null)
                        DestroyBufferCallback(packets[i].data);
                    else
                        FreeBuffer(packets[i].data);
                    packets[i].data = null;
                    /*fprintf (stderr, "Buffer is full, discarding earliest frame %d (currently at %d)\n", timestamp, pointer_timestamp);*/
                }

                /* Copy packet in buffer */
                if (DestroyBufferCallback != null)
                {
                    packets[i].data = packet.data;
                }
                else
                {
                    packets[i].data = AllocBuffer(packet.len);
                    for (j = 0; j < packet.len; j++)
                        packets[i].data[j] = packet.data[j];
                }
                packets[i].timestamp = packet.timestamp;
                packets[i].span = packet.span;
                packets[i].len = packet.len;
                packets[i].sequence = packet.sequence;
                packets[i].user_data = packet.user_data;
                if (reset_state || late)
                    arrival[i] = 0;
                else
                    arrival[i] = next_stop;
            }
        }

        /// <summary>
        /// Packet has been retrieved
        /// </summary>
        public const int JITTER_BUFFER_OK = 0;

        /// <summary>
        /// Packet is lost or is late
        /// </summary>
        public const int JITTER_BUFFER_MISSING = 1;

        /// <summary>
        /// A "fake" packet is meant to be inserted here to increase buffering
        /// </summary>
        public const int JITTER_BUFFER_INSERTION = 2;

        /// <summary>
        /// There was an error in the jitter buffer
        /// </summary>
        public const int JITTER_BUFFER_INTERNAL_ERROR = -1;

        /// <summary>
        /// Invalid argument
        /// </summary>
        public const int JITTER_BUFFER_BAD_ARGUMENT = -2;

        /// <summary>
        /// Get one packet from the jitter buffer
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="desired_span"></param>
        /// <param name="start_offset"></param>
        /// <returns></returns>
        public int Get(ref JitterBufferPacket packet, int desired_span, out int start_offset)
        {
            if (desired_span <= 0)
                throw new ArgumentOutOfRangeException("desired_span");

            int i;
            long j;
            short opt;

            start_offset = 0;

            /* Syncing on the first call */
            if (reset_state)
            {
                bool found = false;
                /* Find the oldest packet */
                long oldest = 0;
                for (i = 0; i < MAX_BUFFER_SIZE; i++)
                {
                    if (packets[i].data != null && (!found || packets[i].timestamp < oldest))
                    {
                        oldest = packets[i].timestamp;
                        found = true;
                    }
                }
                if (found)
                {
                    reset_state = false;
                    pointer_timestamp = oldest;
                    next_stop = oldest;
                }
                else
                {
                    packet.timestamp = 0;
                    packet.span = interp_requested;
                    return JITTER_BUFFER_MISSING;
                }
            }

            last_returned_timestamp = pointer_timestamp;

            if (interp_requested != 0)
            {
                packet.timestamp = pointer_timestamp;
                packet.span = interp_requested;

                /* Increment the pointer because it got decremented in the delay update */
                pointer_timestamp += interp_requested;
                packet.len = 0;
                /*fprintf (stderr, "Deferred interpolate\n");*/

                interp_requested = 0;

                buffered = packet.span - desired_span;

                return JITTER_BUFFER_INSERTION;
            }

            /* Searching for the packet that fits best */

            /* Search the buffer for a packet with the right timestamp and spanning the whole current chunk */
            for (i = 0; i < MAX_BUFFER_SIZE; i++)
            {
                if (packets[i].data != null && packets[i].timestamp == pointer_timestamp && (packets[i].timestamp + packets[i].span) >= (pointer_timestamp + desired_span))
                    break;
            }

            /* If no match, try for an "older" packet that still spans (fully) the current chunk */
            if (i == MAX_BUFFER_SIZE)
            {
                for (i = 0; i < MAX_BUFFER_SIZE; i++)
                {
                    if (packets[i].data != null && packets[i].timestamp <= pointer_timestamp && (packets[i].timestamp + packets[i].span) >= (pointer_timestamp + desired_span))
                        break;
                }
            }

            /* If still no match, try for an "older" packet that spans part of the current chunk */
            if (i == MAX_BUFFER_SIZE)
            {
                for (i = 0; i < MAX_BUFFER_SIZE; i++)
                {
                    if (packets[i].data != null && packets[i].timestamp <= pointer_timestamp && (packets[i].timestamp + packets[i].span) > pointer_timestamp)
                        break;
                }
            }

            /* If still no match, try for earliest packet possible */
            if (i == MAX_BUFFER_SIZE)
            {
                bool found = false;
                long best_time = 0;
                long best_span = 0;
                int besti = 0;
                for (i = 0; i < MAX_BUFFER_SIZE; i++)
                {
                    /* check if packet starts within current chunk */
                    if (packets[i].data != null && packets[i].timestamp < (pointer_timestamp + desired_span) && (packets[i].timestamp >= pointer_timestamp))
                    {
                        if (!found || packets[i].timestamp < best_time || (packets[i].timestamp == best_time && packets[i].span > best_span))
                        {
                            best_time = packets[i].timestamp;
                            best_span = packets[i].span;
                            besti = i;
                            found = true;
                        }
                    }
                }
                if (found)
                {
                    i = besti;
                    /*fprintf (stderr, "incomplete: %d %d %d %d\n", packets[i].timestamp, pointer_timestamp, chunk_size, packets[i].span);*/
                }
            }

            /* If we find something */
            if (i != MAX_BUFFER_SIZE)
            {
                int offset;

                /* We (obviously) haven't lost this packet */
                lost_count = 0;

                /* In this case, 0 isn't as a valid timestamp */
                if (arrival[i] != 0)
                {
                    UpdateTimings(((int)packets[i].timestamp) - ((int)arrival[i]) - buffer_margin);
                }


                /* Copy packet */
                if (DestroyBufferCallback != null)
                {
                    packet.data = packets[i].data;
                    packet.len = packets[i].len;
                }
                else
                {
                    if (packets[i].len > packet.len)
                    {
                        Debug.WriteLine("JitterBuffer.Get(): packet too large to fit. Size is", packets[i].len);
                    }
                    else
                    {
                        packet.len = packets[i].len;
                    }
                    for (j = 0; j < packet.len; j++)
                        packet.data[j] = packets[i].data[j];
                    /* Remove packet */
                    FreeBuffer(packets[i].data);
                }
                packets[i].data = null;
                /* Set timestamp and span (if requested) */
                offset = (int)packets[i].timestamp - (int)pointer_timestamp;
                if (start_offset != 0)
                    start_offset = offset;
                else if (offset != 0)
                    Debug.WriteLine("JitterBuffer.Get(): discarding non-zero start_offset", offset);

                packet.timestamp = packets[i].timestamp;
                last_returned_timestamp = packet.timestamp;

                packet.span = packets[i].span;
                packet.sequence = packets[i].sequence;
                packet.user_data = packets[i].user_data;
                packet.len = packets[i].len;
                /* Point to the end of the current packet */
                pointer_timestamp = packets[i].timestamp + packets[i].span;

                buffered = packet.span - desired_span;

                if (start_offset != 0)
                    buffered += start_offset;

                return JITTER_BUFFER_OK;
            }


            /* If we haven't found anything worth returning */

            /*fprintf (stderr, "not found\n");*/
            lost_count++;
            /*fprintf (stderr, "m");*/
            /*fprintf (stderr, "lost_count = %d\n", lost_count);*/

            opt = ComputeOptDelay();

            /* Should we force an increase in the buffer or just do normal interpolation? */
            if (opt < 0)
            {
                /* Need to increase buffering */

                /* Shift histogram to compensate */
                ShiftTimings((short)-opt);

                packet.timestamp = pointer_timestamp;
                packet.span = -opt;
                /* Don't move the pointer_timestamp forward */
                packet.len = 0;

                buffered = packet.span - desired_span;
                return JITTER_BUFFER_INSERTION;
                /*pointer_timestamp -= delay_step;*/
                /*fprintf (stderr, "Forced to interpolate\n");*/
            }
            else
            {
                /* Normal packet loss */
                packet.timestamp = pointer_timestamp;

                desired_span = RoundDown(desired_span, concealment_size);
                packet.span = desired_span;
                pointer_timestamp += desired_span;
                packet.len = 0;

                buffered = packet.span - desired_span;
                return JITTER_BUFFER_MISSING;
                /*fprintf (stderr, "Normal loss\n");*/
            }
        }

        /** Compensate all timings when we do an adjustment of the buffering */
        void ShiftTimings(short amount)
        {
            int i, j;
            for (i = 0; i < MAX_BUFFERS; i++)
            {
                for (j = 0; j < timeBuffers[i].filled; j++)
                    timeBuffers[i].timing[j] += amount;
            }
        }

        /// <summary>
        /// Let the jitter buffer know it's the right time to adjust the buffering delay to the network conditions
        /// </summary>
        /// <returns></returns>
        int UpdateDelay()
        {
            short opt = ComputeOptDelay();
            /*fprintf(stderr, "opt adjustment is %d ", opt);*/

            if (opt < 0)
            {
                ShiftTimings((short)-opt);

                pointer_timestamp += opt;
                interp_requested = -opt;
                /*fprintf (stderr, "Decision to interpolate %d samples\n", -opt);*/
            }
            else if (opt > 0)
            {
                ShiftTimings((short)-opt);
                pointer_timestamp += opt;
                /*fprintf (stderr, "Decision to drop %d samples\n", opt);*/
            }

            return opt;
        }

        /// <summary>
        /// Call this method to indicate one step in time (one tick).
        /// </summary>
        public void Tick()
        {
            /* Automatically-adjust the buffering delay if requested */
            if (auto_adjust)
                UpdateDelay();

            if (buffered >= 0)
            {
                next_stop = pointer_timestamp - buffered;
            }
            else
            {
                next_stop = pointer_timestamp;
                Debug.WriteLine("jitter buffer sees negative buffering, your code might be broken. Value is ", buffered);
            }
            buffered = 0;
        }
    }

    /// <summary>
    /// Jitter buffer designed for a speex decoder.
    /// </summary>
    public class SpeexJitterBuffer
    {
        private readonly SpeexDecoder decoder;
        private readonly JitterBuffer buffer = new JitterBuffer();
        private JitterBuffer.JitterBufferPacket outPacket;
        private JitterBuffer.JitterBufferPacket inPacket;

        /// <summary>
        /// Creates a new instance using the given <paramref name="decoder"/>
        /// </summary>
        /// <param name="decoder"></param>
        public SpeexJitterBuffer(SpeexDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException("decoder");

            this.decoder = decoder;

            inPacket.sequence = 0;
            inPacket.span = 1;
            inPacket.timestamp = 1;

            buffer.DestroyBufferCallback = (x) => 
            {
                // GC handles that
            };
            buffer.Init(1);
        }

        /// <summary>
        /// Returns the next decoded frame from the buffer.
        /// </summary>
        /// <param name="decodedFrame"></param>
        public void Get(short[] decodedFrame)
        {
            if (decodedFrame == null)
                throw new ArgumentNullException("decodedFrame");

            if (outPacket.data == null)
            {
                outPacket.data = new byte[decodedFrame.Length * 2];
            }
            else
                Array.Clear(outPacket.data, 0, outPacket.data.Length);

            outPacket.len = outPacket.data.Length;

            int temp;
            if (buffer.Get(ref outPacket, 1, out temp) != JitterBuffer.JITTER_BUFFER_OK)
            {
                // no packet found
                decoder.Decode(null, 0, 0, decodedFrame, 0, true);
            }
            else
            {
                decoder.Decode(outPacket.data, 0, outPacket.len, decodedFrame, 0, false);
            }

            buffer.Tick();
        }

        /// <summary>
        /// Puts the <paramref name="frameData"/> into the buffer. Note that the given byte array
        /// is not copied so you transfer ownership to the buffer.
        /// </summary>
        /// <param name="frameData"></param>
        public void Put(byte[] frameData)
        {
            if (frameData == null)
                throw new ArgumentNullException("frameData");

            inPacket.data = frameData;
            inPacket.len = frameData.Length;
            inPacket.timestamp++;

            buffer.Put(inPacket);
        }
    }
}
