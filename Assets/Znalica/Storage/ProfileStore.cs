using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Znalica.Domain;

namespace Znalica.Storage
{
    /// <summary>
    /// The profiles on this PC, one folder each. Deleting a profile deletes its folder: the profile, its answers and
    /// whatever else is kept for it, together, so no progress of a deleted profile is left behind. A profile's data never
    /// mixes with another's because each one's files live in its own folder.
    /// </summary>
    public sealed class ProfileStore
    {
        private readonly DataFolder _folder;
        private readonly IClock _clock;
        private readonly IProblemLog _problems;

        public ProfileStore(DataFolder folder, IClock clock, IProblemLog problems)
        {
            _folder = folder ?? throw new ArgumentNullException(nameof(folder));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _problems = problems ?? throw new ArgumentNullException(nameof(problems));
        }

        /// <summary>Every readable profile, oldest first. A folder whose profile file is missing or unreadable is reported and left out.</summary>
        public IReadOnlyList<Profile> List()
        {
            var profiles = new List<Profile>();
            if (!Directory.Exists(_folder.ProfilesPath))
            {
                return profiles;
            }

            foreach (string directory in Directory.GetDirectories(_folder.ProfilesPath))
            {
                string id = Path.GetFileName(directory);
                if (TryRead(id, out Profile profile))
                {
                    profiles.Add(profile);
                }
                else
                {
                    _problems.Report("Skipped the profile folder " + directory + ": no readable Profile.json.");
                }
            }

            profiles.Sort(Compare);
            return profiles;
        }

        /// <summary>A new profile with its own folder. The name must pass <see cref="ProfileName"/> and the picture be a bundled one.</summary>
        public Profile Create(string displayName, string pictureId)
        {
            string name = ValidName(displayName);
            if (!ProfilePictures.Contains(pictureId))
            {
                throw new ArgumentException("Unknown profile picture '" + pictureId + "'.", nameof(pictureId));
            }

            var profile = new Profile
            {
                ProfileId = Guid.NewGuid().ToString("D"),
                DisplayName = name,
                PictureId = pictureId,
                CreatedAt = _clock.Now.ToString("o", CultureInfo.InvariantCulture),
            };
            Write(profile);
            return profile;
        }

        public Profile Rename(string profileId, string displayName)
        {
            string name = ValidName(displayName);
            if (!TryRead(DataFolder.Canonical(profileId), out Profile profile))
            {
                throw new ArgumentException("There is no profile " + profileId + ".", nameof(profileId));
            }

            profile.DisplayName = name;
            Write(profile);
            return profile;
        }

        /// <summary>Deletes the profile's folder with everything in it. False when there was no such profile.</summary>
        public bool Delete(string profileId)
        {
            string folder = _folder.ProfileFolder(profileId);
            if (!Directory.Exists(folder))
            {
                return false;
            }

            Directory.Delete(folder, true);
            return true;
        }

        /// <summary>The profile's answer log, on disk in the profile's folder.</summary>
        public IAnswerLog AnswerLog(string profileId)
        {
            return new FileAnswerLog(_folder.AnswersPath(profileId), _problems);
        }

        /// <summary>The profile's school-like checks, on disk in the profile's folder and never in its answer log.</summary>
        public ICheckLog CheckLog(string profileId)
        {
            return new FileCheckLog(_folder.ChecksPath(profileId), _problems);
        }

        private static string ValidName(string displayName)
        {
            if (!ProfileName.TryNormalize(displayName, out string name))
            {
                throw new ArgumentException(
                    "A profile name is 1 to " + ProfileName.MaxLength + " letters, spaces or hyphens.", nameof(displayName));
            }

            return name;
        }

        private static int Compare(Profile a, Profile b)
        {
            int byDate = ParseDate(a).CompareTo(ParseDate(b));
            return byDate != 0 ? byDate : string.CompareOrdinal(a.DisplayName, b.DisplayName);
        }

        private static DateTimeOffset ParseDate(Profile profile)
        {
            return DateTimeOffset.TryParse(profile.CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset date)
                ? date
                : DateTimeOffset.MinValue;
        }

        private void Write(Profile profile)
        {
            AtomicFileWriter.Write(_folder.ProfilePath(profile.ProfileId), JsonUtility.ToJson(profile));
        }

        private bool TryRead(string profileId, out Profile profile)
        {
            profile = null;
            if (!Guid.TryParse(profileId, out _))
            {
                return false;
            }

            string path = _folder.ProfilePath(profileId);
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                profile = JsonUtility.FromJson<Profile>(File.ReadAllText(path));
            }
            catch (ArgumentException)
            {
                return false;
            }

            // The folder name is the identity: a file that names another id, or has no name, is not this profile.
            return profile != null
                && profile.ProfileId == DataFolder.Canonical(profileId)
                && ProfileName.TryNormalize(profile.DisplayName, out _)
                && ProfilePictures.Contains(profile.PictureId);
        }
    }
}
