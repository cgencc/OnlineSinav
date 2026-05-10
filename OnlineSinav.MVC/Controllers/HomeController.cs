using Microsoft.AspNetCore.Mvc;
using OnlineSinav.MVC.Models;
using System.Diagnostics;

namespace OnlineSinav.MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Teacher"))
                    return RedirectToAction("Index", "Teacher");
                else if (User.IsInRole("Student"))
                    return RedirectToAction("Exams", "Student");
            }
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}