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

            var allItems = new List<ExampleItem>();
            await _repo.GetHistoryAsync("abc").ForEachAsync(_ => allItems.Add(_));

            Assert.AreEqual(2, allItems.Count());

            Assert.AreEqual("456", allItems[0].Payload);
            Assert.AreEqual("123", allItems[1].Payload);
        }

        [Test]
        public async Task GetAllIdentifiers()
        {
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "123" });
            await _repo.SaveAsync(new ExampleItem() { Identifier = "def", Payload = "123" });
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "456" });

            var allItems = new List<string>();
            await _repo.GetAllIdentifiersAsync().ForEachAsync(_ => allItems.Add(_));

            Assert.AreEqual(2, allItems.Count());
        }

        [Test]
        public async Task GetAllIdentifiers_Temporal()
        {
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "123" });
            await Task.Delay(2000);
            await _repo.SaveAsync(new ExampleItem() { Identifier = "def", Payload = "123" });
            await _repo.SaveAsync(new ExampleItem() { Identifier = "abc", Payload = "456" });

            var allItems = new List<string>();
            await _repo.GetAllIdentifiersAsync(DateTime.Now - TimeSpan.FromMilliseconds(1500)).ForEachAsync(_ => allItems.Add(_));

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task GetAll_Positive()
        {
            await _repo.SaveAsync(new ExampleItem());

            var allItems = new List<ExampleItem>();
            await _repo.GetAllAsync().ForEachAsync(_ => allItems.Add(_));

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_Positive()
        {
            await _repo.SaveAsync(new ExampleItem());

            var allItems = new List<ExampleItem>();
            await _repo.GetAllAsync(DateTime.Now).ForEachAsync(_ => allItems.Add(_));

            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_Negative()
        {
            await _repo.SaveAsync(new ExampleItem());

            var allItems = new List<ExampleItem>();
            await _repo.GetAllAsync(DateTime.Now - TimeSpan.FromDays(1)).ForEachAsync(_ => allItems.Add(_));

            Assert.AreEqual(0, allItems.Count());
        }

        [Test]
        public async Task GetAll_Temporal_GetsCorrectEntity()
        {
            await _repo.SaveAsync(new ExampleItem() { Payload = "abc" });
            await Task.Delay(2000);
            await _repo.SaveAsync(new ExampleItem() { Payload = "def" });

            var allItems = new List<ExampleItem>();
            await _repo.GetAllAsync(DateTime.Now - TimeSpan.FromMilliseconds(1500)).ForEachAsync(_ => allItems.Add(_));

            Assert.AreEqual(1, allItems.Count());
            Assert.AreEqual("abc", allItems.First().Payload);
        }

        [Test]
        public void GetAll_ManyTimes()
        {
            _repo.CreateMany();

            var getAllTasks = new List<IAsyncEnumerable<ExampleItem>>();

            for (int i = 0; i < 10; i++)
            {
                getAllTasks.Add(_repo.GetAllAsync());
            }

            getAllTasks.AsParallel().ForAll(async (task) =>
            {
                await task.ForEachAsync(_ => { });
            });
        }

        [Test]
        public void GetAll_Temporal_ManyTimes()
        {
            _repo.CreateMany();

            var getAllTasks = new List<IAsyncEnumerable<ExampleItem>>();

            for (int i = 0; i < 10; i++)
            {
                getAllTasks.Add(_repo.GetAllAsync(DateTime.Now + TimeSpan.FromSeconds(1)));
            }

            getAllTasks.AsParallel().ForAll(async (task) =>
            {
                await task.ForEachAsync(_ => { });
            });
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

            var allItems = new List<ExampleItem>();
            await _repo.GetAllAsync().ForEachAsync(_ => allItems.Add(_));

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