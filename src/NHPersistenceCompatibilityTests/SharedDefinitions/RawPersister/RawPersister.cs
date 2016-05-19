using System;
using Newtonsoft.Json;

public class RawPersister : MarshalByRefObject, IRawPersister
{
    public void Save(string typeFullName, string body)
    {
        var sagaDataType = Type.GetType(typeFullName);
        var sagaData = JsonConvert.DeserializeObject(body, sagaDataType);

        var persister = Activator.CreateInstance(Type.GetType("Persister"));

        var saveMethod = persister.GetType().GetMethod(nameof(Save)).MakeGenericMethod(sagaDataType);

        saveMethod.Invoke(persister, new [] { sagaData });
    }

    public object Get(string typeFullName, Guid id)
    {
        var sagaDataType = Type.GetType(typeFullName);

        var persister = Activator.CreateInstance(Type.GetType("Persister"));

        var getMethod = persister.GetType().GetMethod(nameof(Get)).MakeGenericMethod(sagaDataType);

        var result = getMethod.Invoke(persister, new object[] { id });

        var body = JsonConvert.SerializeObject(result);

        return body;
    }
}