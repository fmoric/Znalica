using System;

namespace Znalica.Storage
{
    /// <summary>
    /// A child's profile as <c>Profile.json</c> keeps it: a display name or nickname, a bundled picture and the date it
    /// was created. There is no surname or birth date field. Public fields because JsonUtility reads and writes fields;
    /// a profile is changed only through <see cref="ProfileStore"/>.
    /// </summary>
    [Serializable]
    public sealed class Profile
    {
        /// <summary>A GUID; also the name of the profile's folder.</summary>
        public string ProfileId;

        public string DisplayName;

        /// <summary>The name of a bundled picture (<see cref="Znalica.Domain.ProfilePictures"/>).</summary>
        public string PictureId;

        /// <summary>ISO 8601 with offset.</summary>
        public string CreatedAt;
    }
}
