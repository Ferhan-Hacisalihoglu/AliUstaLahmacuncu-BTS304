using AliUsta.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web; // Not strictly needed here but often present
using System.Web.Mvc;

namespace AliUsta.Controllers
{
    [Authorize]
    public class TedarikciController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        private IEnumerable<SelectListItem> GetMalzemelerSelectList()
        {
            var malzemeler = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetMalzemelerForTedarikciSelectList
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMalzemelerForTedarikciSelectList", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                malzemeler.Add(new SelectListItem
                                {
                                    Value = reader.GetInt32("ID").ToString(),
                                    Text = reader.GetString("Adı")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error properly in a real application
                    System.Diagnostics.Debug.WriteLine("GetMalzemelerSelectList Hata: " + ex.Message);
                    TempData["ErrorMessage"] = "Malzeme listesi yüklenirken bir hata oluştu: " + ex.Message;
                }
            }
            return malzemeler;
        }

        public ActionResult Index()
        {
            List<TedarikciViewModel> tedarikciler = new List<TedarikciViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetAllTedarikcilerWithMalzeme
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllTedarikcilerWithMalzeme", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                tedarikciler.Add(new TedarikciViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta"),
                                    MalzemeID = reader.IsDBNull(reader.GetOrdinal("MalzemeID")) ? (int?)null : reader.GetInt32("MalzemeID"),
                                    MalzemeAdi = reader.IsDBNull(reader.GetOrdinal("MalzemeAdi")) ? "Belirtilmemiş" : reader.GetString("MalzemeAdi")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Tedarikçiler listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(tedarikciler);
        }

        public ActionResult Create()
        {
            var viewModel = new TedarikciViewModel
            {
                MalzemelerListesi = GetMalzemelerSelectList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TedarikciViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_CreateTedarikci
                        using (MySqlCommand cmd = new MySqlCommand("sp_CreateTedarikci", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_Adi", viewModel.Adi);
                            cmd.Parameters.AddWithValue("@p_Telefon", (object)viewModel.Telefon ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_EPosta", (object)viewModel.EPosta ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_MalzemeID", (object)viewModel.MalzemeID ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        TempData["SuccessMessage"] = "Tedarikçi başarıyla eklendi.";
                        return RedirectToAction("Index");
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
                    {
                        ModelState.AddModelError("", "Bu tedarikçi adı, telefon veya e-posta zaten kayıtlı olabilir.");
                        TempData["ErrorMessage"] = "Tedarikçi eklenirken bir hata oluştu: Tedarikçi bilgileri zaten mevcut.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Tedarikçi eklenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Tedarikçi eklenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            viewModel.MalzemelerListesi = GetMalzemelerSelectList(); // Repopulate if validation fails
            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            TedarikciViewModel tedarikciViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetTedarikciByIdForEdit
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetTedarikciByIdForEdit", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tedarikciViewModel = new TedarikciViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta"),
                                    MalzemeID = reader.IsDBNull(reader.GetOrdinal("MalzemeID")) ? (int?)null : reader.GetInt32("MalzemeID")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Tedarikçi bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (tedarikciViewModel == null)
            {
                TempData["ErrorMessage"] = "Tedarikçi bulunamadı.";
                return HttpNotFound();
            }
            tedarikciViewModel.MalzemelerListesi = GetMalzemelerSelectList();
            return View(tedarikciViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TedarikciViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_UpdateTedarikci
                        using (MySqlCommand cmd = new MySqlCommand("sp_UpdateTedarikci", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_ID", viewModel.ID);
                            cmd.Parameters.AddWithValue("@p_Adi", viewModel.Adi);
                            cmd.Parameters.AddWithValue("@p_Telefon", (object)viewModel.Telefon ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_EPosta", (object)viewModel.EPosta ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_MalzemeID", (object)viewModel.MalzemeID ?? DBNull.Value);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["SuccessMessage"] = "Tedarikçi başarıyla güncellendi.";
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Tedarikçi güncellenemedi (muhtemelen bulunamadı veya veri değişmedi).";
                                ModelState.AddModelError("", "Tedarikçi güncellenemedi.");
                            }
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
                    {
                        ModelState.AddModelError("", "Güncellemeye çalıştığınız tedarikçi adı, telefon veya e-posta başka bir kayda ait olabilir.");
                        TempData["ErrorMessage"] = "Tedarikçi güncellenirken bir hata oluştu: Tedarikçi bilgileri zaten mevcut olabilir.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Tedarikçi güncellenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Tedarikçi güncellenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            viewModel.MalzemelerListesi = GetMalzemelerSelectList(); // Repopulate if validation fails
            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            TedarikciViewModel tedarikciViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetTedarikciByIdForDeleteView
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetTedarikciByIdForDeleteView", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tedarikciViewModel = new TedarikciViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta"),
                                    MalzemeAdi = reader.IsDBNull(reader.GetOrdinal("MalzemeAdi")) ? "Belirtilmemiş" : reader.GetString("MalzemeAdi")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Tedarikçi silme için yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (tedarikciViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek tedarikçi bulunamadı.";
                return HttpNotFound();
            }
            return View(tedarikciViewModel);
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
                    // Use stored procedure sp_DeleteTedarikciById
                    using (MySqlCommand cmd = new MySqlCommand("sp_DeleteTedarikciById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Tedarikçi başarıyla silindi.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Tedarikçi silinemedi veya bulunamadı.";
                        }
                    }
                }
                catch (MySqlException ex) when (ex.Number == 1451) // Foreign key violation
                {
                    TempData["ErrorMessage"] = "Bu tedarikçi başka kayıtlarla (örneğin alım kayıtları) ilişkili olduğu için silinemez.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Tedarikçi silinirken bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}