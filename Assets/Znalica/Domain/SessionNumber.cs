using System;
using System.Collections.Generic;

namespace Znalica.Domain
{
    /// <summary>
    /// The profile's session counter. A session starts when the profile is picked and its number is one more than the
    /// highest number in the profile's answer log, so an empty log gives session 1.
    /// </summary>
    public static class SessionNumber
    {
        public static int Next(IReadOnlyList<AnswerRecord> log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            int highest = 0;
            foreach (AnswerRecord record in log)
            {
                if (record.SessionNumber > highest)
                {
                    highest = record.SessionNumber;
                }
            }

            return highest + 1;
        }
    }
}
