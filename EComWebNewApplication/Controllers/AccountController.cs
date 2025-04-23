using EComWebNewApplication.Data;
using EComWebNewApplication.Users;
using EComWebNewApplication.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using System.Text;

namespace EComWebNewApplication.Controllers
{
    public class AccountController : Controller
    {
        // Reference to the application's DbContext for accessing the database.
        private readonly ApplicationDbContext _context;
        // Constructor that receives the DbContext via dependency injection.
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Account/Register
        // Renders the registration page.
        public IActionResult Register()
        {
            return View();
        }
        // POST: Account/Register
        // Handles form submission for registering a new customer.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Check if the input model is valid.
            if (ModelState.IsValid)
            {
                // Check if a customer with the provided email already exists.
                if (await _context.Customers.AnyAsync(c => c.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email already registered.");
                    return View(model); // Return view with error message.
                }
                // Create a new Customer instance from the registration data.
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    Email = model.Email,
                    Password = model.Password, // In production, hash the password.
                    Phone = model.Phone,
                    BillingAddress = model.BillingAddress
                };
                // Add the new customer to the database context.
                _context.Customers.Add(customer);
                // Save changes asynchronously.
                await _context.SaveChangesAsync();
                // Redirect to the Login page after successful registration.
                return RedirectToAction("Login");
            }
            // If model validation fails, redisplay the registration form with validation messages.
            return View(model);
        }
        // GET: Account/Login
        // Renders the login page.
        public IActionResult Login()
        {
            return View();
        }
        // POST: Account/Login
        // Processes the login form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Check if the input model is valid.
            if (ModelState.IsValid)
            {
                // Attempt to find a customer with matching email and password.
                var customer = _context.Customers.AsNoTracking()
                    .FirstOrDefault(c => c.Email == model.Email && c.Password == model.Password);
                if (customer != null)
                {
                    // Create claims for the authenticated user.
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                        new Claim(ClaimTypes.Name, customer.CustomerName),
                        new Claim(ClaimTypes.Email, customer.Email)
                    };
                    // Create a ClaimsIdentity with the specified claims and authentication scheme.
                    // Cookie Based Authentication
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    // Sign in the user with the created identity.
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                    // Redirect to the Order Index page upon successful login.
                    return RedirectToAction("Index", "Order");
                }
                // If no matching customer is found, add an error message.
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            // If model validation fails or login unsuccessful, redisplay the login form.
            return View(model);
        }
        // GET: Account/Logout
        // Logs out the user.
        public async Task<IActionResult> Logout()
        {
            // Sign the user out of the cookie-based authentication scheme.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Redirect to the Login page after logout.
            return RedirectToAction("Login");
        }
    }
}



