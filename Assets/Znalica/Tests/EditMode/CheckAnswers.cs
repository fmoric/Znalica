using System;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    /// <summary>Fills in a check worksheet for tests.</summary>
    public static class CheckAnswers
    {
        /// <summary>Writes the right answer in the item box, or chooses its right sign, as a child would on the worksheet.</summary>
        public static void FillRight(SchoolCheckSession session, int index)
        {
            Item item = session.Items[index];
            if (item is ComparisonItem comparison)
            {
                Taken(session.Choose(index, comparison.ExpectedSign));
                return;
            }

            int solution = item is ArithmeticItem arithmetic ? arithmetic.Solution : ((NumberLineItem)item).Solution;
            Type(session, index, solution);
        }

        /// <summary>Types the number into the item box, digit by digit, after emptying it.</summary>
        public static void Type(SchoolCheckSession session, int index, int number)
        {
            while (session.Entered(index).HasValue)
            {
                session.Press(index, AnswerKey.Erase);
            }

            foreach (char digit in number.ToString())
            {
                Taken(session.Press(index, AnswerKey.Digit0 + (digit - '0')));
            }
        }

        public static void FillPageRight(SchoolCheckSession session)
        {
            for (int index = session.PageStart; index < session.PageEnd; index++)
            {
                FillRight(session, index);
            }
        }

        private static void Taken(bool taken)
        {
            if (!taken)
            {
                throw new InvalidOperationException("The worksheet refused an answer a child can give.");
            }
        }
    }
}
