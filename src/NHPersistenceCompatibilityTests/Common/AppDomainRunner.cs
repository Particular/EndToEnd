using System;

namespace Common
{
    public class AppDomainRunner<T>
    {
        public AppDomainRunner(AppDomainDescriptor appDomainDescriptor)
        {
            this.appDomainDescriptor = appDomainDescriptor;
        }

        public void Run(Action<T> action)
        {
            var testFacade = ResolveTestFacadeName(typeof(T));
            var appDomainEntry = (T)appDomainDescriptor.AppDomain.CreateInstanceFromAndUnwrap(appDomainDescriptor.ProjectAssemblyPath, testFacade);
            action(appDomainEntry);
        }

        private string ResolveTestFacadeName(Type type)
        {
            if (type.IsInterface && type.Name.StartsWith("I"))
            {
                return type.Name.Substring(1, type.Name.Length - 1);
            }

            throw new Exception($"Could not resolve test facade for interface {type.Name}");
        }
        
        readonly AppDomainDescriptor appDomainDescriptor;
    }
}