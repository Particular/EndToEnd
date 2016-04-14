using System;

namespace Common
{
    using System.Messaging;

    static class QueueUtils
    {
        public static void DeleteQueuesForEndpoint(string queueName)
        {
            //main queue
            DeleteQueue(queueName);

            //retries queue
            DeleteQueue(queueName + ".retries");

            //timeout queue
            DeleteQueue(queueName + ".timeouts");

            //timeout dispatcher queue
            DeleteQueue(queueName + ".timeoutsdispatcher");
        }

        static void DeleteQueue(string queueName)
        {
            var path = $@"{Environment.MachineName}\private$\{queueName}";
            if (MessageQueue.Exists(path))
            {
                MessageQueue.Delete(path);
            }
        }
    }
}
