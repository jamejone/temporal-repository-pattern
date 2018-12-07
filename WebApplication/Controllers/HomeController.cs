using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private TemporalRepository _repo = new TemporalRepository();

        public IActionResult Index()
        {
            IndexModel model = new IndexModel();

            var result = _repo.GetAll();

            var businessObjectItemList = new List<BusinessObjectItem>();
            foreach (var item in result)
            {
                var businessObjectItem = new BusinessObjectItem()
                {
                    ObjectID = item.Id.ToString(),
                    Name = item.Name
                };
                businessObjectItemList.Add(businessObjectItem);
            }

            model.BusinessObjectItems = businessObjectItemList.ToArray();

            return View(model);
        }

        public IActionResult Create()
        {
            _repo.Create();

            return RedirectToAction("Index");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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
