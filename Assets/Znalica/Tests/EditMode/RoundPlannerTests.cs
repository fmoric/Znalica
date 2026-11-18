using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class RoundPlannerTests
    {
        private const int Session = 5;

        private static readonly ArithmeticItemSet FullSet = new ArithmeticItemSet();

        private sealed class ListItemSet : IItemSet
        {
            public ListItemSet(IReadOnlyList<Item> all)
            {
                All = all;
            }

            public Activity Activity => Activity.Arithmetic;

            public IReadOnlyList<Item> All { get; }

            public bool TryFind(ItemKey key, out Item item)
            {
                item = All.FirstOrDefault(candidate => candidate.Key.Equals(key));
                return item != null;
            }
        }

        private static ArithmeticItem Sum(int a, int b, Unknown unknown = Unknown.Result)
        {
            return new ArithmeticItem(a, Operation.Addition, b, a + b, unknown);
        }

        /// <summary>Answers the item right first time <paramref name="times"/> times, in sessions 1, 2, 3...: box <paramref name="times"/>.</summary>
        private static void RaiseTo(ItemSchedule schedule, Item item, int times)
        {
            for (int session = 1; session <= times; session++)
            {
                schedule.Apply(TestRecords.Make(sessionNumber: session, itemKey: item.Key.ToString(), isCorrect: true));
            }
        }

        /// <summary>Brings the item to <paramref name="box"/> with right answers in the sessions up to and including <paramref name="lastSession"/>.</summary>
        private static void RaiseToBoxIn(ItemSchedule schedule, Item item, int box, int lastSession)
        {
            for (int session = lastSession - box + 1; session <= lastSession; session++)
            {
                schedule.Apply(TestRecords.Make(sessionNumber: session, itemKey: item.Key.ToString(), isCorrect: true));
            }
        }

        private static void Miss(ItemSchedule schedule, Item item, int session)
        {
            schedule.Apply(TestRecords.Make(sessionNumber: session, itemKey: item.Key.ToString(), isCorrect: false));
        }

        private static List<Item> WithinTen(int count)
        {
            return FullSet.All.Where(item => item.Difficulty == Difficulty.WithinTen).Take(count).ToList();
        }

        private static IReadOnlyList<Item> PlanWith(IItemSet set, ItemSchedule schedule, int seed = 1, int session = Session)
        {
            return RoundPlanner.Plan(set, schedule, session, new SeededRandom(seed));
        }

        private static HashSet<ItemKey> Keys(IEnumerable<Item> items)
        {
            return new HashSet<ItemKey>(items.Select(item => item.Key));
        }

        /// <summary>
        /// A set of two levels: five facts within ten (fact 1+2=3 has six items, each of the others one) and eight
        /// items beyond ten.
        /// </summary>
        private static ListItemSet TwoLevels(out List<Item> factA, out List<Item> otherFacts, out List<Item> harder)
        {
            factA = new List<Item>();
            foreach (Unknown unknown in new[] { Unknown.Result, Unknown.First, Unknown.Second })
            {
                factA.Add(Sum(1, 2, unknown));
                factA.Add(Sum(2, 1, unknown));
            }

            otherFacts = new List<Item> { Sum(1, 1), Sum(1, 3), Sum(1, 4), Sum(1, 5) };
            harder = new List<Item>();
            for (int b = 0; b <= 7; b++)
            {
                harder.Add(Sum(12, b));
            }

            return new ListItemSet(factA.Concat(otherFacts).Concat(harder).ToList());
        }

        [Test]
        public void Plan_WithNothingAnswered_GivesTenDistinctItemsOfTheSet_FromTheEasiestLevel()
        {
            IReadOnlyList<Item> round = PlanWith(FullSet, new ItemSchedule(), seed: 1, session: 1);

            Assert.AreEqual(RoundPlanner.RoundSize, round.Count);
            Assert.AreEqual(round.Count, round.Select(item => item.Key).Distinct().Count(), "no item twice");
            Assert.IsTrue(round.All(item => FullSet.TryFind(item.Key, out _)));
            Assert.IsTrue(round.All(item => item.Difficulty == Difficulty.WithinTen));
        }

        [Test]
        public void Plan_SetWithFewerThanTenItems_GivesThemAll()
        {
            var set = new ListItemSet(new List<Item> { Sum(1, 1), Sum(1, 2), Sum(1, 3) });

            Assert.AreEqual(3, PlanWith(set, new ItemSchedule()).Count);
        }

        [Test]
        public void Plan_EmptySet_GivesNoItems()
        {
            Assert.IsEmpty(PlanWith(new ListItemSet(new List<Item>()), new ItemSchedule()));
        }

        [Test]
        public void Plan_NewItemsStayInTheCurrentLevel_EvenWhenItHasTooFew()
        {
            // Three easy items and plenty beyond ten: nothing is promoted, so the round is the three easy items.
            var easy = new List<Item> { Sum(1, 2), Sum(2, 2), Sum(3, 2) };
            var harder = Enumerable.Range(0, 8).Select(b => (Item)Sum(12, b)).ToList();

            IReadOnlyList<Item> round = PlanWith(new ListItemSet(easy.Concat(harder).ToList()), new ItemSchedule());

            Assert.AreEqual(3, round.Count);
            Assert.IsTrue(round.All(item => item.Difficulty == Difficulty.WithinTen));
        }

        [Test]
        public void Plan_SameSeedGivesTheSameRound_DifferentSeedsGiveDifferentOnes()
        {
            var schedule = new ItemSchedule();
            foreach (Item item in WithinTen(20))
            {
                Miss(schedule, item, 1);
            }

            string Round(int seed) => string.Join(" ", PlanWith(FullSet, schedule, seed, session: 2).Select(item => item.Key.ToString()));

            Assert.AreEqual(Round(5), Round(5));
            Assert.AreNotEqual(Round(5), Round(6));
        }

        [Test]
        public void Plan_Shuffles_SoTheOrderIsNotTheSetsOrder()
        {
            int sorted = 0;
            for (int seed = 0; seed < 50; seed++)
            {
                List<string> keys = PlanWith(FullSet, new ItemSchedule(), seed).Select(item => item.Key.ToString()).ToList();
                if (keys.SequenceEqual(keys.OrderBy(key => key, System.StringComparer.Ordinal)))
                {
                    sorted++;
                }
            }

            Assert.Less(sorted, 5, "ten shuffled keys are almost never already in key order");
        }

        [Test]
        public void Plan_ShufflesDueAndNewTogether_SoTheDueItemsAreNotAlwaysFirst()
        {
            var schedule = new ItemSchedule();
            List<Item> due = WithinTen(6);
            foreach (Item item in due)
            {
                Miss(schedule, item, 1);
            }

            HashSet<ItemKey> dueKeys = Keys(due);
            int dueAllFirst = 0;
            for (int seed = 0; seed < 50; seed++)
            {
                IReadOnlyList<Item> round = PlanWith(FullSet, schedule, seed, session: 2);
                if (round.Take(6).All(item => dueKeys.Contains(item.Key)))
                {
                    dueAllFirst++;
                }
            }

            // Without the final shuffle the six due items fill the first six places every time (50 of 50);
            // shuffled, that happens by chance about 1 time in 210.
            Assert.Less(dueAllFirst, 10);
        }

        [Test]
        public void Plan_AFirstTimeWrongItem_IsDueNextSession_AndPlannedWithinTheFirstTwoRounds()
        {
            // Twelve items missed in session 1: the most the first two rounds of session 2 can hold (6 due each).
            var schedule = new ItemSchedule();
            List<Item> missed = WithinTen(12);
            foreach (Item item in missed)
            {
                Miss(schedule, item, 1);
            }

            HashSet<ItemKey> missedKeys = Keys(missed);

            // Session 2, round 1: six of them. The round is answered right first time, which moves them on.
            IReadOnlyList<Item> first = PlanWith(FullSet, schedule, seed: 3, session: 2);
            Assert.AreEqual(RoundPlanner.DueCap, first.Count(item => missedKeys.Contains(item.Key)));
            foreach (Item item in first)
            {
                schedule.Apply(TestRecords.Make(sessionNumber: 2, itemKey: item.Key.ToString(), isCorrect: true));
            }

            // Session 2, round 2: the six that were left.
            IReadOnlyList<Item> second = PlanWith(FullSet, schedule, seed: 4, session: 2);

            Assert.AreEqual(RoundPlanner.DueCap, second.Count(item => missedKeys.Contains(item.Key)));
            Assert.AreEqual(12, Keys(first.Concat(second)).Count(missedKeys.Contains), "all twelve in two rounds");
        }

        [Test]
        public void Plan_AMissedItem_IsNotDueInTheSessionItWasMissed()
        {
            var schedule = new ItemSchedule();
            List<Item> items = WithinTen(11);

            // Items 0-2: box 2, due now (session 4). Items 3-10: missed in session 4 itself, box 0, due in session 5.
            for (int i = 0; i < 3; i++)
            {
                RaiseTo(schedule, items[i], 2);
            }

            for (int i = 3; i < 11; i++)
            {
                Miss(schedule, items[i], 4);
            }

            IReadOnlyList<Item> round = PlanWith(FullSet, schedule, session: 4);

            // Were the eight missed ones due, their box 0 would take all six due places ahead of the three box 2 items.
            HashSet<ItemKey> inRound = Keys(round);
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(inRound.Contains(items[i].Key), "due item " + i);
            }

            Assert.AreEqual(5, schedule.Get(items[3].Key).Value.DueSession);
        }

        [Test]
        public void Plan_WithFewScheduledItems_TopsTheRoundUpWithNewItems()
        {
            var schedule = new ItemSchedule();
            List<Item> notDue = WithinTen(2);
            foreach (Item item in notDue)
            {
                RaiseToBoxIn(schedule, item, 2, Session - 1); // answered last session, due in session 6: fill now
            }

            IReadOnlyList<Item> round = PlanWith(FullSet, schedule);

            Assert.AreEqual(RoundPlanner.RoundSize, round.Count);
            Assert.AreEqual(8, round.Count(item => !schedule.Get(item.Key).HasValue), "two fill, eight new: four, then the rest");
        }

        [Test]
        public void Plan_PlansAtMostSixDueItems_AndFillsTheRestWithFourNew()
        {
            var schedule = new ItemSchedule();
            List<Item> due = WithinTen(9);
            foreach (Item item in due)
            {
                Miss(schedule, item, 1);
            }

            IReadOnlyList<Item> round = PlanWith(FullSet, schedule, session: 2);

            HashSet<ItemKey> dueKeys = Keys(due);
            Assert.AreEqual(RoundPlanner.RoundSize, round.Count);
            Assert.AreEqual(6, round.Count(item => dueKeys.Contains(item.Key)), "the cap of 6 due");
            Assert.AreEqual(4, round.Count(item => !dueKeys.Contains(item.Key)), "the other four are new");
        }

        [Test]
        public void Plan_DueItemsLowestBoxFirst()
        {
            var schedule = new ItemSchedule();
            List<Item> items = WithinTen(8);

            // Items 0-3: box 2, due in session 4 (right in sessions 1 and 2). Items 4-7: box 0, due in session 4 (missed in 3).
            for (int i = 0; i < 4; i++)
            {
                RaiseTo(schedule, items[i], 2);
            }

            for (int i = 4; i < 8; i++)
            {
                Miss(schedule, items[i], 3);
            }

            HashSet<ItemKey> inRound = Keys(PlanWith(FullSet, schedule, session: 4));

            for (int i = 4; i < 8; i++)
            {
                Assert.IsTrue(inRound.Contains(items[i].Key), "box 0 item " + i + " leads");
            }

            Assert.AreEqual(2, Enumerable.Range(0, 4).Count(i => inRound.Contains(items[i].Key)), "two box 2 items fill the cap of 6");
        }

        [Test]
        public void Plan_WithinTheSameBox_TheLongestDueGoesFirst()
        {
            var schedule = new ItemSchedule();
            List<Item> items = WithinTen(8);
            for (int i = 0; i < 4; i++)
            {
                Miss(schedule, items[i], 3); // due in 4
            }

            for (int i = 4; i < 8; i++)
            {
                Miss(schedule, items[i], 1); // due in 2: longer due
            }

            HashSet<ItemKey> inRound = Keys(PlanWith(FullSet, schedule, session: 6));

            for (int i = 4; i < 8; i++)
            {
                Assert.IsTrue(inRound.Contains(items[i].Key), "longest due item " + i);
            }

            Assert.AreEqual(2, Enumerable.Range(0, 4).Count(i => inRound.Contains(items[i].Key)));
        }

        [Test]
        public void Plan_AtMostFourNewItems_WhenThereIsEnoughToFillWith()
        {
            var schedule = new ItemSchedule();
            List<Item> notDue = WithinTen(8);
            foreach (Item item in notDue)
            {
                RaiseToBoxIn(schedule, item, 2, Session - 1); // due in session 6: not yet due, and not answered in this session
            }

            IReadOnlyList<Item> round = PlanWith(FullSet, schedule);

            HashSet<ItemKey> known = Keys(notDue);
            Assert.AreEqual(RoundPlanner.RoundSize, round.Count);
            Assert.AreEqual(RoundPlanner.NewCap, round.Count(item => !known.Contains(item.Key)), "four new");
            Assert.AreEqual(6, round.Count(item => known.Contains(item.Key)), "six of the eight not-yet-due items fill");
        }

        [Test]
        public void Plan_FillsWithNotYetDueItems_LowestBoxFirst()
        {
            var schedule = new ItemSchedule();
            List<Item> items = WithinTen(9);

            // Three each in box 2, 3 and 4, the last right answer in session 4: not yet due in session 5 (box 1 is due the
            // session after its answer, so it is never fill).
            for (int i = 0; i < 9; i++)
            {
                int box = i / 3 + 2;
                RaiseToBoxIn(schedule, items[i], box, Session - 1);
                Assert.AreEqual(box, schedule.Get(items[i].Key).Value.Box);
                Assert.Greater(schedule.Get(items[i].Key).Value.DueSession, Session);
            }

            HashSet<ItemKey> inRound = Keys(PlanWith(FullSet, schedule));

            for (int i = 0; i < 6; i++)
            {
                Assert.IsTrue(inRound.Contains(items[i].Key), "box " + (i / 3 + 2) + " item fills");
            }

            for (int i = 6; i < 9; i++)
            {
                Assert.IsFalse(inRound.Contains(items[i].Key), "box 4 items are left out");
            }
        }

        [Test]
        public void Plan_ADueItemOverTheCap_IsNotUsedAsFill()
        {
            // Eight due items and a set with no other item: six are planned, the two over the cap are not fill.
            var schedule = new ItemSchedule();
            List<Item> items = WithinTen(8);
            foreach (Item item in items)
            {
                Miss(schedule, item, 1);
            }

            IReadOnlyList<Item> round = PlanWith(new ListItemSet(items), schedule, session: 2);

            Assert.AreEqual(RoundPlanner.DueCap, round.Count);
        }

        [Test]
        public void Plan_TiesAreBrokenAtRandom_AmongDueItemsAndAmongNewOnes()
        {
            var schedule = new ItemSchedule();
            foreach (Item item in WithinTen(20))
            {
                Miss(schedule, item, 1);
            }

            var dueChosen = new HashSet<string>();
            var newChosen = new HashSet<string>();
            for (int seed = 0; seed < 30; seed++)
            {
                dueChosen.Add(string.Join(",", PlanWith(FullSet, schedule, seed, session: 2)
                    .Where(item => schedule.Get(item.Key).HasValue).Select(item => item.Key.ToString()).OrderBy(k => k, System.StringComparer.Ordinal)));
                newChosen.Add(string.Join(",", PlanWith(FullSet, new ItemSchedule(), seed, session: 1)
                    .Select(item => item.Key.ToString()).OrderBy(k => k, System.StringComparer.Ordinal)));
            }

            Assert.Greater(dueChosen.Count, 20, "which six of the twenty equal due items: not always the same");
            Assert.Greater(newChosen.Count, 20, "which ten of the new items: not always the same");
        }

        [Test]
        public void Plan_AKeyTheSetCannotBuild_IsSkipped()
        {
            var schedule = new ItemSchedule();
            schedule.Apply(TestRecords.Make(sessionNumber: 1, itemKey: "C:7?12", isCorrect: false));
            schedule.Apply(TestRecords.Make(sessionNumber: 1, itemKey: "A:3-?=7", isCorrect: false));

            IReadOnlyList<Item> round = PlanWith(FullSet, schedule, session: 2);

            Assert.AreEqual(RoundPlanner.RoundSize, round.Count);
            Assert.IsFalse(round.Any(item => item.Key.ToString() == "C:7?12" || item.Key.ToString() == "A:3-?=7"));
        }

        [Test]
        public void Plan_PlansTheSameRound_FromAScheduleBuiltFromALogOrAppliedAnswerByAnswer()
        {
            var log = new List<AnswerRecord>();
            int n = 0;
            foreach (Item item in WithinTen(15))
            {
                log.Add(TestRecords.Make(sessionNumber: 1, itemKey: item.Key.ToString(), isCorrect: n++ % 2 == 0));
            }

            var applied = new ItemSchedule();
            foreach (AnswerRecord record in log)
            {
                applied.Apply(record);
            }

            string Round(ItemSchedule schedule) => string.Join(" ", PlanWith(FullSet, schedule, seed: 9, session: 2).Select(item => item.Key.ToString()));

            Assert.AreEqual(Round(applied), Round(ItemSchedule.Build(log)));
        }

        [Test]
        public void CurrentLevel_StaysBelowEightyPercentOfFactsKnown_AndMovesAtEighty()
        {
            ListItemSet set = TwoLevels(out List<Item> factA, out List<Item> otherFacts, out _);
            var schedule = new ItemSchedule();
            RaiseTo(schedule, factA[0], 3);
            RaiseTo(schedule, otherFacts[0], 3);
            RaiseTo(schedule, otherFacts[1], 3);

            Assert.AreEqual(Difficulty.WithinTen, RoundPlanner.CurrentLevel(set, schedule), "3 of 5 facts");

            RaiseTo(schedule, otherFacts[2], 3);

            Assert.AreEqual(Difficulty.BeyondTenWithoutCrossing, RoundPlanner.CurrentLevel(set, schedule), "4 of 5 facts is exactly 80%");
        }

        [Test]
        public void CurrentLevel_JustBelowEightyPercent_DoesNotMove()
        {
            // Ten facts within ten (0 + 0 .. 0 + 9, with the whole 0 .. 9) and one beyond: 7 of 10 stays, 8 of 10 moves.
            var easy = Enumerable.Range(0, 10).Select(b => (Item)Sum(0, b)).ToList();
            var set = new ListItemSet(easy.Concat(new List<Item> { Sum(12, 3) }).ToList());
            var schedule = new ItemSchedule();
            for (int i = 0; i < 7; i++)
            {
                RaiseTo(schedule, easy[i], 3);
            }

            Assert.AreEqual(Difficulty.WithinTen, RoundPlanner.CurrentLevel(set, schedule), "70%");

            RaiseTo(schedule, easy[7], 3);

            Assert.AreEqual(Difficulty.BeyondTenWithoutCrossing, RoundPlanner.CurrentLevel(set, schedule), "80%");
        }

        [Test]
        public void CurrentLevel_AFactIsKnownWhenAnyOfItsItemsIsInBoxThreeOrHigher()
        {
            ListItemSet set = TwoLevels(out List<Item> factA, out List<Item> otherFacts, out _);
            var schedule = new ItemSchedule();
            RaiseTo(schedule, factA[0], 3); // one of the six items of fact A
            RaiseTo(schedule, otherFacts[0], 3);
            RaiseTo(schedule, otherFacts[1], 3);
            RaiseTo(schedule, otherFacts[2], 3);

            Assert.AreEqual(Difficulty.BeyondTenWithoutCrossing, RoundPlanner.CurrentLevel(set, schedule));
        }

        [Test]
        public void CurrentLevel_CountsFactsNotItems()
        {
            ListItemSet set = TwoLevels(out List<Item> factA, out List<Item> otherFacts, out _);
            var schedule = new ItemSchedule();
            foreach (Item item in factA)
            {
                RaiseTo(schedule, item, 3);
            }

            RaiseTo(schedule, otherFacts[0], 3);
            RaiseTo(schedule, otherFacts[1], 3);

            // 8 of 10 items are in box 3, but only 3 of 5 facts.
            Assert.AreEqual(Difficulty.WithinTen, RoundPlanner.CurrentLevel(set, schedule));
        }

        [Test]
        public void CurrentLevel_BoxTwoIsNotKnown()
        {
            ListItemSet set = TwoLevels(out List<Item> factA, out List<Item> otherFacts, out _);
            var schedule = new ItemSchedule();
            RaiseTo(schedule, factA[0], 2);
            foreach (Item item in otherFacts)
            {
                RaiseTo(schedule, item, 2);
            }

            Assert.AreEqual(Difficulty.WithinTen, RoundPlanner.CurrentLevel(set, schedule));
        }

        [Test]
        public void CurrentLevel_WithEveryLevelPromoted_IsTheHardest_AndNullForAnEmptySet()
        {
            var set = new ListItemSet(new List<Item> { Sum(1, 1), Sum(12, 3) });
            var schedule = new ItemSchedule();
            RaiseTo(schedule, set.All[0], 3);
            RaiseTo(schedule, set.All[1], 3);

            Assert.AreEqual(Difficulty.BeyondTenWithoutCrossing, RoundPlanner.CurrentLevel(set, schedule));
            Assert.IsNull(RoundPlanner.CurrentLevel(new ListItemSet(new List<Item>()), new ItemSchedule()));
        }

        [Test]
        public void Plan_AfterPromotion_NewItemsComeFromTheNextLevelOnly()
        {
            ListItemSet set = TwoLevels(out List<Item> factA, out List<Item> otherFacts, out _);
            var schedule = new ItemSchedule();
            RaiseTo(schedule, factA[0], 3);
            foreach (Item item in otherFacts)
            {
                RaiseTo(schedule, item, 3);
            }

            // All five easy facts are known (box 3, due in session 7); the planning session is 4.
            IReadOnlyList<Item> round = PlanWith(set, schedule, session: 4);

            Assert.GreaterOrEqual(round.Count(item => item.Difficulty == Difficulty.BeyondTenWithoutCrossing), RoundPlanner.NewCap,
                "the new items come from the next level");
            Assert.IsFalse(round.Any(item => item.Difficulty == Difficulty.WithinTen && !schedule.Get(item.Key).HasValue),
                "an easy item never answered does not come as new once the level is promoted");
        }

        [Test]
        public void Plan_TwoRoundsInOneSession_NeverAskAnItemTwice_AndNoItemClimbsTwo()
        {
            var schedule = new ItemSchedule();
            var log = new InMemoryAnswerLog();
            var clock = new FixedClock(new System.DateTimeOffset(2026, 11, 17, 10, 30, 0, System.TimeSpan.FromHours(1)));

            HashSet<ItemKey> PlayRound(int seed, string roundId)
            {
                IReadOnlyList<Item> planned = RoundPlanner.Plan(FullSet, schedule, 1, new SeededRandom(seed));
                var session = new RoundSession(planned, log, schedule, clock, 1, roundId);
                while (session.State != RoundState.Ended)
                {
                    session.Submit(Answer.FromNumber(((ArithmeticItem)session.CurrentItem).Solution), 1000);
                }

                return Keys(planned);
            }

            HashSet<ItemKey> first = PlayRound(1, "round-1");
            HashSet<ItemKey> second = PlayRound(2, "round-2");

            Assert.AreEqual(RoundPlanner.RoundSize, second.Count);
            Assert.IsFalse(second.Overlaps(first), "no item of round 1 comes again in the same session");
            Assert.IsTrue(schedule.Entries.All(entry => entry.Value.Box == 1), "every item answered moved once: box 1, none in box 2");
        }

        [Test]
        public void Plan_BeforePromotion_NewItemsComeFromTheCurrentLevelOnly()
        {
            ListItemSet set = TwoLevels(out List<Item> factA, out _, out _);
            var schedule = new ItemSchedule();
            RaiseTo(schedule, factA[0], 3);

            IReadOnlyList<Item> round = PlanWith(set, schedule, session: 4);

            Assert.IsTrue(round.All(item => item.Difficulty == Difficulty.WithinTen));
        }
    }
}
