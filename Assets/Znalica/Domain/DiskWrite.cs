using System;
using System.IO;

namespace Znalica.Domain
{
    /// <summary>
    /// Runs something that writes to the disk so that a disk that refuses (full, read-only, locked by another program,
    /// no permission) does not stop the app. <see cref="IOException"/> and <see cref="UnauthorizedAccessException"/> are
    /// what File, FileStream and Directory operations throw for those causes; any other exception is a bug and is not caught.
    /// </summary>
    public static class DiskWrite
    {
        /// <summary>Runs <paramref name="write"/>. On a disk failure writes it to the problem log and returns false.</summary>
        public static bool TryRun(Action write, IProblemLog problems, string what)
        {
            if (write == null)
            {
                throw new ArgumentNullException(nameof(write));
            }

            if (problems == null)
            {
                throw new ArgumentNullException(nameof(problems));
            }

            try
            {
                write();
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                problems.Report("Could not " + what + ": " + exception.GetType().Name + ": " + exception.Message);
                return false;
            }
        }
    }
}
