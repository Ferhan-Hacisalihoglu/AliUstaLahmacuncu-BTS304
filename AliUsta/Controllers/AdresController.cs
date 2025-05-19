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
    public class AdresController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        private IEnumerable<SelectListItem> GetMusterilerSelectList()
        {
            var musteriler = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetMusterilerForSelectList
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMusterilerForSelectList", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                musteriler.Add(new SelectListItem
                                {
                                    Value = reader.GetInt32("ID").ToString(),
                                    Text = reader.GetString("AdSoyad")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Consider more robust logging or error handling
                    System.Diagnostics.Debug.WriteLine("GetMusterilerSelectList Hata: " + ex.Message);
                    // Optionally, set a TempData message if this list is critical for a view
                    // TempData["ErrorMessage"] = "Müşteri listesi yüklenemedi: " + ex.Message;
                }
            }
            return musteriler;
        }

        public ActionResult Index()
        {
            List<AdresViewModel> adresler = new List<AdresViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetAllAdreslerWithMusteri
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllAdreslerWithMusteri", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                adresler.Add(new AdresViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Aciklama = reader.IsDBNull(reader.GetOrdinal("Açıklama")) ? null : reader.GetString("Açıklama"),
                                    MusteriID = reader.GetInt32("MusteriID"),
                                    MusteriAdiSoyadi = reader.IsDBNull(reader.GetOrdinal("MusteriAdiSoyadi")) ? "Bilinmiyor" : reader.GetString("MusteriAdiSoyadi")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Adresler listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(adresler);
        }

        public ActionResult Create()
        {
            var viewModel = new AdresViewModel
            {
                MusterilerListesi = GetMusterilerSelectList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdresViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_CreateAdres
                        using (MySqlCommand cmd = new MySqlCommand("sp_CreateAdres", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_Aciklama", (object)viewModel.Aciklama ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_MusteriID", viewModel.MusteriID);
                            cmd.ExecuteNonQuery();
                        }
                        TempData["SuccessMessage"] = "Adres başarıyla eklendi.";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Adres eklenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Adres eklenirken bir hata oluştu: " + ex.Message; // Redundant with ModelState
                    }
                }
            }
            viewModel.MusterilerListesi = GetMusterilerSelectList(); // Repopulate if validation fails
            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            AdresViewModel adresViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetAdresById
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAdresById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                adresViewModel = new AdresViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Aciklama = reader.IsDBNull(reader.GetOrdinal("Açıklama")) ? null : reader.GetString("Açıklama"),
                                    MusteriID = reader.GetInt32("MusteriID")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Adres bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index"); // Or show an error view
                }
            }

            if (adresViewModel == null)
            {
                TempData["ErrorMessage"] = "Adres bulunamadı.";
                return HttpNotFound();
            }
            adresViewModel.MusterilerListesi = GetMusterilerSelectList();
            return View(adresViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdresViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_UpdateAdres
                        using (MySqlCommand cmd = new MySqlCommand("sp_UpdateAdres", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_ID", viewModel.ID);
                            cmd.Parameters.AddWithValue("@p_Aciklama", (object)viewModel.Aciklama ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_MusteriID", viewModel.MusteriID);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["SuccessMessage"] = "Adres başarıyla güncellendi.";
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Adres güncellenemedi (muhtemelen bulunamadı veya veri değişmedi).";
                                ModelState.AddModelError("", "Adres güncellenemedi."); // More user-friendly
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Adres güncellenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Adres güncellenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            viewModel.MusterilerListesi = GetMusterilerSelectList(); // Repopulate if validation fails
            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            AdresViewModel adresViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetAdresForDeleteViewById
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAdresForDeleteViewById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                adresViewModel = new AdresViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Aciklama = reader.IsDBNull(reader.GetOrdinal("Açıklama")) ? null : reader.GetString("Açıklama"),
                                    MusteriAdiSoyadi = reader.IsDBNull(reader.GetOrdinal("MusteriAdiSoyadi")) ? "Bilinmiyor" : reader.GetString("MusteriAdiSoyadi")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Adres silme için yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (adresViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek adres bulunamadı.";
                return HttpNotFound();
            }
            return View(adresViewModel);
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
                    // Use stored procedure sp_DeleteAdresById
                    using (MySqlCommand cmd = new MySqlCommand("sp_DeleteAdresById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Adres başarıyla silindi.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Adres silinemedi veya bulunamadı.";
                        }
                    }
                }
                catch (MySqlException ex) when (ex.Number == 1451) // Foreign key violation
                {
                    TempData["ErrorMessage"] = "Bu adres başka kayıtlarla ilişkili olduğu için silinemez (örneğin siparişler). Lütfen önce ilişkili kayıtları silin veya güncelleyin.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Adres silinirken bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}