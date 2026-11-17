namespace Znalica.Domain
{
    /// <summary>The voice volume range, and the rule that at the lowest volume nothing is played at all.</summary>
    public static class VoiceVolume
    {
        public const float Min = 0f;
        public const float Max = 1f;

        public static float Clamp(float volume)
        {
            return volume < Min ? Min : volume > Max ? Max : volume;
        }

        /// <summary>True when playing would make no sound, so the player skips playback entirely.</summary>
        public static bool IsSilent(float volume)
        {
            return Clamp(volume) <= Min;
        }
    }
}
