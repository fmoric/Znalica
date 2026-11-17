using UnityEngine;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>Reports a problem to the Unity console and the player log.</summary>
    public sealed class UnityProblemLog : IProblemLog
    {
        public void Report(string message)
        {
            Debug.LogError(message);
        }
    }
}
