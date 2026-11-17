using NUnit.Framework;
using Znalica.Domain;

namespace Znalica.Tests.EditMode
{
    public class SmokeTests
    {
        [Test]
        public void NumberRange_ContainsZeroToTwenty_AndNothingOutside()
        {
            Assert.IsTrue(NumberRange.Contains(0));
            Assert.IsTrue(NumberRange.Contains(20));
            Assert.IsFalse(NumberRange.Contains(-1));
            Assert.IsFalse(NumberRange.Contains(21));
        }
    }
}
