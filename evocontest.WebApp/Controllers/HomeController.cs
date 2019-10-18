using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using evocontest.WebApp.Models;
using evocontest.WebApp.Data;
using Microsoft.EntityFrameworkCore;
using evocontest.WebApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using evocontest.WebApp.Data.Helper;

namespace evocontest.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(ContestDb database, UserManager<ApplicationUser> userManager)
        {
            myDb = database;
            myUserManager = userManager;
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
                .OrderBy(x => x.MatchDate)
                .ToListAsync();

            var orderedMatches = matches;
            var lastMatch = orderedMatches.LastOrDefault();

            var adminUsers = await myUserManager.GetUsersInRoleAsync(Roles.Admin);

            var lastMatchOrderedMeasurements = lastMatch?.Measurements
                .Where(x => x.MeasurementResult.Result != null)
                .OrderByDescending(x => adminUsers.Contains(x.Submission.User) ? -1 : x.MeasurementResult.Result.DifficultyLevel)
                .ThenBy(x => x.MeasurementResult.Result.TotalMilliseconds)
                .ToList();
            var lastMatchInvalidMeasurements = lastMatch?.Measurements
                .Where(x => x.MeasurementResult.Result == null).ToList() ?? new List<Measurement>();

            return View(new RankingsViewModel
            {
                AdminUsers = adminUsers,
                OrderedMatches = orderedMatches,
                LastMatch = lastMatch,
                LastMatchOrderedMeasurements = lastMatchOrderedMeasurements,
                LastMatchInvalidMeasurements = lastMatchInvalidMeasurements
            });
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
        private readonly UserManager<ApplicationUser> myUserManager;
    }
}
