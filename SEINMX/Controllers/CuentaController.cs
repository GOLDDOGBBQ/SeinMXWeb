using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Context;
using SEINMX.Clases; // Para hash de contraseñas
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;


namespace SEINMX.Controllers
{
    public class CuentaController : Controller
    {
        private readonly AppDbContext _context;

        public CuentaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Cuenta/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // GET: /Cuenta/Login
        [HttpGet]
        [Authorize]
        public IActionResult Inicio()
        {
            return View();
        }


        // POST: /Cuenta/Login
        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string password)
        {
            ViewBag.Usuario = usuario;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                return View();
            }


            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Usuario1 == usuario && !u.Eliminado);

            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash)) // En producción: usa hash y salt
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View();
            }


            if (user.CambiarPassword)
            {
                TempData["IdUsuarioCambio"] = user.IdUsuario;
                TempData["Mensaje"] = "Debe cambiar su contraseña antes de continuar.";
                return RedirectToAction("CambiarPassword");
            }


            // Actualizar último acceso
            user.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();


            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Usuario1),
                new Claim(ClaimTypes.Name, user.Nombre ?? user.Usuario1),
                new Claim("Usuario", user.Usuario1),
                new Claim("Admin", user.Admin.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                });



            return RedirectToAction("Inicio", "Cuenta");
        }


        // GET: /Cuenta/CambiarPassword
        [HttpGet]
        public IActionResult CambiarPassword()
        {
            if (!TempData.ContainsKey("IdUsuarioCambio"))
                return RedirectToAction("Login");

            ViewBag.Mensaje = TempData["Mensaje"];
            return View();
        }


// POST: /Cuenta/CambiarPassword
        [HttpPost]
        public async Task<IActionResult> CambiarPassword(string nuevaPassword, string confirmarPassword)
        {
            if (!TempData.ContainsKey("IdUsuarioCambio"))
                return RedirectToAction("Login");

            int idUsuario = (int)TempData["IdUsuarioCambio"];

            if (string.IsNullOrEmpty(nuevaPassword) || nuevaPassword != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden o están vacías.";
                TempData["IdUsuarioCambio"] = idUsuario; // Mantener temporal
                return View();
            }

            var user = await _context.Usuarios.FindAsync(idUsuario);
            if (user == null)
                return RedirectToAction("Login");

            user.PasswordHash = PasswordHasher.GenerateHash(nuevaPassword);
            user.CambiarPassword = false;
            user.UltimoAcceso = DateTime.Now;

            await _context.SaveChangesAsync();

            // Crear claims y hacer login
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.Nombre ?? user.Usuario1),
                new Claim("Usuario", user.Usuario1),
                new Claim("Admin", user.Admin.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
                });

            return RedirectToAction("Inicio", "Cuenta");
        }

        // =======================
        // Logout
        // =======================
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        // GET: /Cuenta/AccesoDenegado
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}