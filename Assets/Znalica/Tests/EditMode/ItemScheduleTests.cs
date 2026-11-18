using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class ItemScheduleTests
    {
        private static ItemKey Key(string text)
        {
            Assert.IsTrue(ItemKey.TryParse(text, out ItemKey key));
            return key;
        }

        private static AnswerRecord Answer(int session, bool correct, AttemptKind attempt = AttemptKind.First, string key = "A:8+5=?")
        {
            return TestRecords.Make(sessionNumber: session, itemKey: key, isCorrect: correct, attempt: attempt);
        }

        [Test]
        public void AnItemNeverAnswered_IsNew()
        {
            var schedule = new ItemSchedule();

            Assert.IsNull(schedule.Get(Key("A:8+5=?")));
        }

        [Test]
        public void FirstAttemptWrong_IsBoxZero_AndDueNextSession()
        {
            var schedule = new ItemSchedule();

            schedule.Apply(Answer(session: 4, correct: false));

            ItemProgress progress = schedule.Get(Key("A:8+5=?")).Value;
            Assert.AreEqual(0, progress.Box);
            Assert.AreEqual(5, progress.DueSession);
        }

        [Test]
        public void RightFirstTime_RaisesTheBox_AndTheIntervalsLengthen()
        {
            var schedule = new ItemSchedule();
            // The sessions in which each right answer is given, and what the schedule must then say: box and due session.
            int session = 1;
            int[] expectedBox = { 1, 2, 3, 4, 4 };
            int[] expectedInterval = { 1, 2, 4, 8, 8 };

            for (int step = 0; step < expectedBox.Length; step++)
            {
                schedule.Apply(Answer(session, correct: true));

                ItemProgress progress = schedule.Get(Key("A:8+5=?")).Value;
                Assert.AreEqual(expectedBox[step], progress.Box, "box after right answer " + (step + 1));
                Assert.AreEqual(session + expectedInterval[step], progress.DueSession, "due after right answer " + (step + 1));
                session = progress.DueSession;
            }
        }

        [Test]
        public void AMissAfterRightAnswers_GoesBackToBoxZero()
        {
            var schedule = new ItemSchedule();
            schedule.Apply(Answer(1, true));
            schedule.Apply(Answer(2, true));

            schedule.Apply(Answer(4, false));

            Assert.AreEqual(0, schedule.Get(Key("A:8+5=?")).Value.Box);
            Assert.AreEqual(5, schedule.Get(Key("A:8+5=?")).Value.DueSession);
        }

        [Test]
        public void RetryAndReturn_DoNotMoveAnItem()
        {
            var schedule = new ItemSchedule();
            schedule.Apply(Answer(2, false));
            ItemProgress before = schedule.Get(Key("A:8+5=?")).Value;

            schedule.Apply(Answer(2, true, AttemptKind.Retry));
            schedule.Apply(Answer(2, true, AttemptKind.Return));
            schedule.Apply(Answer(3, false, AttemptKind.Retry));

            Assert.AreEqual(before.Box, schedule.Get(Key("A:8+5=?")).Value.Box);
            Assert.AreEqual(before.DueSession, schedule.Get(Key("A:8+5=?")).Value.DueSession);
        }

        [Test]
        public void ARetryOrReturnOfAnItemNeverAskedFirst_LeavesItNew()
        {
            var schedule = new ItemSchedule();

            schedule.Apply(Answer(1, true, AttemptKind.Return));

            Assert.IsNull(schedule.Get(Key("A:8+5=?")));
        }

        [Test]
        public void ItemsAreIndependent()
        {
            var schedule = new ItemSchedule();

            schedule.Apply(Answer(1, false, key: "A:8+5=?"));
            schedule.Apply(Answer(1, true, key: "A:8+?=13"));

            Assert.AreEqual(0, schedule.Get(Key("A:8+5=?")).Value.Box);
            Assert.AreEqual(1, schedule.Get(Key("A:8+?=13")).Value.Box);
        }

        [Test]
        public void ARecordWithAKeyThatIsNotAKey_IsSkipped()
        {
            var schedule = new ItemSchedule();

            schedule.Apply(Answer(1, false, key: "A:8 + 5"));

            Assert.IsEmpty(schedule.Entries);
        }

        [Test]
        public void BuiltFromALog_EqualsOneAppliedAnswerByAnswer()
        {
            var log = new List<AnswerRecord>
            {
                Answer(1, false, key: "A:1+1=?"),
                Answer(1, true, AttemptKind.Retry, "A:1+1=?"),
                Answer(1, true, key: "A:2+1=?"),
                Answer(1, false, key: "A:3+1=?"),
                Answer(1, true, AttemptKind.Return, "A:3+1=?"),
                Answer(2, true, key: "A:1+1=?"),
                Answer(2, true, key: "A:2+1=?"),
                Answer(3, false, key: "A:2+1=?"),
                Answer(4, true, key: "A:2+1=?"),
            };
            var applied = new ItemSchedule();
            foreach (AnswerRecord record in log)
            {
                applied.Apply(record);
            }

            ItemSchedule built = ItemSchedule.Build(log);

            string Dump(ItemSchedule s) => string.Join(";", s.Entries
                .OrderBy(e => e.Key.ToString(), System.StringComparer.Ordinal)
                .Select(e => e.Key + "=" + e.Value.Box + "/" + e.Value.DueSession));
            Assert.AreEqual(3, built.Entries.Count());
            Assert.AreEqual(Dump(applied), Dump(built));
            Assert.AreEqual("A:1+1=?=1/3;A:2+1=?=1/5;A:3+1=?=0/2", Dump(built));
        }

        [Test]
        public void Apply_RemembersTheSessionOfTheAnswerThatMovedTheItem()
        {
            var schedule = new ItemSchedule();

            schedule.Apply(Answer(session: 3, correct: false));
            Assert.AreEqual(3, schedule.Get(Key("A:8+5=?")).Value.LastSession);

            schedule.Apply(Answer(session: 6, correct: true));
            Assert.AreEqual(6, schedule.Get(Key("A:8+5=?")).Value.LastSession);

            schedule.Apply(Answer(session: 9, correct: true, attempt: AttemptKind.Retry));
            Assert.AreEqual(6, schedule.Get(Key("A:8+5=?")).Value.LastSession, "a Retry does not move the item");
        }
    }
}
