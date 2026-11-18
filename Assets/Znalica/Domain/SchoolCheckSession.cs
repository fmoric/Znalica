using System;
using System.Collections.Generic;
using System.Globalization;

namespace Znalica.Domain
{
    /// <summary>
    /// One school-like check being taken: the worksheet's items in pages of ten, what has been written in each box so far,
    /// and the log. There is no feedback of any kind: nothing here says whether an answer is right until the check is
    /// finished, and then only as the score in the parent area. The answers of a page are recorded when the page is
    /// finished, so a page can still be changed until then; the last page's finish also closes the check with its score.
    /// A check left before that has no closing line and is listed as not finished.
    /// </summary>
    public sealed class SchoolCheckSession
    {
        public const int PageSize = 10;

        private readonly IReadOnlyList<Item> _items;
        private readonly ICheckLog _log;
        private readonly IClock _clock;
        private readonly AnswerEntry[] _numbers;
        private readonly Sign?[] _signs;
        private int _recorded;
        private bool _closed;

        public SchoolCheckSession(IReadOnlyList<Item> items, ICheckLog log, IClock clock, string checkId)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            CheckId = string.IsNullOrEmpty(checkId) ? throw new ArgumentException("A check needs an id.", nameof(checkId)) : checkId;
            if (items.Count == 0)
            {
                throw new ArgumentException("A check holds at least one item.", nameof(items));
            }

            _numbers = new AnswerEntry[items.Count];
            _signs = new Sign?[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                // Refuses an item that has no place on the worksheet.
                SchoolCheckPlanner.SectionOf(items[i]);
                if (!IsSignItem(i))
                {
                    _numbers[i] = new AnswerEntry();
                }
            }
        }

        public string CheckId { get; }

        public IReadOnlyList<Item> Items => _items;

        public int PageCount => (_items.Count + PageSize - 1) / PageSize;

        /// <summary>The page being filled in, from 0; it stays on the last page once the check is finished.</summary>
        public int Page { get; private set; }

        public bool IsFinished => _closed;

        /// <summary>The index of the page's first item.</summary>
        public int PageStart => Page * PageSize;

        /// <summary>One past the index of the page's last item.</summary>
        public int PageEnd => Math.Min(PageStart + PageSize, _items.Count);

        public bool IsSignItem(int index)
        {
            return _items[index] is ComparisonItem;
        }

        /// <summary>The number in the item's box, or null while it is empty or the item is a sign item.</summary>
        public int? Entered(int index)
        {
            return _numbers[index]?.Value;
        }

        /// <summary>The sign chosen for a sign item, or null.</summary>
        public Sign? Chosen(int index)
        {
            return _signs[index];
        }

        /// <summary>
        /// A digit or Erase for the item's box, by the answer box's rule (no number above 20). Any other key, a sign item and an
        /// item on another page are left alone. False when nothing changed.
        /// </summary>
        public bool Press(int index, AnswerKey key)
        {
            if (_closed || !OnThisPage(index) || IsSignItem(index))
            {
                return false;
            }

            if (key == AnswerKey.Erase)
            {
                int? before = _numbers[index].Value;
                _numbers[index].Erase();
                return before != _numbers[index].Value;
            }

            return key >= AnswerKey.Digit0 && key <= AnswerKey.Digit9 && _numbers[index].TryType(key - AnswerKey.Digit0);
        }

        /// <summary>Chooses the sign of a sign item on this page; a choice already made is replaced. False for any other item.</summary>
        public bool Choose(int index, Sign sign)
        {
            if (_closed || !OnThisPage(index) || !IsSignItem(index))
            {
                return false;
            }

            _signs[index] = sign;
            return true;
        }

        /// <summary>
        /// Records the page's answers, an item left empty as an empty answer, and goes on to the next page; after the last page
        /// closes the check with its score. If the log fails part-way (the exception comes out of here) what was written stays,
        /// and calling this again writes only what is missing, so no line is recorded twice.
        /// </summary>
        public void FinishPage()
        {
            if (_closed)
            {
                throw new InvalidOperationException("The check is already finished.");
            }

            for (int index = Math.Max(_recorded, PageStart); index < PageEnd; index++)
            {
                _log.Append(AnswerLine(index));
                _recorded = index + 1;
            }

            if (Page < PageCount - 1)
            {
                Page++;
                return;
            }

            _log.Append(CheckLine.ForClosing(CheckId, Stamp(), Score(), _items.Count));
            _closed = true;
        }

        /// <summary>The number of items answered right so far; an item left empty is wrong.</summary>
        public int Score()
        {
            int score = 0;
            for (int index = 0; index < _items.Count; index++)
            {
                if (IsCorrect(index))
                {
                    score++;
                }
            }

            return score;
        }

        private bool OnThisPage(int index)
        {
            return index >= PageStart && index < PageEnd;
        }

        private Answer? AnswerGiven(int index)
        {
            if (IsSignItem(index))
            {
                return _signs[index].HasValue ? Answer.FromSign(_signs[index].Value) : (Answer?)null;
            }

            return _numbers[index].Value.HasValue ? Answer.FromNumber(_numbers[index].Value.Value) : (Answer?)null;
        }

        private bool IsCorrect(int index)
        {
            Answer? answer = AnswerGiven(index);
            return answer.HasValue && _items[index].IsCorrect(answer.Value);
        }

        private CheckLine AnswerLine(int index)
        {
            Answer? answer = AnswerGiven(index);
            return CheckLine.ForAnswer(
                CheckId, _items[index].Key.ToString(), answer.HasValue ? answer.Value.Canonical() : string.Empty, IsCorrect(index), Stamp());
        }

        private string Stamp()
        {
            return _clock.Now.ToString("o", CultureInfo.InvariantCulture);
        }
    }
}
