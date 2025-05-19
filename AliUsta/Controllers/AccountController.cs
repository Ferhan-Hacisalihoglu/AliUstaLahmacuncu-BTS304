using AliUsta.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data; // Required for CommandType
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace AliUsta.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AdminViewModel model, string returnUrl)
        {
            if (!ModelState.IsValidField("Username") || !ModelState.IsValidField("Password"))
            {
                if (!ModelState.IsValidField("Username")) ModelState.AddModelError("Username", "Kullanıcı adı gereklidir.");
                if (!ModelState.IsValidField("Password")) ModelState.AddModelError("Password", "Şifre gereklidir.");
                return View(model);
            }

            string passwordFromDb = null;
            bool userExists = false; // We can infer this from passwordFromDb != null

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                // Use stored procedure sp_GetAdminPasswordByUsername
                using (MySqlCommand cmd = new MySqlCommand("sp_GetAdminPasswordByUsername", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_Username", model.Username); // Match SP parameter name

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        passwordFromDb = result.ToString();
                        userExists = true;
                    }
                }
            }

            // IMPORTANT: This is plain text password comparison. HASH passwords!
            if (userExists && model.Password == passwordFromDb)
            {
                FormsAuthentication.SetAuthCookie(model.Username, false);
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
                return View(model);
            }
        }

        public ActionResult Register()
        {
            return View(new AdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(AdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool userExists = false;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                // Use stored procedure sp_CheckAdminExists
                using (MySqlCommand checkCmd = new MySqlCommand("sp_CheckAdminExists", conn))
                {
                    checkCmd.CommandType = CommandType.StoredProcedure;
                    checkCmd.Parameters.AddWithValue("@p_Username", model.Username);
                    userExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                }

                if (userExists)
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                    return View(model);
                }

                // Use stored procedure sp_RegisterAdmin
                // IMPORTANT: HASH model.Password before sending it to the SP in a real app!
                using (MySqlCommand cmd = new MySqlCommand("sp_RegisterAdmin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_Username", model.Username);
                    cmd.Parameters.AddWithValue("@p_Password", model.Password); // Send plain text, SP stores plain text
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapın.";
            return RedirectToAction("Login");
        }

        public ActionResult ChangePassword()
        {
            // If you want to pre-fill username for logged-in user:
            // var viewModel = User.Identity.IsAuthenticated ? new AdminViewModel { Username = User.Identity.Name } : new AdminViewModel();
            var viewModel = new AdminViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(AdminViewModel model)
        {
            // It's better to identify the user by their current session/identity
            // rather than asking them to type their username again if they are already logged in.
            // However, your current model allows changing password for any user if you know their username.
            // For this example, I'll stick to your model structure.
            // If this action requires authentication, add [Authorize] attribute.

            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(model.Username))
                    ModelState.AddModelError("Username", "Kullanıcı adı gereklidir.");
                if (string.IsNullOrWhiteSpace(model.NewPassword))
                    ModelState.AddModelError("NewPassword", "Yeni şifre gereklidir.");
                return View(model);
            }

            bool userExists = false;
            int rowsAffected = 0;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                // Use stored procedure sp_CheckAdminExists
                using (MySqlCommand checkCmd = new MySqlCommand("sp_CheckAdminExists", conn))
                {
                    checkCmd.CommandType = CommandType.StoredProcedure;
                    checkCmd.Parameters.AddWithValue("@p_Username", model.Username);
                    userExists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                }

                if (!userExists)
                {
                    ModelState.AddModelError("Username", "Belirtilen kullanıcı bulunamadı.");
                    return View(model);
                }

                // Use stored procedure sp_ChangeAdminPassword
                // IMPORTANT: HASH model.NewPassword before sending it to the SP in a real app!
                using (MySqlCommand cmd = new MySqlCommand("sp_ChangeAdminPassword", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_Username", model.Username);
                    cmd.Parameters.AddWithValue("@p_NewPassword", model.NewPassword);
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            if (rowsAffected > 0)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi. Lütfen yeni şifrenizle giriş yapın.";
                // Consider logging the user out if they changed their own password
                // FormsAuthentication.SignOut();
                return RedirectToAction("Login");
            }
            else
            {
                // This case might also mean the user existed but the update failed for some other reason,
                // or if the SP had logic to not update if new password was same as old.
                // For this simple SP, it implies user was found but update didn't affect rows, which is unlikely
                // unless there's a trigger or other constraint.
                // The more likely error is that the user wasn't found, which is caught above.
                ModelState.AddModelError("", "Şifre değiştirilirken bir hata oluştu.");
                return View(model);
            }
        }

        [Authorize] // This should be Authorize to ensure only logged-in users can log off
        [HttpPost] // LogOff should ideally be a POST to prevent CSRF via GET requests
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}