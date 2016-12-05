using System;
using System.IO;
using System.Linq;
using Common;
using Common.Tests;

class Tester : MarshalByRefObject, ISerializationTester
{
    public void Serialize(Type testCaseType, SerializationFormat format, string filePath)
    {
        try
        {
            var testCase = (TestCase)Activator.CreateInstance(testCaseType);

            var serializer = CreateSerializer(format, testCase.MessageType);

            var testMessage = CreateTestMessage(serializer, testCase);

            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, testMessage);

                memoryStream.Position = 0;

                using (var writer = new FileStream(filePath, FileMode.Create))
                {
                    memoryStream.CopyTo(writer);

                    writer.Flush(true);
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public void Verify(Type testCaseType, SerializationFormat format, string filePath)
    {
        try
        {
            var testCase = (TestCase)Activator.CreateInstance(testCaseType);

            var serializer = CreateSerializer(format, testCase.MessageType);

            var testMessage = CreateTestMessage(serializer, testCase);

            using (var fileStream = new FileInfo(filePath).OpenRead())
            {
                var deserializedMessage = serializer.Deserialize(fileStream).First();

                testCase.CheckIfAreEqual(testMessage, deserializedMessage);
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    ISerializerFacade CreateSerializer(SerializationFormat format, Type messageType)
    {
        return format == SerializationFormat.Xml ? (ISerializerFacade)new XmlSerializerFacade(messageType) : new JsonSerializerFacade(messageType);
    }

    static object CreateTestMessage(ISerializerFacade serializer, TestCase testCase)
    {
        var instance = serializer.CreateInstance(testCase.MessageType);

        testCase.Populate(instance);

        return instance;
    }
}