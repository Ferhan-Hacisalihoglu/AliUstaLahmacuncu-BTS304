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
    public class KategoriController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public ActionResult Index()
        {
            List<KategoriViewModel> kategoriler = new List<KategoriViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetAllKategoriler
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllKategoriler", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                kategoriler.Add(new KategoriViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Kategoriler listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(kategoriler);
        }

        public ActionResult Create()
        {
            return View(new KategoriViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(KategoriViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_CreateKategori
                        using (MySqlCommand cmd = new MySqlCommand("sp_CreateKategori", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_Adi", (object)viewModel.Adi ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
                        return RedirectToAction("Index");
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry for unique key
                    {
                        ModelState.AddModelError("Adi", "Bu kategori adı zaten mevcut.");
                        TempData["ErrorMessage"] = "Kategori eklenirken bir hata oluştu: Bu kategori adı zaten mevcut.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Kategori eklenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Kategori eklenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            KategoriViewModel kategoriViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetKategoriById
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetKategoriById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                kategoriViewModel = new KategoriViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Kategori bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (kategoriViewModel == null)
            {
                TempData["ErrorMessage"] = "Kategori bulunamadı.";
                return HttpNotFound();
            }
            return View(kategoriViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(KategoriViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_UpdateKategori
                        using (MySqlCommand cmd = new MySqlCommand("sp_UpdateKategori", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_ID", viewModel.ID);
                            cmd.Parameters.AddWithValue("@p_Adi", (object)viewModel.Adi ?? DBNull.Value);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Kategori güncellenemedi (muhtemelen bulunamadı veya veri değişmedi).";
                                ModelState.AddModelError("", "Kategori güncellenemedi.");
                            }
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry for unique key
                    {
                        ModelState.AddModelError("Adi", "Bu kategori adı zaten mevcut.");
                        TempData["ErrorMessage"] = "Kategori güncellenirken bir hata oluştu: Bu kategori adı zaten mevcut.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Kategori güncellenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Kategori güncellenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            KategoriViewModel kategoriViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetKategoriById (can reuse for delete confirmation view)
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetKategoriById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                kategoriViewModel = new KategoriViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Kategori silme için yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (kategoriViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek kategori bulunamadı.";
                return HttpNotFound();
            }
            return View(kategoriViewModel);
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
                    // Use stored procedure sp_DeleteKategoriById
                    using (MySqlCommand cmd = new MySqlCommand("sp_DeleteKategoriById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Kategori silinemedi veya bulunamadı.";
                        }
                    }
                }
                catch (MySqlException ex) when (ex.Number == 1451) // Foreign key violation
                {
                    TempData["ErrorMessage"] = "Bu kategori başka kayıtlarla (örneğin ürünlerle) ilişkili olduğu için silinemez.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Kategori silinirken bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}