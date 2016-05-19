using System;
using System.Collections.Generic;

namespace PersistenceCompatibilityTests
{
    public interface ITestPersistence
    {
        void Persist(Guid id, string originator);
        void Verify(Guid id, string originator);


        void Persist(Guid id, IList<int> data, string originator);
        void Verify(Guid id, IList<int> ints, string originator);

        void Persist(Guid id, string compositeValue, string originator);

        void Verify(Guid id, string compositeValue, string originator);
    }
}