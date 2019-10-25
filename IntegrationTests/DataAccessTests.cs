using DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NUnit.Framework;
using Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApp;

namespace IntegrationTests
{
    public class Tests
    {
        private ConfigurationModel _config;
        ExampleItemRepository _repo;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _config = ConfigurationHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);
            _repo = new ExampleItemRepository(_config);
        }

        [SetUp]
        public async Task SetUp()
        {
            var testRepo = new TestTemporalRepository<ExampleItem>(_config);
            await testRepo.ClearCollectionAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            var testRepo = new TestTemporalRepository<ExampleItem>(_config);
            await testRepo.ClearCollectionAsync();
        }

        [Test]
        public async Task TestConnection()
        {
            var testRepo = new TestTemporalRepository<ExampleItem>(_config);

            await testRepo.Healthcheck();
        }

        [Test]
        public async Task CreateAndRetrieveItemFromTheDatabase()
        {
            _repo.Create(new ExampleItem());

            var allItems = await _repo.GetAllAsync();

            Assert.AreEqual(allItems.Count(), 1);

            Assert.Pass();
        }

        [Test]
        public async Task CreateAndRetrieveItemFromTheDatabaseAsOf_Positive()
        {
            _repo.Create(new ExampleItem());

            var allItems = await _repo.GetAllAsync(DateTime.Now);

            Assert.AreEqual(allItems.Count(), 1);

            Assert.Pass();
        }

        [Test]
        public async Task CreateAndRetrieveItemFromTheDatabaseAsOf_Negative()
        {
            _repo.Create(new ExampleItem());

            var allItems = await _repo.GetAllAsync(DateTime.Today - TimeSpan.FromDays(1));

            Assert.AreEqual(allItems.Count(), 0);

            Assert.Pass();
        }
    }
}