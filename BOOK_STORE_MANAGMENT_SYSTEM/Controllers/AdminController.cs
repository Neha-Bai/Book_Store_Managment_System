using BOOK_STORE_MANAGMENT_SYSTEM.Models;
using Microsoft.AspNetCore.Mvc;

namespace BOOK_STORE_MANAGMENT_SYSTEM.Controllers
{
    public class AdminController : Controller
    {
        private readonly DbRepo _db;
        public AdminController(DbRepo db) {
            _db = db; 
        }

        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account"); // if not admin redirect back to login page
            return View(); // if admin return view
        }
    }
}
