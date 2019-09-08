using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using evorace.WebApp.Models;
using evorace.WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace evorace.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(ContestDb database)
        {
            myDb = database;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Rankings()
        {
            var matches = await myDb.Matches
                .Include(x => x.Measurements)
                    .ThenInclude(x => x.Submission)
                        .ThenInclude(x => x.User)
                .ToListAsync();

            return View(matches);
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

        private readonly ContestDb myDb;
    }
}
