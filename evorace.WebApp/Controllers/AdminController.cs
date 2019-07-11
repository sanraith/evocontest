using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using evorace.WebApp.Data.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace evorace.WebApp.Controllers
{
    [Authorize(Roles = Roles.Administrator)]
    public class AdminController : Controller
    {
        public IActionResult Admin()
        {
            return View();
        }
    }
}