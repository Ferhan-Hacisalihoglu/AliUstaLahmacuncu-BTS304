using AliUsta.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace AliUsta.Controllers
{
    [Authorize]
    public class SiparisController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
        // private static Random random = new Random(); // Random logic moved to SP

        // HELPER METODLAR: Dropdown listelerini doldurmak için
        private IEnumerable<SelectListItem> GetMusterilerSelectList()
        {
            var list = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetMusterilerForSiparisSelectList
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMusterilerForSiparisSelectList", conn))
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

        private IEnumerable<SelectListItem> GetUrunlerSelectList()
        {
            var list = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetUrunlerForSiparisSelectList
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetUrunlerForSiparisSelectList", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new SelectListItem { Value = reader["ID"].ToString(), Text = reader["Adı"].ToString() });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GetUrunlerSelectList Hata: {ex.Message}");
                }
            }
            return list;
        }

        // GetPersonelIDsByGorev is no longer needed as SP handles Usta/Kurye assignment

        // AJAX İÇİN METODLAR
        [HttpGet]
        public JsonResult GetAdreslerByMusteri(int musteriId)
        {
            var adresler = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetAdreslerByMusteriIdForSiparis
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAdreslerByMusteriIdForSiparis", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_MusteriID", musteriId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                adresler.Add(new SelectListItem
                                {
                                    Value = reader["ID"].ToString(),
                                    Text = reader["Açıklama"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Adresler getirilirken hata: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = true, data = adresler }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetUrunFiyati(int urunId)
        {
            decimal fiyat = 0;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetUrunFiyatiByIdForSiparis
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetUrunFiyatiByIdForSiparis", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_UrunID", urunId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            fiyat = Convert.ToDecimal(result);
                        }
                        else
                        {
                            return Json(new { fiyat = 0, error = "Ürün fiyatı bulunamadı." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GetUrunFiyati Hata: {ex.Message}");
                    return Json(new { fiyat = 0, error = "Fiyat alınırken hata: " + ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { fiyat = fiyat }, JsonRequestBehavior.AllowGet);
        }

        // CRUD ACTIONS
        public ActionResult Index()
        {
            List<SiparisViewModel> siparisler = new List<SiparisViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetAllSiparislerWithDetails
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllSiparislerWithDetails", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                siparisler.Add(new SiparisViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    MusteriAdiSoyadi = reader.GetString("MusteriAdiSoyadi"),
                                    UrunAdi = reader.GetString("UrunAdi"),
                                    UstaAdiSoyadi = reader.GetString("UstaAdiSoyadi"),
                                    KuryeAdiSoyadi = reader.GetString("KuryeAdiSoyadi"),
                                    Miktar = reader.GetInt32("Miktar"),
                                    Fiyat = reader.GetDecimal("Fiyat"),
                                    Tarih = reader.GetDateTime("Tarih"),
                                    AdresAciklamasi = reader.GetString("AdresAciklamasi")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Siparişler listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(siparisler);
        }

        public ActionResult Create()
        {
            var viewModel = new SiparisViewModel
            {
                MusterilerListesi = GetMusterilerSelectList(),
                UrunlerListesi = GetUrunlerSelectList(),
                Tarih = DateTime.Now // Set default date
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SiparisViewModel viewModel)
        {
            ModelState.Remove("UstaID"); // UstaID will be assigned by SP
            ModelState.Remove("KuryeID"); // KuryeID will be assigned by SP
            ModelState.Remove("Fiyat");   // Fiyat will be calculated by SP

            if (viewModel.MusteriID == 0) ModelState.AddModelError("MusteriID", "Müşteri seçimi zorunludur.");
            if (viewModel.UrunID == 0) ModelState.AddModelError("UrunID", "Ürün seçimi zorunludur.");
            if (viewModel.AdresID == 0) ModelState.AddModelError("AdresID", "Adres seçimi zorunludur.");
            if (viewModel.Miktar <= 0) ModelState.AddModelError("Miktar", "Miktar 0'dan büyük olmalıdır.");


            if (ModelState.IsValid)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        // Use sp_ProcessSiparisCreation
                        using (MySqlCommand cmd = new MySqlCommand("sp_ProcessSiparisCreation", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_MusteriID", viewModel.MusteriID);
                            cmd.Parameters.AddWithValue("@p_UrunID", viewModel.UrunID);
                            cmd.Parameters.AddWithValue("@p_AdresID", viewModel.AdresID);
                            cmd.Parameters.AddWithValue("@p_SiparisMiktar", viewModel.Miktar);
                            cmd.Parameters.AddWithValue("@p_Tarih", viewModel.Tarih == DateTime.MinValue ? DateTime.Now : viewModel.Tarih); // Ensure Tarih is set

                            MySqlParameter pGeneratedSiparisID = new MySqlParameter("@p_GeneratedSiparisID", MySqlDbType.Int32) { Direction = ParameterDirection.Output };
                            MySqlParameter pSuccess = new MySqlParameter("@p_Success", MySqlDbType.Byte) { Direction = ParameterDirection.Output };
                            MySqlParameter pErrorMessage = new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 1024) { Direction = ParameterDirection.Output }; // Matched SP size

                            cmd.Parameters.Add(pGeneratedSiparisID);
                            cmd.Parameters.Add(pSuccess);
                            cmd.Parameters.Add(pErrorMessage);

                            cmd.ExecuteNonQuery();

                            bool success = Convert.ToBoolean(pSuccess.Value);
                            string spMessage = pErrorMessage.Value.ToString();

                            if (success)
                            {
                                TempData["SuccessMessage"] = spMessage; // "Sipariş başarıyla oluşturuldu."
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                ModelState.AddModelError("", spMessage);
                                TempData["ErrorMessage"] = spMessage;
                            }
                        }
                    }
                    catch (Exception ex) // Catch C# level exceptions
                    {
                        ModelState.AddModelError("", "Sipariş oluşturulurken beklenmedik bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Sipariş oluşturulurken beklenmedik bir hata oluştu: " + ex.Message;
                    }
                }
            }

            // Repopulate lists if validation fails or SP fails
            viewModel.MusterilerListesi = GetMusterilerSelectList();
            viewModel.UrunlerListesi = GetUrunlerSelectList();
            // Adres listesi JS ile yüklendiği için burada tekrar yüklemeye gerek yok (MusteriID değişmediyse)
            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            SiparisViewModel siparisViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Use sp_GetSiparisByIdForDeleteView
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetSiparisByIdForDeleteView", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_SiparisID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                siparisViewModel = new SiparisViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    MusteriAdiSoyadi = reader.GetString("MusteriAdiSoyadi"),
                                    UrunAdi = reader.GetString("UrunAdi"),
                                    UstaAdiSoyadi = reader.GetString("UstaAdiSoyadi"),
                                    KuryeAdiSoyadi = reader.GetString("KuryeAdiSoyadi"),
                                    Miktar = reader.GetInt32("Miktar"),
                                    Fiyat = reader.GetDecimal("Fiyat"),
                                    Tarih = reader.GetDateTime("Tarih"),
                                    AdresAciklamasi = reader.GetString("AdresAciklamasi")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Sipariş detayı yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (siparisViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek sipariş bulunamadı.";
                return HttpNotFound();
            }
            return View(siparisViewModel);
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
                    // Use sp_ProcessSiparisDeletion
                    using (MySqlCommand cmd = new MySqlCommand("sp_ProcessSiparisDeletion", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_SiparisID", id);

                        MySqlParameter pSuccess = new MySqlParameter("@p_Success", MySqlDbType.Byte) { Direction = ParameterDirection.Output };
                        MySqlParameter pErrorMessage = new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 255) { Direction = ParameterDirection.Output };

                        cmd.Parameters.Add(pSuccess);
                        cmd.Parameters.Add(pErrorMessage);

                        cmd.ExecuteNonQuery();

                        bool success = Convert.ToBoolean(pSuccess.Value);
                        string spMessage = pErrorMessage.Value.ToString();

                        if (success)
                        {
                            TempData["SuccessMessage"] = spMessage; // "Sipariş başarıyla silindi ve stoklar güncellendi."
                        }
                        else
                        {
                            TempData["ErrorMessage"] = spMessage;
                        }
                    }
                }
                catch (Exception ex) // Catch C# level exceptions
                {
                    TempData["ErrorMessage"] = "Sipariş silinirken beklenmedik bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}