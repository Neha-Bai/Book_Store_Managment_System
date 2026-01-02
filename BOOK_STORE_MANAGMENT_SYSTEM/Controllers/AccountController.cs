
using BOOK_STORE_MANAGMENT_SYSTEM.Models;
using BOOK_STORE_MANAGMENT_SYSTEM.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BOOK_STORE_MANAGMENT_SYSTEM.Controllers
{
    public class AccountController : Controller
    {
        private readonly DbRepo _db;
        private readonly IPasswordHasher<User> _hasher; // password hashing object

        public AccountController(DbRepo db, IPasswordHasher<User> hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // Login page
        public IActionResult Login(string type)  // it open page based on input type customer / Admin login
        {
            ViewBag.LoginType = type; //store type admin or customer?
            return View();
        }

        // LOGIN LOGIC
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password, string type)
        {
            if (type == "Admin")
            {
                if (userName == "Admin" && password == "Admin123")
                {
                    HttpContext.Session.SetString("UserRole", "Admin");
                    return RedirectToAction("Index", "Books");
                }

                ModelState.AddModelError("", "Invalid Admin Credentials");
                ViewBag.LoginType = "Admin";//ViewBag is a temporary bag 
                return View();
            }

            // CUSTOMER LOGIN
            if (type == "Customer")
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName); // find first user 

                if (user == null)
                {
                    TempData["Error"] = "User not found — please register first.";
                    return RedirectToAction("Register");
                }

                var check = _hasher.VerifyHashedPassword(user, user.Password, password); // compare pass with hashed password

                if (check == PasswordVerificationResult.Success)
                {
                    HttpContext.Session.SetString("UserRole", user.Role);//session tempray storage type saves as logged in is customer
                    HttpContext.Session.SetInt32("UserId", user.Id);//user id used for ither things likekmcart billing
                    HttpContext.Session.SetString("UserName", user.UserName); // user name for bill

                    return RedirectToAction("CustomerBooks", "Books"); // logged in customer successfully then redirect to customer page
                }

                ModelState.AddModelError("", "Invalid Customer Credentials"); // if no customer/inavlid pass
                return View(); 
            }

            return View();
        }

        // Register Page
        public IActionResult Register() => View(); //get

        // REGISTER LOGIC
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model) //async bcz loading takes time so it tells EF tro wait
        {
            if (!ModelState.IsValid) // like email should be vlid  or pass , no empty field
                return View(model);

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Password mismatch.");
                return View(model);
            }

            // Check if user already exists(only duplicate/same name )
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);

            if (existing != null)
            {
                ModelState.AddModelError("", "User already exists.");
                return View(model);
            }

            // Create User
            var user = new User //user table for login authentication
            {
                UserName = model.UserName,
                Role = "Customer"
            };

            user.Password = _hasher.HashPassword(user, model.Password); //coverting user password into hashed

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var customer = new Customer //for customer table
            {
                UserId = user.Id, //foreign key linking user to customer
                Name = model.UserName,
                Email = model.Email,
                Phone = model.Phone
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Registration successful — please login!"; //Tmp message
            return RedirectToAction("Login", new { type = "Customer" }); //redirect to login
        }

        public IActionResult CustomerOptions()
        {
            return View();
        }
    }
}
