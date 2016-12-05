namespace Common.Tests.TestCases
{
    using System;
    using NUnit.Framework;
    using Types;

    [Explicit]
    [Ignore("Need to investigate why generic messages are not properly working with xml serialization")]
    public class TestGenericMessage : TestCase
    {
        public override Type MessageType => typeof(GenericMessage<int, string>);

        public override void Populate(object instance)
        {
            var expected = (GenericMessage<int, string>)instance;

            expected.SagaId = Guid.NewGuid();
            expected.Data1 = 1234;
            expected.Data2 = "Lorem ipsum";
        }

        public override void CheckIfAreEqual(object instanceA, object instanceB)
        {
            var expected = (GenericMessage<int, string>)instanceA;
            var result = (GenericMessage<int, string>)instanceB;

            Assert.AreEqual(expected.Data1, result.Data1);
            Assert.AreEqual(expected.Data2, result.Data2);
        }
    }
}