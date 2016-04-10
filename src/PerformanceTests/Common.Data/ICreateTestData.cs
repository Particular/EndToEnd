#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif

namespace Common
{
    public interface ICreateTestData
    {
        void CreateTestData(Configuration configuration);

        void CleanUpTestData(Configuration configuration);
    }
}
