using DataAccess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApp.Models;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ExampleItemRepository _exampleItemRepo;

        public HomeController(ExampleItemRepository ExampleItemRepo)
        {
            _exampleItemRepo = ExampleItemRepo;
        }

        public async Task<IActionResult> Index()
        {
            IndexModel model = new IndexModel();

            var businessObjectItemList = new List<BusinessObjectItem>();
            int count = 0;
            await foreach (var item in _exampleItemRepo.GetAllAsync())
            {
                var businessObjectItem = new BusinessObjectItem()
                {
                    ObjectID = item.Id.ToString()
                };
                businessObjectItemList.Add(businessObjectItem);
                count++;
            }

            model.BusinessObjectItems = businessObjectItemList.ToArray();

            model.NumberOfItems = count;

            return View(model);
        }

        public IActionResult Create()
        {
            _exampleItemRepo.CreateMany();

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
