using System;
using Common;
using Newtonsoft.Json;

namespace PersistenceCompatibilityTests
{
    public class PersisterFacade
    {
        AppDomainRunner<IRawPersister> _rawPersister;

        public PersisterFacade(AppDomainRunner<IRawPersister> rawPersister)
        {
            _rawPersister = rawPersister;
        }

        public void Save<T>(T instance, string correlationPropertyName, string correlationPropertyValue)
        {
            var body = JsonConvert.SerializeObject(instance);

            _rawPersister.Run(p => p.Save(instance.GetType().FullName, body, correlationPropertyName, correlationPropertyValue));
        }

        public void Update<T>(T instance)
        {
            var body = JsonConvert.SerializeObject(instance);

            _rawPersister.Run(p => p.Update(instance.GetType().FullName, body));
        }

        public T Get<T>(Guid sagaId)
        {
            var body = string.Empty;
            var sagaDataType = typeof (T);

            _rawPersister.Run(p => body = (string)p.Get(sagaDataType.FullName, sagaId));

            var result = JsonConvert.DeserializeObject(body, sagaDataType);

            return (T) result;
        }

        public T GetByCorrelationId<T>(string correlationPropertyName, object correlationPropertyValue)
        {
            var body = string.Empty;
            var sagaDataType = typeof(T);

            _rawPersister.Run(p => body = (string)p.GetByCorrelationProperty(sagaDataType.FullName, correlationPropertyName, correlationPropertyValue));

            var result = JsonConvert.DeserializeObject(body, sagaDataType);

            return (T)result;
        }
    }
}