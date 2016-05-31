namespace Categories
{
    using System;
    using NUnit.Framework;

    [SetUpFixture]
    public class TestSessionInitializer
    {
        [OneTimeSetUp]
        public void Setup()
        {
            Base.SessionId = DateTime.UtcNow.Ticks.ToString();
        }
    }
}