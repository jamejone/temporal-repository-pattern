using DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NUnit.Framework;
using Shared;
using WebApp;

namespace IntegrationTests
{
    public class Tests
    {
        private ConfigurationModel _config;
        MongoItemRepository _repo;

        [SetUp]
        public void Setup()
        {
            _config = ConfigurationHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);
            _repo = new MongoItemRepository(_config);
        }

        [Test]
        public void Test1()
        {
            _repo.Create(new MongoItem());

            Assert.Pass();
        }
    }
}