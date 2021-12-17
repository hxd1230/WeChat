using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WeChat.UI.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Item()
        {
            return View();
        }
        public IActionResult CallBack(string data)
        {
            ViewData["data"] = data;
            return View();
        }
    }
}