using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.Models;
using Project.Data;
using System;
using System.Linq;

namespace Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly WarehouseDbContext _context;

        public AccountController(WarehouseDbContext context)
        {
            _context = context;
        }

        // === LOGIN ===
        public IActionResult Login()
        {
            // Jika sudah login, langsung ke Home
            if (HttpContext.Session.GetString("Username") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Username atau password salah!";
            return View();
        }

        // === REGISTER ===
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("", "Username sudah digunakan.");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();
                TempData["Success"] = "Registrasi berhasil. Silakan login.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // === FORGOT PASSWORD ===
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgotPassword(string username)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                // Redirect ke halaman reset password
                return RedirectToAction("ResetPassword", new { username });
            }

            TempData["ResetSuccess"] = "Username tidak ditemukan.";
            return View();
        }


        // === GET: Tampilkan form reset password ===
        [HttpGet]
        public IActionResult ResetPassword(string username)
        {
            var model = new ResetPasswordViewModel
            {
                Username = username
            };
            return View(model);
        }

        // POST: Simpan password baru
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Konfirmasi password tidak cocok.");
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);
            if (user == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            user.Password = model.NewPassword;
            _context.SaveChanges();

            TempData["Success"] = "Password berhasil diubah. Silakan login.";
            return RedirectToAction("Login");
        }

        // === LOGOUT ===
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Hapus semua session
            return RedirectToAction("Login");
        }
    }
}
