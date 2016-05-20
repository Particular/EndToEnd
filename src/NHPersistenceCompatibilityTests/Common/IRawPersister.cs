using System;

public interface IRawPersister
{
    void Save(string typeFullName, string body, string correlationPropertyName, string correlationPropertyValue);
    object Get(string typeFullName, Guid sagaId);
    object GetByCorrelationProperty(string typeFullName, string correlationPropertyName, object correlationPropertyValue);
}