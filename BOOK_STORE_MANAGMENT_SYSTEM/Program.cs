using BOOK_STORE_MANAGMENT_SYSTEM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1️Add MVC
builder.Services.AddControllersWithViews(); // without this app will not return views

// 2️ Add Session
builder.Services.AddSession(); // to store temp data like logged in user 

// 3️ Add DbContext(tells to use sql server + Connection String from app
builder.Services.AddDbContext<DbRepo>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// 4️ Add Password Hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();



//build the app so it can start processing requests
var app = builder.Build();

// Middleware
app.UseStaticFiles(); // load css js files
app.UseRouting(); // enables navigations routing //Tells app how to match a URL → controller → action


app.UseSession(); //make session avaiable

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
