using System;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// Giving an answer, the same on every activity screen: the answer is written to the log, and the feedback it produced is
    /// spoken. A write that fails is handled here (<see cref="DiskWrite"/>), so the screen only has to redraw.
    /// </summary>
    internal static class AnswerGiving
    {
        /// <summary>
        /// Runs <paramref name="give"/> (a call to the play that records the answer) and speaks the feedback it returns, if any.
        /// False if the answer could not be saved: nothing moved, the failure is in the problem log, and the screen shows
        /// <see cref="PhraseKey.SaveFailed"/> so the child can give the answer again.
        /// </summary>
        public static bool TryGive(Func<Feedback> give, IProblemLog problems, PhraseCatalog catalog, VoicePlayer voice)
        {
            Feedback feedback = null;
            if (!DiskWrite.TryRun(() => feedback = give(), problems, "save the answer"))
            {
                return false;
            }

            if (feedback != null)
            {
                voice.Play(catalog.VoiceClips(feedback.Phrase, feedback.Args));
            }

            return true;
        }
    }
}
