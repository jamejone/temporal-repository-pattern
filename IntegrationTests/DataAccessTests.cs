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
        public async Task GetAll()
        {
            _repo.Save(new ExampleItem());

            var allItems = await _repo.GetAllAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task TemporalGetAll_Positive()
        {
            _repo.Save(new ExampleItem());

            var allItems = await _repo.GetAllAsync(DateTime.Now);

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task CreateAndRetrieveItemFromTheDatabaseAsOf_Negative()
        {
            _repo.Save(new ExampleItem());

            var allItems = await _repo.GetAllAsync(DateTime.Now - TimeSpan.FromDays(1));

            Assert.AreEqual(0, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Positive()
        {
            _repo.Save(new ExampleItem());
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now + TimeSpan.FromDays(1));

            var allItems = await _repo.GetAllAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Positive_Keep2Versions()
        {
            _repo.Save(new ExampleItem());
            _repo.Save(new ExampleItem());
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now + TimeSpan.FromDays(1), 2);

            var allItems = await _repo.GetAllAsync();

            Assert.AreEqual(2, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Negative()
        {
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now + TimeSpan.FromDays(1));

            var allItems = await _repo.GetAllAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Negative_Keep2Versions()
        {
            _repo.Save(new ExampleItem());
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now + TimeSpan.FromDays(1), 2);

            var allItems = await _repo.GetAllAsync();

            Assert.AreEqual(2, allItems.Count());
        }
    }
}