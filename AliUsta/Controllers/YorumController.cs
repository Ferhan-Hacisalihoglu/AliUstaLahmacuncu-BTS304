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
    public class YorumController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        private IEnumerable<SelectListItem> GetMusterilerSelectList()
        {
            var list = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetMusterilerForYorumSelectList
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMusterilerForYorumSelectList", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new SelectListItem { Value = reader["ID"].ToString(), Text = reader["AdSoyad"].ToString() });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GetMusterilerSelectList Hata: {ex.Message}");
                }
            }
            return list;
        }

        [HttpGet]
        public JsonResult GetSiparislerByMusteri(int musteriId)
        {
            var siparisler = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetSiparislerByMusteriForYorum
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetSiparislerByMusteriForYorum", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_MusteriID", musteriId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                siparisler.Add(new SelectListItem
                                {
                                    Value = reader["ID"].ToString(),
                                    Text = reader["SiparisBilgisi"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Siparişler getirilirken hata: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = true, data = siparisler }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index()
        {
            List<YorumViewModel> yorumlar = new List<YorumViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetAllYorumlarWithDetails
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllYorumlarWithDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                yorumlar.Add(new YorumViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    MusteriID = reader.GetInt32("MusteriID"),
                                    MusteriAdiSoyadi = reader.GetString("MusteriAdiSoyadi"),
                                    SiparisID = reader.GetInt32("SiparişID"),
                                    SiparisDetayi = $"Sipariş #{reader.GetInt32("SiparişID")} - Ürün: {reader.GetString("SiparisUrunAdi")}",
                                    Puan = reader.GetInt32("Puan"),
                                    Aciklama = reader.IsDBNull(reader.GetOrdinal("Açıklama")) ? null : reader.GetString("Açıklama"),
                                    Tarih = reader.GetDateTime("Tarih")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Yorumlar listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(yorumlar);
        }

        public ActionResult Create()
        {
            var viewModel = new YorumViewModel
            {
                MusterilerListesi = GetMusterilerSelectList(),
                SiparislerListesi = new List<SelectListItem>(), // Initially empty, populated by AJAX
                Tarih = DateTime.Now
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(YorumViewModel viewModel)
        {
            if (viewModel.MusteriID == 0) ModelState.AddModelError("MusteriID", "Müşteri seçimi zorunludur.");
            if (viewModel.SiparisID == 0) ModelState.AddModelError("SiparisID", "Sipariş seçimi zorunludur.");
            if (viewModel.Puan < 1 || viewModel.Puan > 5) ModelState.AddModelError("Puan", "Puan 1 ile 5 arasında olmalıdır.");


            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use sp_CreateYorumWithCheck
                        using (MySqlCommand cmd = new MySqlCommand("sp_CreateYorumWithCheck", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_MusteriID", viewModel.MusteriID);
                            cmd.Parameters.AddWithValue("@p_SiparisID", viewModel.SiparisID);
                            cmd.Parameters.AddWithValue("@p_Puan", viewModel.Puan);
                            cmd.Parameters.AddWithValue("@p_Aciklama", (object)viewModel.Aciklama ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_Tarih", viewModel.Tarih);

                            MySqlParameter pSuccess = new MySqlParameter("@p_Success", MySqlDbType.Byte) { Direction = ParameterDirection.Output };
                            MySqlParameter pErrorMessage = new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 512) { Direction = ParameterDirection.Output };
                            cmd.Parameters.Add(pSuccess);
                            cmd.Parameters.Add(pErrorMessage);

                            cmd.ExecuteNonQuery(); // Transaction is handled within the SP

                            bool success = Convert.ToBoolean(pSuccess.Value);
                            string spMessage = pErrorMessage.Value.ToString();

                            if (success)
                            {
                                TempData["SuccessMessage"] = spMessage; // "Yorum başarıyla eklendi."
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                ModelState.AddModelError("", spMessage);
                                TempData["ErrorMessage"] = spMessage;
                            }
                        }
                    }
                    catch (Exception ex) // Catch C# level exceptions if SP call itself fails
                    {
                        ModelState.AddModelError("", "Yorum eklenirken beklenmedik bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Yorum eklenirken beklenmedik bir hata oluştu: " + ex.Message;
                    }
                }
            }

            // Repopulate lists if validation fails or SP fails
            viewModel.MusterilerListesi = GetMusterilerSelectList();
            if (viewModel.MusteriID > 0)
            {
                // If you want to re-populate SiparislerListesi on server-side validation failure (though AJAX usually handles this for good UX)
                // you would call a method similar to GetSiparislerByMusteri here, but it's generally better to rely on the AJAX call
                // from the client-side if MusteriID is already selected.
                // For simplicity, we'll leave it to be refilled by AJAX if the view re-renders.
                viewModel.SiparislerListesi = new List<SelectListItem>(); // Or try to refill if you have a synchronous way
            }
            else
            {
                viewModel.SiparislerListesi = new List<SelectListItem>();
            }
            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            YorumViewModel yorumViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetYorumByIdForDeleteView
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetYorumByIdForDeleteView", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_YorumID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                yorumViewModel = new YorumViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    MusteriAdiSoyadi = reader.GetString("MusteriAdiSoyadi"),
                                    SiparisDetayi = $"Sipariş #{reader.GetInt32("SiparisIDnum")} - Ürün: {reader.GetString("SiparisUrunAdi")}",
                                    Puan = reader.GetInt32("Puan"),
                                    Aciklama = reader.IsDBNull(reader.GetOrdinal("Açıklama")) ? null : reader.GetString("Açıklama"),
                                    Tarih = reader.GetDateTime("Tarih")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Yorum silme için yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (yorumViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek yorum bulunamadı.";
                return HttpNotFound();
            }
            return View(yorumViewModel);
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
                    // Use sp_DeleteYorumById
                    using (MySqlCommand cmd = new MySqlCommand("sp_DeleteYorumById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_YorumID", id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Yorum silinemedi veya bulunamadı.";
                        }
                    }
                }
                catch (MySqlException ex) when (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Bu yorum başka kayıtlarla ilişkili olduğu için silinemez.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Yorum silinirken bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}