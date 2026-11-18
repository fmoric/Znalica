using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Znalica.Domain
{
    /// <summary>Reads a check log's lines back into the checks they belong to.</summary>
    public static class CheckHistory
    {
        /// <summary>
        /// The checks in the order their first line appears, which is the order they were taken. A check with no closing line
        /// is listed as not finished; a second closing line for one check is ignored.
        /// </summary>
        public static IReadOnlyList<CheckResult> From(IEnumerable<CheckLine> lines)
        {
            var order = new List<string>();
            var answers = new Dictionary<string, List<CheckLine>>();
            var closings = new Dictionary<string, CheckLine>();
            foreach (CheckLine line in lines)
            {
                if (!answers.ContainsKey(line.CheckId))
                {
                    order.Add(line.CheckId);
                    answers[line.CheckId] = new List<CheckLine>();
                }

                if (!line.IsClosing)
                {
                    answers[line.CheckId].Add(line);
                }
                else if (!closings.ContainsKey(line.CheckId))
                {
                    closings[line.CheckId] = line;
                }
            }

            return order
                .Select(id =>
                {
                    closings.TryGetValue(id, out CheckLine closing);
                    string time = answers[id].Count > 0 ? answers[id][0].AnsweredAt : closing.CompletedAt;
                    return new CheckResult(id, ParseDate(time), answers[id], closing);
                })
                .ToList();
        }

        private static DateTimeOffset ParseDate(string text)
        {
            return DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset date)
                ? date
                : DateTimeOffset.MinValue;
        }
    }
}
