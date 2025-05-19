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
    public class MalzemeController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public ActionResult Index()
        {
            List<MalzemeViewModel> malzemeler = new List<MalzemeViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetAllMalzemeler
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllMalzemeler", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                malzemeler.Add(new MalzemeViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı"),
                                    // Ensure your SP sp_GetAllMalzemeler selects `Stok Miktarı`
                                    StokMiktari = reader.GetInt32(reader.GetOrdinal("Stok Miktarı"))
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Malzemeler listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(malzemeler);
        }

        public ActionResult Create()
        {
            var viewModel = new MalzemeViewModel();
            // StokMiktari will likely default to 0 or be set by a different process (e.g., Alim)
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MalzemeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_CreateMalzeme
                        using (MySqlCommand cmd = new MySqlCommand("sp_CreateMalzeme", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_Adi", (object)viewModel.Adi ?? DBNull.Value);
                            // StokMiktari is not passed here as the SP (and original query) doesn't set it.
                            // It defaults in DB or is handled by Alim.
                            cmd.ExecuteNonQuery();
                        }
                        TempData["SuccessMessage"] = "Malzeme başarıyla eklendi.";
                        return RedirectToAction("Index");
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry for unique key
                    {
                        ModelState.AddModelError("Adi", "Bu malzeme adı zaten mevcut.");
                        TempData["ErrorMessage"] = "Malzeme eklenirken bir hata oluştu: Bu malzeme adı zaten mevcut.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Malzeme eklenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Malzeme eklenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            MalzemeViewModel malzemeViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetMalzemeByIdForEdit
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMalzemeByIdForEdit", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                malzemeViewModel = new MalzemeViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı")
                                    // StokMiktari is not fetched here as per original code and sp_GetMalzemeByIdForEdit
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Malzeme bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (malzemeViewModel == null)
            {
                TempData["ErrorMessage"] = "Malzeme bulunamadı.";
                return HttpNotFound();
            }
            return View(malzemeViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MalzemeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_UpdateMalzemeAdi
                        using (MySqlCommand cmd = new MySqlCommand("sp_UpdateMalzemeAdi", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_ID", viewModel.ID);
                            cmd.Parameters.AddWithValue("@p_Adi", (object)viewModel.Adi ?? DBNull.Value);
                            // StokMiktari is not updated here as per original code and sp_UpdateMalzemeAdi
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["SuccessMessage"] = "Malzeme başarıyla güncellendi.";
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Malzeme güncellenemedi (muhtemelen bulunamadı veya veri değişmedi).";
                                ModelState.AddModelError("", "Malzeme güncellenemedi.");
                            }
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry for unique key
                    {
                        ModelState.AddModelError("Adi", "Bu malzeme adı zaten mevcut.");
                        TempData["ErrorMessage"] = "Malzeme güncellenirken bir hata oluştu: Bu malzeme adı zaten mevcut.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Malzeme güncellenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Malzeme güncellenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            MalzemeViewModel malzemeViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetMalzemeByIdForDelete
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMalzemeByIdForDelete", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                malzemeViewModel = new MalzemeViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı"),
                                    StokMiktari = reader.GetInt32(reader.GetOrdinal("Stok Miktarı"))
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Malzeme silme için yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (malzemeViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek malzeme bulunamadı.";
                return HttpNotFound();
            }
            return View(malzemeViewModel);
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
                    // Use stored procedure sp_DeleteMalzemeById
                    using (MySqlCommand cmd = new MySqlCommand("sp_DeleteMalzemeById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Malzeme başarıyla silindi.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Malzeme silinemedi veya bulunamadı.";
                        }
                    }
                }
                catch (MySqlException ex) when (ex.Number == 1451) // Foreign key violation
                {
                    TempData["ErrorMessage"] = "Bu malzeme, bir tedarikçiyle, alım kaydıyla veya ürünle ilişkili olduğu için silinemez. Lütfen önce ilgili kayıtları güncelleyin veya silin.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Malzeme silinirken bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}