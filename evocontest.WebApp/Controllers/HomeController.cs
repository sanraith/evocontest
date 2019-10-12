using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using evocontest.WebApp.Models;
using evocontest.WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace evocontest.WebApp.Controllers
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

        public IActionResult Info()
        {
            return View();
        }

        public IActionResult Rules()
        {
            return View();
        }

        public async Task<IActionResult> Rankings()
        {
            var matches = await myDb.Matches
                .Include(x => x.Measurements)
                    .ThenInclude(x => x.Submission)
                        .ThenInclude(x => x.User)
                .Select(x => new Match
                {
                    Id = x.Id,
                    MatchDate = x.MatchDate,
                    Measurements = x.Measurements.Select(m => new Measurement
                    {
                        Id = m.Id,
                        JsonResult = m.JsonResult,
                        Submission = new Submission
                        {
                            Id = m.Submission.Id,
                            User = m.Submission.User
                        }
                    }).ToList()
                })
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
