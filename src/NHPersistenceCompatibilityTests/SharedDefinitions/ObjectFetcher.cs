using System;
using System.Collections.Generic;


static class ObjectFetcher
{
    public static void Traverse(object instance, Type instanceType)
    {
        foreach (var propertyInfo in instanceType.GetProperties())
        {
            if (ReferenceEquals(propertyInfo.DeclaringType, typeof(object)))
            {
                continue;
            }

            if (propertyInfo.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null)
            {
                propertyInfo.GetValue(instance, null)?.ToString();
            }
            else
            {
                var propertyValue = propertyInfo.GetValue(instance, null);

                if (propertyValue != null)
                {
                    Traverse(propertyValue, propertyInfo.PropertyType);
                }
            }
        }
    }
}
