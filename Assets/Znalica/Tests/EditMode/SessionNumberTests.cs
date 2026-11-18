using System;
using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class SessionNumberTests
    {
        [Test]
        public void Next_EmptyLog_IsSessionOne()
        {
            Assert.AreEqual(1, SessionNumber.Next(new InMemoryAnswerLog().ReadAll()));
        }

        [Test]
        public void Next_IsOneMoreThanTheHighestNumberInTheLog_WhateverTheOrder()
        {
            var log = new InMemoryAnswerLog();
            log.Append(TestRecords.Make(sessionNumber: 2));
            log.Append(TestRecords.Make(sessionNumber: 5));
            log.Append(TestRecords.Make(sessionNumber: 3));

            Assert.AreEqual(6, SessionNumber.Next(log.ReadAll()));
        }

        [Test]
        public void Next_ManyAnswersInOneSession_DoNotAdvanceTheNumber()
        {
            var log = new InMemoryAnswerLog();
            for (int i = 0; i < 30; i++)
            {
                log.Append(TestRecords.Make(sessionNumber: 1));
            }

            Assert.AreEqual(2, SessionNumber.Next(log.ReadAll()));
        }

        [Test]
        public void Next_NoLog_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => SessionNumber.Next(null));
        }
    }
}
