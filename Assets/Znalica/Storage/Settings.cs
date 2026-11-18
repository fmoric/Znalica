using System;

namespace Znalica.Storage
{
    /// <summary>The settings in <c>Settings.json</c>. Voice volume only: the parent gate has no setting in this version.</summary>
    [Serializable]
    public sealed class Settings
    {
        /// <summary>0.0 to 1.0.</summary>
        public float VoiceVolume = 1f;
    }
}
