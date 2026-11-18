using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class AnswerTests
    {
        [Test]
        public void Canonical_IsTheFormTheAnswerLogKeeps()
        {
            Assert.AreEqual("13", Answer.FromNumber(13).Canonical());
            Assert.AreEqual("Less", Answer.FromSign(Sign.Less).Canonical());
            Assert.AreEqual("Greater", Answer.FromSign(Sign.Greater).Canonical());
            Assert.AreEqual("Equal", Answer.FromSign(Sign.Equal).Canonical());
            Assert.AreEqual("3,9,14", Answer.FromOrder(new[] { 3, 9, 14 }).Canonical());
            Assert.AreEqual(string.Empty, default(Answer).Canonical());
        }

        [Test]
        public void Order_CannotBeChangedThroughTheListItReturns()
        {
            Answer answer = Answer.FromOrder(new[] { 3, 9, 14 });

            Assert.IsNotInstanceOf<int[]>(answer.Order);
            Assert.Throws<System.NotSupportedException>(() => ((System.Collections.Generic.IList<int>)answer.Order)[0] = 99);
            Assert.AreEqual("3,9,14", answer.Canonical());
        }

        [Test]
        public void FromOrder_KeepsTheNumbersAsGiven_AndItsOwnCopy()
        {
            int[] numbers = { 9, 3 };
            Answer answer = Answer.FromOrder(numbers);

            numbers[0] = 0;

            CollectionAssert.AreEqual(new[] { 9, 3 }, answer.Order);
            Assert.IsNull(answer.Number);
            Assert.IsNull(answer.Sign);
        }

        [Test]
        public void SignText_IsTheSignAsDrawnOnTheButton()
        {
            Assert.AreEqual("<", ItemFormatter.SignText(Sign.Less));
            Assert.AreEqual(">", ItemFormatter.SignText(Sign.Greater));
            Assert.AreEqual("=", ItemFormatter.SignText(Sign.Equal));
        }
    }
}
