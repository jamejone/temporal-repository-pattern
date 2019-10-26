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
        TestTemporalRepository<ExampleItem> _testRepo;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _config = ConfigurationHelper.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);
            _repo = new ExampleItemRepository(_config);
            _testRepo = new TestTemporalRepository<ExampleItem>(_config);
        }

        [SetUp]
        public async Task SetUp()
        {
            await _testRepo.ClearCollectionAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            await _testRepo.ClearCollectionAsync();
        }

        [Test]
        public async Task TestConnection()
        {
            var testRepo = new TestTemporalRepository<ExampleItem>(_config);

            await testRepo.Healthcheck();
        }

        [Test]
        public async Task Get()
        {
            _repo.Save(new ExampleItem() { Identifier = "abc" });

            var item = await _repo.Get("abc");

            Assert.IsNotNull(item);
            Assert.AreEqual("abc", item.Identifier);
        }

        [Test]
        public async Task Get_GetsLatest()
        {
            _repo.Save(new ExampleItem() { Identifier = "abc", Payload = "123" });
            _repo.Save(new ExampleItem() { Identifier = "abc", Payload = "456" });

            var item = await _repo.Get("abc");

            Assert.IsNotNull(item);
            Assert.AreEqual("abc", item.Identifier);
            Assert.AreEqual("456", item.Payload);
        }

        [Test]
        public async Task Get_Temporal_GetsCorrectEntity()
        {
            _repo.Save(new ExampleItem() { Identifier = "abc", Payload = "123" });
            await Task.Delay(2000);
            _repo.Save(new ExampleItem() { Identifier = "abc", Payload = "456" });

            var item = await _repo.Get("abc", DateTime.Now - TimeSpan.FromMilliseconds(1500));

            Assert.IsNotNull(item);
            Assert.AreEqual("abc", item.Identifier);
            Assert.AreEqual("123", item.Payload);
        }

        [Test]
        public async Task Get_Temporal_Negative()
        {
            _repo.Save(new ExampleItem() { Identifier = "abc", Payload = "123" });

            var item = await _repo.Get("abc", DateTime.Now - TimeSpan.FromDays(1));

            Assert.IsNull(item);
        }

        [Test]
        public async Task GetAll_Positive()
        {
            _repo.Save(new ExampleItem());

            var allItems = await _repo.GetAllAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_Positive()
        {
            _repo.Save(new ExampleItem());

            var allItems = await _repo.GetAllAsync(DateTime.Now);

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_Negative()
        {
            _repo.Save(new ExampleItem());

            var allItems = await _repo.GetAllAsync(DateTime.Now - TimeSpan.FromDays(1));

            Assert.AreEqual(0, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_GetsCorrectEntity()
        {
            _repo.Save(new ExampleItem() { Payload = "abc" });
            await Task.Delay(2000);
            _repo.Save(new ExampleItem() { Payload = "def" });

            var allItems = await _repo.GetAllAsync(DateTime.Now - TimeSpan.FromMilliseconds(1500));

            Assert.AreEqual(1, allItems.Count());
            Assert.AreEqual("abc", allItems.First().Payload);
        }

        [Test]
        public async Task PurgeOldRecords_Positive()
        {
            _repo.Save(new ExampleItem());
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now + TimeSpan.FromDays(1));

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Positive_Keep2Versions()
        {
            _repo.Save(new ExampleItem());
            _repo.Save(new ExampleItem());
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now + TimeSpan.FromDays(1), 2);

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(2, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Negative()
        {
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now + TimeSpan.FromDays(1));

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Negative_Keep2Versions()
        {
            _repo.Save(new ExampleItem());
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now + TimeSpan.FromDays(1), 2);

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(2, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Negative_DoesntDeleteAfterTimeSpecified()
        {
            _repo.Save(new ExampleItem());

            await _repo.PurgeHistoricalVersions(DateTime.Now - TimeSpan.FromDays(1));

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(1, allItems.Count());
        }
    }
}