using GatePassVR.Core;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class CarryAlongTests
    {
        private static readonly string[] Security = { "S3-1", "S3-2", "S3-3" };

        [Test]
        public void ShouldBring_SecurityStep_DoesNotBring()
        {
            Assert.IsFalse(CarryAlong.ShouldBring("S3-1", Security));
        }

        [Test]
        public void ShouldBring_BoardingStep_Brings()
        {
            Assert.IsTrue(CarryAlong.ShouldBring("S4-1", Security));
        }

        [Test]
        public void ShouldBring_UnknownStepOrNoList_Brings()
        {
            Assert.IsTrue(CarryAlong.ShouldBring(null, Security));
            Assert.IsTrue(CarryAlong.ShouldBring("S3-1", null));
        }
    }
}
