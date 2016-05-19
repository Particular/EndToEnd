using System;
using Common;
using Newtonsoft.Json;

namespace PersistenceCompatibilityTests
{
    public class PersisterFacade
    {
        readonly AppDomainRunner<IRawPersister> _rawPersister;

        public PersisterFacade(AppDomainRunner<IRawPersister> rawPersister)
        {
            _rawPersister = rawPersister;
        }

        public void Save<T>(T instance, string correlationPropertyName, string correlationPropertyValue)
        {
            var body = JsonConvert.SerializeObject(instance);

            _rawPersister.Run(p => p.Save(instance.GetType().FullName, body, correlationPropertyName, correlationPropertyValue));
        }

        public T Get<T>(Guid sagaId)
        {
            string body = string.Empty;
            Type sagaDataType = typeof (T);

            _rawPersister.Run(p => body = (string)p.Get(sagaDataType.FullName, sagaId));

            var result = JsonConvert.DeserializeObject(body, sagaDataType);

            return (T) result;
        }
    }
}