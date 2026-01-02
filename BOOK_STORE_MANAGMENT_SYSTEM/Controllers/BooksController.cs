using BOOK_STORE_MANAGMENT_SYSTEM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BOOK_STORE_MANAGMENT_SYSTEM.Controllers
{
    public class BooksController : Controller
    {
        private readonly DbRepo _db;

        public BooksController(DbRepo db)
        {
            _db = db;
        }

        private bool IsAdmin() =>
            HttpContext.Session.GetString("UserRole") == "Admin"; // check if is admin

        // ---------------- ADMIN BOOK LIST ----------------
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var books = _db.Books.ToList(); //list of books on index page will be displayed all boks
            return View(books);  // if admin open books view
        }

        // ---------------- CREATE BOOK ----------------
        public IActionResult Create() //get create.cshtml
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.Categories = BookCategories(); //loads categories into viewbag sp can be diaplyed in view
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Book book)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = BookCategories();
                return View(book);
            }

            _db.Books.Add(book);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ---------------- EDIT BOOK ----------------
        public async Task<IActionResult> Edit(int id) //Get edit.cshtml with selected book id
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();

            ViewBag.Categories = BookCategories();
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Book book) // save upa=dated book info
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = BookCategories();
                return View(book);
            }

            
            _db.Books.Update(book);

            
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ---------------- DELETE BOOK ----------------
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var book = await _db.Books.FindAsync(id);
            if (book == null) return NotFound();

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ---------------- CUSTOMER VIEW ONLY ----------------
        public async Task<IActionResult> CustomerBooks(string? search)
        {
            var books = _db.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b =>
                    b.Title.Contains(search) ||
                    b.Author.Contains(search));
            }

            var result = await books.ToListAsync();

            return View("~/Views/Customer/CustomerBooks.cshtml", result);
        }


        //Buying a book Action:
        public async Task<IActionResult>Buy(int id)
        {
            var book = await _db.Books.FindAsync(id);

            if(book == null)
            {
                return NotFound();
            }
            return View("~/Views/Customer/Buy.cshtml", book);
        }

        //place order
        public IActionResult PlaceOrder()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cartItems = _db.Carts
                               .Include(c => c.Book) //includes related book data
                               .Where(c => c.UserId == userId) //get all items for curret user from cart
                               .ToList(); // convert relsult into list

            if (!cartItems.Any()) //not any item in cart
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Cart"); //sends back to cart page
            }

            // Create Bill
            var bill = new Bill
            {
                BillingDate = DateTime.Now,
                CustomerName = HttpContext.Session.GetString("UserName") ?? "Unknown" // if no name then unknown
            };

            // Add Bill items
            foreach (var item in cartItems)
            {
                bill.Items.Add(new BillItem   
                {
                    BookName = item.Book.Title,
                    BookPrice = item.Book.Price,
                    Quantity = item.Quantity
                });

                // Reduce stock
                item.Book.Quantity -= item.Quantity; //reduce quanatity whenn order dine
            }

            _db.Bills.Add(bill); // Adds new bill to the database

            // Clear cart
            _db.Carts.RemoveRange(cartItems);

            _db.SaveChanges();

            TempData["Success"] = " Your order has been placed successfully!";
            return RedirectToAction("OrderSuccess", new { id = bill.Id }); //passing bill id to ordersuccess view to show details
        }
        //order confimed
        public IActionResult OrderSuccess(int id) //after placing order then this id is the bill id passed from place order
        {
            var bill = _db.Bills
                          .Include(b => b.Items)
                          .FirstOrDefault(b => b.Id == id);

            if (bill == null) return NotFound();

            return View("~/Views/Customer/OrderSuccess.cshtml", bill);
        }


        //Add to cart
        public IActionResult AddToCart(int BookId, int Qty)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var existingItem = _db.Carts
                                 .FirstOrDefault(c => c.UserId == userId && c.BookId == BookId); //checks if the same user already added that book in the cart if alreadybadded then quantity update else cart added

            var book = _db.Books.FirstOrDefault(b => b.Id == BookId); // fetches book detail liek title , price quamtity
            if (book == null) return NotFound();

            // Check stock availability
            if (Qty > book.Quantity)
            {
                TempData["Error"] = "Not enough stock available!";
                return RedirectToAction("CustomerBooks");
            }

            if (existingItem != null) //if sme user already added books qunatity pupdate if quantity exceeds redirect back custoomer books
            {
                // If adding more would exceed stock:
                if (existingItem.Quantity + Qty > book.Quantity)
                {
                    TempData["Error"] = "Requested quantity exceeds available stock!";
                    return RedirectToAction("CustomerBooks");
                }

                existingItem.Quantity += Qty; //quantity ++
                _db.Carts.Update(existingItem);
                 
            }
            else
            {
                var cart = new Cart
                {
                    UserId = userId.Value,
                    BookId = BookId,
                    Quantity = Qty
                };
                _db.Carts.Add(cart); //add items into cart
            }

            _db.SaveChanges();
            return RedirectToAction("Cart"); 
        }

        public IActionResult Cart()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var items = _db.Carts
                           .Include(c => c.Book)
                           .Where(c => c.UserId == userId)
                           .ToList();

            return View("~/Views/Customer/Cart.cshtml", items);
        }


        //update cart
        [HttpPost]
        public IActionResult UpdateCart(int CartId, int Qty)
        {
            var cart = _db.Carts.Include(c => c.Book).FirstOrDefault(c => c.Id == CartId);
            if (cart == null) return NotFound();

            if (Qty > cart.Book.Quantity)
            {
                TempData["Error"] = "Not enough stock!";
                return RedirectToAction("Cart");
            }

            cart.Quantity = Qty;
            _db.Carts.Update(cart);
            _db.SaveChanges();

            return RedirectToAction("Cart");
        }
        //Delete item from cart
        public IActionResult DeleteFromCart(int id)
        {
            var item = _db.Carts.FirstOrDefault(c => c.Id == id);
            if (item == null) return NotFound();

            _db.Carts.Remove(item);
            _db.SaveChanges();

            return RedirectToAction("Cart");
        }
        //checkout button
        public IActionResult Checkout()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cartItems = _db.Carts
                               .Include(c => c.Book)
                               .Where(c => c.UserId == userId)
                               .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Cart");
            }

            // Prepare the Bill model for the checkout page
            var bill = new Bill
            {
                BillingDate = DateTime.Now,
                CustomerName = HttpContext.Session.GetString("UserName") ?? "Customer", //if name is null then set CustometName = customer
                Items = cartItems.Select(c => new BillItem
                {
                    BookName = c.Book.Title,
                    BookPrice = c.Book.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            return View("~/Views/Customer/Checkout.cshtml", bill);
        }

        [HttpPost]
        public IActionResult ConfirmCheckout(Bill bill)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Save bill to DB
            _db.Bills.Add(bill);

            // Reduce stock
            foreach (var item in bill.Items)
            {
                var book = _db.Books.FirstOrDefault(b => b.Title == item.BookName);
                if (book != null)
                {
                    book.Quantity -= item.Quantity;
                    _db.Books.Update(book);
                }
            }

            // Empty user cart
            var cartItems = _db.Carts.Where(c => c.UserId == userId).ToList();
            _db.Carts.RemoveRange(cartItems);

            _db.SaveChanges();

            return RedirectToAction("OrderSuccess", new { id = bill.Id });
        }
        

        // Helper category list
        private List<string> BookCategories() => new List<string>
        {
            "Fiction",
            "Non-Fiction",
            "Science",
            "Technology",
            "Coding",
            "Biography",
            "Children",
            "Mystery",
            "Social",
            "Fantasy"
        }; 
    }
}
