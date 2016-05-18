using System;

public interface IRawPersister
{
    void Save(string typeFullName, string body);
    object Get(string typeFullName, Guid sagaId);
}