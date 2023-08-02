using OpenTS2.Content.DBPF.Effects;

namespace OpenTS2.Content.DBPF
{
    /// <summary>
    /// This is effect descriptions for Andrew Willmott's Swarm particles effect system used at Maxis. It is a system
    /// to make particle effects through simple text files:
    ///
    /// <code>
    /// particles "colorTest"
    ///     life 2
    ///     source -square 16
    ///
    ///     rate 200
    ///     emit -speed 200 -dir [0 1 0]
    ///
    ///     color [0 0 1] [1 0 0] [0 1 0] [0 0 0] [1 1 1]
    ///     alpha 1
    ///     size 0.2
    ///     texture snowflake
    /// end
    /// </code>
    ///
    /// These were used by artists to quickly prototype and see their changes real-time in the game. They're used in
    /// some obvious spots like smoke around neighborhood decorations but also in less obvious ways, such as the fishes
    /// in the aquarium being completely particle effects.
    ///
    /// In the shipped game these nice textual descriptions are turned into a binary format, this asset represents
    /// those particle effects.
    ///
    /// See https://www.andrewwillmott.com/talks/swarm-procedural-content for Andrew's talks on Swarm.
    /// </summary>
    public class EffectsAsset : AbstractAsset
    {
        public ParticleEffect[] Particles { get; }
        public MetaParticle[] MetaParticles { get; }
        public DecalEffect[] DecalEffects { get; }
        public SequenceEffect[] SequenceEffects { get; }
        public SoundEffect[] SoundEffects { get; }
        public CameraEffect[] CameraEffects { get; }
        public ModelEffect[] ModelEffects { get; }
        public ScreenEffect[] ScreenEffects { get; }
        public WaterEffect[] WaterEffects { get; }

        public EffectsAsset(ParticleEffect[] particles, MetaParticle[] metaParticles, DecalEffect[] decalEffects,
            SequenceEffect[] sequenceEffects, SoundEffect[] soundEffects, CameraEffect[] cameraEffects,
            ModelEffect[] modelEffects, ScreenEffect[] screenEffects, WaterEffect[] waterEffects)
        {
            Particles = particles;
            MetaParticles = metaParticles;
            DecalEffects = decalEffects;
            SequenceEffects = sequenceEffects;
            SoundEffects = soundEffects;
            CameraEffects = cameraEffects;
            ModelEffects = modelEffects;
            ScreenEffects = screenEffects;
            WaterEffects = waterEffects;
        }
    }
}