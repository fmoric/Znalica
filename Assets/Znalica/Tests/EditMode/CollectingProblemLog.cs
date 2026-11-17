using System.Collections.Generic;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    /// <summary>An <see cref="IProblemLog"/> that keeps the messages, so a test can assert that a problem was reported.</summary>
    public sealed class CollectingProblemLog : IProblemLog
    {
        public List<string> Messages { get; } = new List<string>();

        public void Report(string message)
        {
            Messages.Add(message);
        }
    }
}
