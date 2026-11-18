using System;
using System.IO;

namespace Znalica.Tests.EditMode
{
    /// <summary>
    /// A folder under the system's temporary folder, made for one test and deleted with it. Storage tests write only
    /// here, never under Application.persistentDataPath.
    /// </summary>
    public sealed class TempFolder : IDisposable
    {
        public TempFolder()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ZnalicaTests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}
