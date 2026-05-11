using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OnlineSinav.MVC.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Create() => View();
        public IActionResult Edit(int id) => View("Create", id); // aynı formu kullanacağız
        public IActionResult Questions(int examId) => View(model: examId);
        public IActionResult Results(int examId) => View(model: examId);
    }

}