using AliUsta.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data; // Required for CommandType
using System.Web.Mvc;

namespace AliUsta.Controllers
{
    [Authorize]
    public class MusteriController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public ActionResult Index()
        {
            List<MusteriViewModel> musteriler = new List<MusteriViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetAllMusteriler
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllMusteriler", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                musteriler.Add(new MusteriViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Ad = reader.IsDBNull(reader.GetOrdinal("Ad")) ? null : reader.GetString("Ad"),
                                    Soyad = reader.IsDBNull(reader.GetOrdinal("Soyad")) ? null : reader.GetString("Soyad"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Müşteriler listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(musteriler);
        }

        public ActionResult Create()
        {
            return View(new MusteriViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MusteriViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_CreateMusteri
                        using (MySqlCommand cmd = new MySqlCommand("sp_CreateMusteri", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_Ad", viewModel.Ad);
                            cmd.Parameters.AddWithValue("@p_Soyad", viewModel.Soyad);
                            cmd.Parameters.AddWithValue("@p_Telefon", (object)viewModel.Telefon ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_EPosta", (object)viewModel.EPosta ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        TempData["SuccessMessage"] = "Müşteri başarıyla eklendi.";
                        return RedirectToAction("Index");
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry (if you have unique constraints on Telefon or EPosta)
                    {
                        // Example: Determine which field caused the error if possible, or a general message
                        ModelState.AddModelError("", "Bu telefon numarası veya e-posta adresi zaten kayıtlı.");
                        TempData["ErrorMessage"] = "Müşteri eklenirken bir hata oluştu: Bu telefon numarası veya e-posta adresi zaten kayıtlı.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Müşteri eklenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Müşteri eklenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            MusteriViewModel musteriViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetMusteriById
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMusteriById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                musteriViewModel = new MusteriViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Ad = reader.IsDBNull(reader.GetOrdinal("Ad")) ? null : reader.GetString("Ad"),
                                    Soyad = reader.IsDBNull(reader.GetOrdinal("Soyad")) ? null : reader.GetString("Soyad"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Müşteri bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (musteriViewModel == null)
            {
                TempData["ErrorMessage"] = "Müşteri bulunamadı.";
                return HttpNotFound();
            }
            return View(musteriViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MusteriViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_UpdateMusteri
                        using (MySqlCommand cmd = new MySqlCommand("sp_UpdateMusteri", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_ID", viewModel.ID);
                            cmd.Parameters.AddWithValue("@p_Ad", viewModel.Ad);
                            cmd.Parameters.AddWithValue("@p_Soyad", viewModel.Soyad);
                            cmd.Parameters.AddWithValue("@p_Telefon", (object)viewModel.Telefon ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_EPosta", (object)viewModel.EPosta ?? DBNull.Value);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["SuccessMessage"] = "Müşteri başarıyla güncellendi.";
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Müşteri güncellenemedi (muhtemelen bulunamadı veya veri değişmedi).";
                                ModelState.AddModelError("", "Müşteri güncellenemedi.");
                            }
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
                    {
                        ModelState.AddModelError("", "Güncellemeye çalıştığınız telefon numarası veya e-posta adresi başka bir müşteriye ait olabilir.");
                        TempData["ErrorMessage"] = "Müşteri güncellenirken bir hata oluştu: Telefon/E-posta zaten mevcut olabilir.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Müşteri güncellenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Müşteri güncellenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            MusteriViewModel musteriViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetMusteriById (reusing for delete confirmation view)
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMusteriById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                musteriViewModel = new MusteriViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Ad = reader.IsDBNull(reader.GetOrdinal("Ad")) ? null : reader.GetString("Ad"),
                                    Soyad = reader.IsDBNull(reader.GetOrdinal("Soyad")) ? null : reader.GetString("Soyad"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Müşteri silme için yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (musteriViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek müşteri bulunamadı.";
                return HttpNotFound();
            }
            return View(musteriViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_DeleteMusteriById
                    using (MySqlCommand cmd = new MySqlCommand("sp_DeleteMusteriById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Müşteri başarıyla silindi.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Müşteri silinemedi veya bulunamadı.";
                        }
                    }
                }
                catch (MySqlException ex) when (ex.Number == 1451) // Foreign key violation
                {
                    TempData["ErrorMessage"] = "Bu müşteri başka kayıtlarla (örneğin adresler veya siparişlerle) ilişkili olduğu için silinemez.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Müşteri silinirken bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}