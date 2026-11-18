using System;
using System.IO;

namespace Znalica.Storage
{
    /// <summary>
    /// The layout of the data folder: <c>Settings.json</c> and, per profile, a folder named by the profile's id holding
    /// <c>Profile.json</c> and <c>Answers.jsonl</c>. The only place that builds these paths. A profile id is checked to be a
    /// GUID before it becomes part of a path, so an id read from anywhere cannot point outside the folder.
    /// </summary>
    public sealed class DataFolder
    {
        private const string SettingsFileName = "Settings.json";
        private const string ProfilesFolderName = "Profiles";
        private const string ProfileFileName = "Profile.json";
        private const string AnswersFileName = "Answers.jsonl";

        private readonly string _root;

        public DataFolder(string root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public string SettingsPath => Path.Combine(_root, SettingsFileName);

        public string ProfilesPath => Path.Combine(_root, ProfilesFolderName);

        public string ProfileFolder(string profileId)
        {
            return Path.Combine(ProfilesPath, Canonical(profileId));
        }

        public string ProfilePath(string profileId)
        {
            return Path.Combine(ProfileFolder(profileId), ProfileFileName);
        }

        public string AnswersPath(string profileId)
        {
            return Path.Combine(ProfileFolder(profileId), AnswersFileName);
        }

        /// <summary>The id in its one written form, or an <see cref="ArgumentException"/> if it is not a GUID.</summary>
        public static string Canonical(string profileId)
        {
            if (!Guid.TryParse(profileId, out Guid id))
            {
                throw new ArgumentException("A profile id is a GUID, not '" + profileId + "'.", nameof(profileId));
            }

            return id.ToString("D");
        }
    }
}
