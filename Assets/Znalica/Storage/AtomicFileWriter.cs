using System.IO;
using System.Text;

namespace Znalica.Storage
{
    /// <summary>
    /// Replaces a whole small file so that a crash leaves either the old content or the new, never half of each: the
    /// text goes to <c>path.tmp</c> first and is flushed to disk, then the temporary file replaces the real one.
    /// <see cref="File.Replace(string, string, string)"/> throws when the destination does not exist, hence the
    /// two branches.
    /// </summary>
    public static class AtomicFileWriter
    {
        private const string TemporarySuffix = ".tmp";

        // No byte order mark: JsonUtility reads the file back as plain text.
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        public static void Write(string path, string text)
        {
            string temporary = path + TemporarySuffix;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            byte[] bytes = Utf8.GetBytes(text);
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (File.Exists(path))
            {
                File.Replace(temporary, path, null);
            }
            else
            {
                File.Move(temporary, path);
            }
        }
    }
}
