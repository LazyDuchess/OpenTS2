//
// Copyright (C) 2003 Jean-Marc Valin
// Copyright (C) 1999-2003 Wimba S.A., All Rights Reserved.
// Copyright (C) 2008 Filip Navara
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

namespace NSpeex
{
	/// <summary>
	/// Speex Encoder interface, used as a base for the Narrowband and sideband
	/// encoders.
	/// </summary>
    internal interface IEncoder
	{
		/// <summary>
		/// Encode the given input signal.
		/// </summary>
		/// <returns>1 if successful.</returns>
		int Encode(Bits bits, float[] ins0);

		int EncodedFrameSize
		{
			get;
		}

		int FrameSize
		{
			get;
		}

        int Quality
        {
            set;
        }

		int BitRate
		{
			get;
			set;
		}

		float[] PiGain
		{
			get;
		}

		float[] Exc
		{
			get;
		}

		float[] Innov
		{
			get;
		}

		int Mode
		{
			get;
			set;
		}

		bool Vbr
		{
			get;
			set;
		}

		bool Vad
		{
			get;
			set;
		}

		bool Dtx
		{
			get;
			set;
		}

		int Abr
		{
			get;
			set;
		}

		float VbrQuality
		{
			get;
			set;
		}

		int Complexity
		{
			get;
			set;
		}

		int SamplingRate
		{
			get;
			set;
		}

		int LookAhead
		{
			get;
		}

		float RelativeQuality
		{
			get;
		}
	}
}
