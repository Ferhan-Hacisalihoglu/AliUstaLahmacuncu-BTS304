using AliUsta.Models; // ViewModel'lerinizin bulunduğu namespace
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.Mvc;

namespace AliUsta.Controllers
{
    [Authorize]
    public class PersonelController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        private IEnumerable<SelectListItem> GetGorevlerSelectList()
        {
            var gorevler = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetGorevlerForSelectList
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetGorevlerForSelectList", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                gorevler.Add(new SelectListItem
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
                    // Consider more robust logging for a production environment
                    System.Diagnostics.Debug.WriteLine("GetGorevlerSelectList Hata: " + ex.Message);
                    // Optionally set TempData if this list is critical for view rendering
                    // TempData["ErrorMessage"] = "Görev listesi yüklenemedi: " + ex.Message;
                }
            }
            return gorevler;
        }

        public ActionResult Index()
        {
            List<PersonelViewModel> personeller = new List<PersonelViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetAllPersonellerWithGorev
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllPersonellerWithGorev", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                personeller.Add(new PersonelViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Ad = reader.IsDBNull(reader.GetOrdinal("Ad")) ? null : reader.GetString("Ad"),
                                    Soyad = reader.IsDBNull(reader.GetOrdinal("Soyad")) ? null : reader.GetString("Soyad"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta"),
                                    Maas = reader.IsDBNull(reader.GetOrdinal("Maaş")) ? 0 : reader.GetDecimal("Maaş"),
                                    GorevID = reader.IsDBNull(reader.GetOrdinal("GörevID")) ? (int?)null : reader.GetInt32("GörevID"),
                                    GorevAdi = reader.IsDBNull(reader.GetOrdinal("GorevAdi")) ? "Belirtilmemiş" : reader.GetString("GorevAdi")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Personeller listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(personeller);
        }

        public ActionResult Create()
        {
            var viewModel = new PersonelViewModel
            {
                GorevlerListesi = GetGorevlerSelectList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PersonelViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_CreatePersonel
                        using (MySqlCommand cmd = new MySqlCommand("sp_CreatePersonel", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_Ad", viewModel.Ad);
                            cmd.Parameters.AddWithValue("@p_Soyad", viewModel.Soyad);
                            cmd.Parameters.AddWithValue("@p_Telefon", (object)viewModel.Telefon ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_EPosta", (object)viewModel.EPosta ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_Maas", viewModel.Maas); // Assuming Maas is not nullable in ViewModel or has a default
                            cmd.Parameters.AddWithValue("@p_GorevID", (object)viewModel.GorevID ?? DBNull.Value);
                            cmd.ExecuteNonQuery();
                        }
                        TempData["SuccessMessage"] = "Personel başarıyla eklendi.";
                        return RedirectToAction("Index");
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry for unique key (e.g., EPosta, Telefon if unique)
                    {
                        ModelState.AddModelError("", "Bu e-posta veya telefon numarası zaten kayıtlı olabilir.");
                        TempData["ErrorMessage"] = "Personel eklenirken bir hata oluştu: E-posta/Telefon zaten kayıtlı.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Personel eklenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Personel eklenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            viewModel.GorevlerListesi = GetGorevlerSelectList(); // Repopulate if validation fails
            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            PersonelViewModel personelViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetPersonelByIdForEdit
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetPersonelByIdForEdit", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                personelViewModel = new PersonelViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Ad = reader.IsDBNull(reader.GetOrdinal("Ad")) ? null : reader.GetString("Ad"),
                                    Soyad = reader.IsDBNull(reader.GetOrdinal("Soyad")) ? null : reader.GetString("Soyad"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta"),
                                    Maas = reader.IsDBNull(reader.GetOrdinal("Maaş")) ? 0 : reader.GetDecimal("Maaş"),
                                    GorevID = reader.IsDBNull(reader.GetOrdinal("GörevID")) ? (int?)null : reader.GetInt32("GörevID")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Personel bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (personelViewModel == null)
            {
                TempData["ErrorMessage"] = "Personel bulunamadı.";
                return HttpNotFound();
            }
            personelViewModel.GorevlerListesi = GetGorevlerSelectList();
            return View(personelViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PersonelViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use stored procedure sp_UpdatePersonel
                        using (MySqlCommand cmd = new MySqlCommand("sp_UpdatePersonel", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_ID", viewModel.ID);
                            cmd.Parameters.AddWithValue("@p_Ad", viewModel.Ad);
                            cmd.Parameters.AddWithValue("@p_Soyad", viewModel.Soyad);
                            cmd.Parameters.AddWithValue("@p_Telefon", (object)viewModel.Telefon ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_EPosta", (object)viewModel.EPosta ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_Maas", viewModel.Maas);
                            cmd.Parameters.AddWithValue("@p_GorevID", (object)viewModel.GorevID ?? DBNull.Value);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                TempData["SuccessMessage"] = "Personel başarıyla güncellendi.";
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Personel güncellenemedi (muhtemelen bulunamadı veya veri değişmedi).";
                                ModelState.AddModelError("", "Personel güncellenemedi.");
                            }
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1062) // Duplicate entry
                    {
                        ModelState.AddModelError("", "Güncellemeye çalıştığınız e-posta veya telefon başka bir personele ait olabilir.");
                        TempData["ErrorMessage"] = "Personel güncellenirken bir hata oluştu: E-posta/Telefon zaten mevcut olabilir.";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Personel güncellenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Personel güncellenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            viewModel.GorevlerListesi = GetGorevlerSelectList(); // Repopulate if validation fails
            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            PersonelViewModel personelViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use stored procedure sp_GetPersonelByIdForDelete
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetPersonelByIdForDelete", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                personelViewModel = new PersonelViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Ad = reader.IsDBNull(reader.GetOrdinal("Ad")) ? null : reader.GetString("Ad"),
                                    Soyad = reader.IsDBNull(reader.GetOrdinal("Soyad")) ? null : reader.GetString("Soyad"),
                                    Telefon = reader.IsDBNull(reader.GetOrdinal("Telefon")) ? null : reader.GetString("Telefon"),
                                    EPosta = reader.IsDBNull(reader.GetOrdinal("EPosta")) ? null : reader.GetString("EPosta"),
                                    Maas = reader.IsDBNull(reader.GetOrdinal("Maaş")) ? 0 : reader.GetDecimal("Maaş"),
                                    GorevAdi = reader.IsDBNull(reader.GetOrdinal("GorevAdi")) ? "Belirtilmemiş" : reader.GetString("GorevAdi")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Personel silme için yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (personelViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek personel bulunamadı.";
                return HttpNotFound();
            }
            return View(personelViewModel);
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
                    // Use stored procedure sp_DeletePersonelById
                    using (MySqlCommand cmd = new MySqlCommand("sp_DeletePersonelById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_ID", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Personel başarıyla silindi.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Personel silinemedi veya bulunamadı.";
                        }
                    }
                }
                catch (MySqlException ex) when (ex.Number == 1451) // Foreign key violation
                {
                    TempData["ErrorMessage"] = "Bu personel başka kayıtlarla (örneğin atandığı görevler veya siparişlerle) ilişkili olduğu için silinemez.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Personel silinirken bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}