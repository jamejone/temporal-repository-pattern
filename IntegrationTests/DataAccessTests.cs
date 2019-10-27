using DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NUnit.Framework;
using Shared;
using System;
using System.Collections.Generic;
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
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc" });

            var item = await _repo.GetAsync("abc");

            Assert.IsNotNull(item);
            Assert.AreEqual("abc", item.Identifier);
        }

        [Test]
        public async Task Get_GetsLatest()
        {
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "123" });
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "456" });

            var item = await _repo.GetAsync("abc");

            Assert.IsNotNull(item);
            Assert.AreEqual("abc", item.Identifier);
            Assert.AreEqual("456", item.Payload);
        }

        [Test]
        public async Task Get_Temporal_GetsCorrectEntity()
        {
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "123" });
            await Task.Delay(2000);
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "456" });

            var item = await _repo.GetAsync("abc", DateTime.Now - TimeSpan.FromMilliseconds(1500));

            Assert.IsNotNull(item);
            Assert.AreEqual("abc", item.Identifier);
            Assert.AreEqual("123", item.Payload);
        }

        [Test]
        public async Task Get_Temporal_Negative()
        {
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "123" });

            var item = await _repo.GetAsync("abc", DateTime.Now - TimeSpan.FromDays(1));

            Assert.IsNull(item);
        }

        [Test]
        public async Task GetHistory()
        {
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "123" });
            await _repo.SaveAsync(new ExampleItem() { Identifier = "def", Payload = "123" });
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "456" });

            IEnumerable<ExampleItem> items = await _repo.GetHistoryAsync("abc");

            Assert.AreEqual(2, items.Count());

            List<ExampleItem> itemsList = items.ToList();

            Assert.AreEqual("456", itemsList[0].Payload);
            Assert.AreEqual("123", itemsList[1].Payload);
        }

        [Test]
        public async Task GetAll_Positive()
        {
            await _repo.SaveAsync(new ExampleItem());

            var allItems = await _repo.GetAllAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_Positive()
        {
            await _repo.SaveAsync(new ExampleItem());

            var allItems = await _repo.GetAllAsync(DateTime.Now);

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_Negative()
        {
            await _repo.SaveAsync(new ExampleItem());

            var allItems = await _repo.GetAllAsync(DateTime.Now - TimeSpan.FromDays(1));

            Assert.AreEqual(0, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_GetsCorrectEntity()
        {
            await _repo.SaveAsync(new ExampleItem() { Payload = "abc" });
            await Task.Delay(2000);
            await _repo.SaveAsync(new ExampleItem() { Payload = "def" });

            var allItems = await _repo.GetAllAsync(DateTime.Now - TimeSpan.FromMilliseconds(1500));

            Assert.AreEqual(1, allItems.Count());
            Assert.AreEqual("abc", allItems.First().Payload);
        }

        [Test]
        public void GetAll_ManyTimes()
        {
            _repo.CreateMany();

            List<Task> getAllTasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                getAllTasks.Add(_repo.GetAllAsync());
            }

            Task.WaitAll(getAllTasks.ToArray());
        }

        [Test]
        public async Task GetAll_Temporal_ManyTimes()
        {
            _repo.CreateMany();

            List<Task> getAllTasks = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                getAllTasks.Add(_repo.GetAllAsync(DateTime.Now + TimeSpan.FromSeconds(1)));
            }

            Task.WaitAll(getAllTasks.ToArray());
        }

        [Test]
        public async Task Save_ManyEntities()
        {
            List<Task> saveTasks = new List<Task>();

            for (int i = 0; i < 1000; i++)
            {
                saveTasks.Add(_repo.SaveAsync(new ExampleItem() { Identifier = i.ToString() }));
            }

            Task.WaitAll(saveTasks.ToArray());

            var allItems = await _repo.GetAllAsync();

            Assert.IsNotNull(allItems);
            Assert.AreEqual(1000, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Positive()
        {
            await _repo.SaveAsync(new ExampleItem());
            await _repo.SaveAsync(new ExampleItem());

            await _repo.PurgeHistoricalVersionsAsync(DateTime.Now + TimeSpan.FromDays(1));

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Positive_Keep2Versions()
        {
            await _repo.SaveAsync(new ExampleItem());
            await _repo.SaveAsync(new ExampleItem());
            await _repo.SaveAsync(new ExampleItem());

            await _repo.PurgeHistoricalVersionsAsync(DateTime.Now + TimeSpan.FromDays(1), 2);

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(2, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Negative()
        {
            await _repo.SaveAsync(new ExampleItem());

            await _repo.PurgeHistoricalVersionsAsync(DateTime.Now + TimeSpan.FromDays(1));

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Negative_Keep2Versions()
        {
            await _repo.SaveAsync(new ExampleItem());
            await _repo.SaveAsync(new ExampleItem());

            await _repo.PurgeHistoricalVersionsAsync(DateTime.Now + TimeSpan.FromDays(1), 2);

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(2, allItems.Count());
        }

        [Test]
        public async Task PurgeOldRecords_Negative_DoesntDeleteAfterTimeSpecified()
        {
            await _repo.SaveAsync(new ExampleItem());

            await _repo.PurgeHistoricalVersionsAsync(DateTime.Now - TimeSpan.FromDays(1));

            var allItems = await _testRepo.GetAllIncludingAllVersionsAsync();

            Assert.AreEqual(1, allItems.Count());
        }
    }
}