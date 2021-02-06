using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DenBagus.Models;

namespace DenBagus.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private const string AuthSchemes =
            CookieAuthenticationDefaults.AuthenticationScheme + "," +
            OpenIdConnectDefaults.AuthenticationScheme;

        private IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        //[Authorize(AuthenticationSchemes = AuthSchemes)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
