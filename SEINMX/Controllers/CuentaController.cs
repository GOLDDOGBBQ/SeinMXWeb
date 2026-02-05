using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEINMX.Context;
using SEINMX.Clases; // Para hash de contraseñas
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SEINMX.Models.Cuenta;


namespace SEINMX.Controllers
{
    public class CuentaController : ApplicationController
    {
        private readonly AppDbContext _db;

        public CuentaController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Cuenta/Login
        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginViewModel
            {
                Recuerdame = true
            };
            return View(model);
        }

        // GET: /Cuenta/Login
        [HttpGet]
        [Authorize]
        public IActionResult Inicio()
        {
            var model = new InicioVM()
            {
              UltimoTC = _db.TipoCambioDofs.OrderByDescending(x => x.Fecha).FirstOrDefault()
            };


            return View(model);
        }


        // POST: /Cuenta/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                TempData["toast-error"] = "Revise los campos marcados.";
                return View(model);
            }

            model.Normalize();

            if (string.IsNullOrEmpty(model.Usuario) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                TempData["toast-error"] = "Debe ingresar usuario y contraseña.";
                return View(model);
            }


            var user = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Usuario1 == model.Usuario && !u.Eliminado);

            if (user == null ||
                !PasswordHasher.Verify(model.Password, user.PasswordHash)) // En producción: usa hash y salt
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                TempData["toast-error"] = "Usuario o contraseña incorrectos.";
                return View(model);
            }


            // Actualizar último acceso
            user.UltimoAcceso = DateTime.Now;
            await _db.SaveChangesAsync();


            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Usuario1),
                new Claim(ClaimTypes.Name, user.Nombre ?? user.Usuario1),
                new Claim("Usuario", user.Usuario1),
                new Claim("Admin", user.Admin.ToString()),
                new Claim("IdUsuario", user.IdUsuario.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            if (model.Recuerdame)
            {
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(15)
                    });
            }
            else
            {
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = true
                    });
            }

            if (user.CambiarPassword)
            {
                TempData["Mensaje"] = "Debe cambiar su contraseña antes de continuar.";
                return RedirectToAction("CambiarPassword");
            }


            return RedirectToAction("Inicio", "Cuenta");
        }


        // GET: /Cuenta/CambiarPassword
        [HttpGet]
        [Authorize]
        public IActionResult CambiarPassword()
        {
            ViewBag.Mensaje = TempData["Mensaje"];
            var model = new LoginCambioViewModel();
            return View(model);
        }


        // POST: /Cuenta/CambiarPassword
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CambiarPassword(LoginCambioViewModel model)
        {
            int idUsuario = GetIdUsuarioPrimaryKey();

            if (idUsuario == 0)
            {
                TempData["toast-error"] = "Usuario no encontrado. inicia sesion de nuevo";
                return RedirectToAction("Logout", "Cuenta");
            }

            if (string.IsNullOrEmpty(model.NuevaPassword) || model.NuevaPassword != model.ConfirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden o están vacías.";
                TempData["toast-error"] = "Las contraseñas no coinciden o están vacías.";
                return View(model);
            }

            var user = await _db.Usuarios.FindAsync(idUsuario);
            if (user == null)
                return RedirectToAction("Login");

            user.PasswordHash = PasswordHasher.GenerateHash(model.NuevaPassword);
            user.CambiarPassword = false;
            user.UltimoAcceso = DateTime.Now;

            await _db.SaveChangesAsync();

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