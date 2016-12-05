namespace Common.Tests.TestCases.Types
{
    using System;

    [Serializable]
    public class GenericMessage<T1, T2>
    {
        public GenericMessage()
        {
        }

        public GenericMessage(Guid sagaId, T1 data1, T2 data2)
        {
            SagaId = sagaId;
            Data1 = data1;
            Data2 = data2;
        }

        public Guid SagaId { get; set; }

        public T1 Data1 { get; set; }

        public T2 Data2 { get; set; }
    }
}