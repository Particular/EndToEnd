using System;
using System.Reflection;
using Newtonsoft.Json;

public class RawPersister : MarshalByRefObject, IRawPersister
{
    public void Save(string typeFullName, string body, string correlationPropertyName = null, string correlationPropertyValue = null)
    {
        var sagaDataType = Type.GetType(typeFullName);
        var sagaData = JsonConvert.DeserializeObject(body, sagaDataType);

        var persister = CreatePersisterInstance();
        var saveMethod = GetMethod(persister, nameof(Save), sagaDataType);

        saveMethod.Invoke(persister, new [] { sagaData, correlationPropertyName, correlationPropertyValue });
    }

    public void Update(string typeFullName, string body)
    {
        var sagaDataType = Type.GetType(typeFullName);
        var sagaData = JsonConvert.DeserializeObject(body, sagaDataType);

        var persister = CreatePersisterInstance();
        var updateMethod = GetMethod(persister, nameof(Update), sagaDataType);

        updateMethod.Invoke(persister, new[] { sagaData });
    }

    public object Get(string typeFullName, Guid id)
    {
        var sagaDataType = Type.GetType(typeFullName);

        var persister = CreatePersisterInstance();
        var getMethod = GetMethod(persister, nameof(Get), sagaDataType);

        var result = getMethod.Invoke(persister, new object[] { id });

        return JsonConvert.SerializeObject(result);
    }

    public object GetByCorrelationProperty(string typeFullName, string correlationPropertyName, object correlationPropertyValue)
    {
        var sagaDataType = Type.GetType(typeFullName);

        var persister = CreatePersisterInstance();
        var getMethod = GetMethod(persister, nameof(GetByCorrelationProperty), sagaDataType);

        var result = getMethod.Invoke(persister, new[] { correlationPropertyName, correlationPropertyValue });

        return JsonConvert.SerializeObject(result);
    }

    static MethodInfo GetMethod(object persister, string methodName, Type sagaDataType)
    {
        return persister.GetType().GetMethod(methodName).MakeGenericMethod(sagaDataType);
    }

    static object CreatePersisterInstance()
    {
        return Activator.CreateInstance(Type.GetType("Persister"));
    }
}