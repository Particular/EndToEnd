using System;

public interface IRawPersister
{
    void Save(string typeFullName, string body, string correlationPropertyName, string correlationPropertyValue);
    void Update(string typeFullName, string body);
    object Get(string typeFullName, Guid sagaId);
    object GetByCorrelationProperty(string typeFullName, string correlationPropertyName, object correlationPropertyValue);
}