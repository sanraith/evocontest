using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using evorace.WebApp.Data;
using evorace.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace evorace.WebApp.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class AdminController : Controller
    {
        public AdminController(ContestDb db)
        {
            myDb = db;
        }

        public IActionResult Admin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DoClearAllSubmissions()
        {
            myDb.Submissions.RemoveRange(myDb.Submissions);
            await myDb.SaveChangesAsync();

            ViewBag.Message = "Success";

            return View(nameof(Admin));
        }

        private readonly ContestDb myDb;
    }
}