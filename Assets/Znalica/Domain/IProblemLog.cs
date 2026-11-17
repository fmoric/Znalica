namespace Znalica.Domain
{
    /// <summary>
    /// Where the domain and storage report a problem that must not stop the game (a missing phrase, a broken text file).
    /// The domain has no engine reference, so it cannot call Debug.Log; the app passes one that does, tests pass one
    /// that collects the messages.
    /// </summary>
    public interface IProblemLog
    {
        void Report(string message);
    }
}
