using System;

namespace Znalica.Domain
{
    /// <summary>How much was practised in one calendar week: the distinct rounds and all answers recorded in it.</summary>
    public readonly struct WeekPractice
    {
        public WeekPractice(DateTime monday, int rounds, int answers)
        {
            Monday = monday;
            Rounds = rounds;
            Answers = answers;
        }

        /// <summary>The Monday the week starts on (a date, no time).</summary>
        public DateTime Monday { get; }

        /// <summary>Distinct round ids with an answer in the week; a round that runs over midnight on Sunday counts in both weeks.</summary>
        public int Rounds { get; }

        /// <summary>Every recorded answer of the week: first tries, retries and returns.</summary>
        public int Answers { get; }
    }
}
