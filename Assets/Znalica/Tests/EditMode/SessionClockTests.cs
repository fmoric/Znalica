using System;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class SessionClockTests
    {
        private static readonly DateTimeOffset Start = new DateTimeOffset(2026, 11, 21, 9, 0, 0, TimeSpan.Zero);

        private static SessionClock StartedAt(FixedClock clock)
        {
            var session = new SessionClock(clock);
            session.Start();
            return session;
        }

        [Test]
        public void BreakDue_NoProfilePickedYet_IsFalse()
        {
            var clock = new FixedClock(Start);
            var session = new SessionClock(clock);

            clock.Now = Start.AddHours(3);

            Assert.IsFalse(session.BreakDue);
        }

        [Test]
        public void BreakDue_BeforeTenMinutes_IsFalse()
        {
            var clock = new FixedClock(Start);
            SessionClock session = StartedAt(clock);

            clock.Now = Start.AddMinutes(10).AddTicks(-1);

            Assert.IsFalse(session.BreakDue);
        }

        [Test]
        public void BreakDue_AtExactlyTenMinutes_IsTrue()
        {
            var clock = new FixedClock(Start);
            SessionClock session = StartedAt(clock);

            clock.Now = Start.AddMinutes(10);

            Assert.IsTrue(session.BreakDue);
        }

        [Test]
        public void BreakDue_AfterTenMinutes_IsStillTrue()
        {
            var clock = new FixedClock(Start);
            SessionClock session = StartedAt(clock);

            clock.Now = Start.AddMinutes(25);

            Assert.IsTrue(session.BreakDue);
        }

        [Test]
        public void BreakDue_AnotherProfilePicked_CountsFromThePick()
        {
            var clock = new FixedClock(Start);
            SessionClock session = StartedAt(clock);
            clock.Now = Start.AddMinutes(12);
            Assert.IsTrue(session.BreakDue);

            session.Start();
            clock.Now = Start.AddMinutes(21);

            Assert.IsFalse(session.BreakDue, "9 minutes after the second pick");
            clock.Now = Start.AddMinutes(22);
            Assert.IsTrue(session.BreakDue, "10 minutes after the second pick");
        }

        [Test]
        public void BreakDue_ClockSetBackBeforeTheStart_IsFalse()
        {
            var clock = new FixedClock(Start);
            SessionClock session = StartedAt(clock);

            clock.Now = Start.AddHours(-1);

            Assert.IsFalse(session.BreakDue);
        }

        [Test]
        public void Constructor_NullClock_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SessionClock(null));
        }
    }
}
