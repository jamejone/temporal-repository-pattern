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
        private readonly MongoItemRepository _mongoItemRepo;

        public HomeController(MongoItemRepository mongoItemRepo)
        {
            _mongoItemRepo = mongoItemRepo;
        }

        public async Task<IActionResult> Index()
        {
            IndexModel model = new IndexModel();

            var response = await _mongoItemRepo.GetAllAsync();

            var businessObjectItemList = new List<BusinessObjectItem>();
            foreach (var item in response)
            {
                var businessObjectItem = new BusinessObjectItem()
                {
                    ObjectID = item.Id.ToString()
                };
                businessObjectItemList.Add(businessObjectItem);
            }

            model.BusinessObjectItems = businessObjectItemList.ToArray();

            model.NumberOfItems = response.Count();

            return View(model);
        }

        public IActionResult Create()
        {
            _mongoItemRepo.CreateMany();

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
