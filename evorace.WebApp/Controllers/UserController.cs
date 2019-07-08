using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace evorace.WebApp.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        public IActionResult Submit()
        {
            return View();
        }

        public IActionResult Stats()
        {
            return View();
        }
    }
}