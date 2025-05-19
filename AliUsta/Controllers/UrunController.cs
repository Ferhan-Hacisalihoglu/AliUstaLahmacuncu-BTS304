using AliUsta.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace AliUsta.Controllers
{
    [Authorize]
    public class UrunController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        private IEnumerable<SelectListItem> GetKategorilerSelectList()
        {
            var kategoriler = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetKategorilerForUrunSelectList", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                kategoriler.Add(new SelectListItem
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
                    System.Diagnostics.Debug.WriteLine("GetKategorilerSelectList Hata: " + ex.Message);
                }
            }
            return kategoriler;
        }

        private IEnumerable<SelectListItem> GetMalzemelerSelectList()
        {
            var malzemeler = new List<SelectListItem>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetMalzemelerForUrunSelectList", conn))
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
                    System.Diagnostics.Debug.WriteLine("GetMalzemelerSelectList Hata: " + ex.Message);
                }
            }
            return malzemeler;
        }

        public ActionResult Index()
        {
            List<UrunViewModel> urunler = new List<UrunViewModel>();
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetAllUrunlerWithKategori", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                urunler.Add(new UrunViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı"),
                                    Fiyat = reader.IsDBNull(reader.GetOrdinal("Fiyat")) ? 0 : reader.GetDecimal("Fiyat"),
                                    Aciklama = reader.IsDBNull(reader.GetOrdinal("Açıklama")) ? null : reader.GetString("Açıklama"),
                                    KategoriID = reader.IsDBNull(reader.GetOrdinal("KategoriID")) ? (int?)null : reader.GetInt32("KategoriID"),
                                    KategoriAdi = reader.IsDBNull(reader.GetOrdinal("KategoriAdi")) ? "Belirtilmemiş" : reader.GetString("KategoriAdi")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ürünler listelenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(urunler);
        }

        public ActionResult Create()
        {
            var viewModel = new UrunViewModel
            {
                KategorilerListesi = GetKategorilerSelectList(),
                YeniUrunMalzemeFormu = new UrunMalzemeViewModel // For the form part
                {
                    MalzemelerListesi = GetMalzemelerSelectList()
                },
                UrunMalzemeleri = new List<UrunMalzemeViewModel>() // Initialize list for added malzemeler
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UrunViewModel viewModel) // Removed urunMalzemeleriInput, assuming viewModel.UrunMalzemeleri is populated
        {
            if (ModelState.IsValid)
            {
                int newUrunId = 0;
                MySqlTransaction transaction = null;

                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();

                        // Create Urun
                        using (MySqlCommand cmdUrun = new MySqlCommand("sp_CreateUrun", conn, transaction))
                        {
                            cmdUrun.CommandType = CommandType.StoredProcedure;
                            cmdUrun.Parameters.AddWithValue("@p_Adi", viewModel.Adi);
                            cmdUrun.Parameters.AddWithValue("@p_Fiyat", viewModel.Fiyat);
                            cmdUrun.Parameters.AddWithValue("@p_Aciklama", (object)viewModel.Aciklama ?? DBNull.Value);
                            cmdUrun.Parameters.AddWithValue("@p_KategoriID", (object)viewModel.KategoriID ?? DBNull.Value);

                            MySqlParameter pNewUrunID = new MySqlParameter("@p_NewUrunID", MySqlDbType.Int32) { Direction = ParameterDirection.Output };
                            cmdUrun.Parameters.Add(pNewUrunID);

                            cmdUrun.ExecuteNonQuery();
                            newUrunId = Convert.ToInt32(pNewUrunID.Value);
                        }

                        if (newUrunId > 0)
                        {
                            if (viewModel.UrunMalzemeleri != null && viewModel.UrunMalzemeleri.Any())
                            {
                                foreach (var um in viewModel.UrunMalzemeleri)
                                {
                                    if (um.MalzemeID > 0 && um.Miktar > 0)
                                    {
                                        using (MySqlCommand cmdUM = new MySqlCommand("sp_AddUrunMalzeme", conn, transaction))
                                        {
                                            cmdUM.CommandType = CommandType.StoredProcedure;
                                            cmdUM.Parameters.AddWithValue("@p_UrunID", newUrunId);
                                            cmdUM.Parameters.AddWithValue("@p_MalzemeID", um.MalzemeID);
                                            cmdUM.Parameters.AddWithValue("@p_Miktar", um.Miktar);
                                            cmdUM.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                            transaction.Commit();
                            TempData["SuccessMessage"] = "Ürün ve malzemeleri başarıyla eklendi.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            transaction?.Rollback();
                            TempData["ErrorMessage"] = "Ürün eklendi ancak ID alınamadı.";
                            ModelState.AddModelError("", "Ürün eklendi ancak ID alınamadı.");
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1062)
                    {
                        transaction?.Rollback();
                        ModelState.AddModelError("Adi", "Bu ürün adı zaten mevcut.");
                        TempData["ErrorMessage"] = "Ürün eklenirken bir hata oluştu: Bu ürün adı zaten mevcut.";
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        ModelState.AddModelError("", "Ürün ve malzemeler eklenirken bir hata oluştu: " + ex.Message);
                        TempData["ErrorMessage"] = "Ürün ve malzemeler eklenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }

            viewModel.KategorilerListesi = GetKategorilerSelectList();
            if (viewModel.YeniUrunMalzemeFormu == null) viewModel.YeniUrunMalzemeFormu = new UrunMalzemeViewModel();
            viewModel.YeniUrunMalzemeFormu.MalzemelerListesi = GetMalzemelerSelectList();
            if (viewModel.UrunMalzemeleri == null) viewModel.UrunMalzemeleri = new List<UrunMalzemeViewModel>();
            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            UrunDetayMalzemeViewModel viewModel = new UrunDetayMalzemeViewModel();
            viewModel.UrunMalzemeleri = new List<UrunMalzemeViewModel>(); // Initialize
            viewModel.YeniUrunMalzeme = new UrunMalzemeViewModel(); // Initialize

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Get Urun Details
                    using (MySqlCommand cmdUrun = new MySqlCommand("sp_GetUrunByIdForEditView", conn))
                    {
                        cmdUrun.CommandType = CommandType.StoredProcedure;
                        cmdUrun.Parameters.AddWithValue("@p_UrunID", id);
                        using (MySqlDataReader readerUrun = cmdUrun.ExecuteReader())
                        {
                            if (readerUrun.Read())
                            {
                                viewModel.Urun = new UrunViewModel
                                {
                                    ID = readerUrun.GetInt32("ID"),
                                    Adi = readerUrun.IsDBNull(readerUrun.GetOrdinal("Adı")) ? null : readerUrun.GetString("Adı"),
                                    Fiyat = readerUrun.IsDBNull(readerUrun.GetOrdinal("Fiyat")) ? 0 : readerUrun.GetDecimal("Fiyat"),
                                    Aciklama = readerUrun.IsDBNull(readerUrun.GetOrdinal("Açıklama")) ? null : readerUrun.GetString("Açıklama"),
                                    KategoriID = readerUrun.IsDBNull(readerUrun.GetOrdinal("KategoriID")) ? (int?)null : readerUrun.GetInt32("KategoriID"),
                                };
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                                return HttpNotFound();
                            }
                        } // readerUrun closed
                    }
                    // Get Urun Malzemeleri
                    viewModel.UrunMalzemeleri = GetUrunMalzemeleri(id, conn);
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ürün veya malzeme bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
            viewModel.Urun.KategorilerListesi = GetKategorilerSelectList();
            viewModel.YeniUrunMalzeme.UrunID = id;
            viewModel.YeniUrunMalzeme.MalzemelerListesi = GetMalzemelerSelectList();
            return View(viewModel);
        }

        private List<UrunMalzemeViewModel> GetUrunMalzemeleri(int urunId, MySqlConnection existingConnection = null)
        {
            var urunMalzemeleri = new List<UrunMalzemeViewModel>();
            MySqlConnection conn = null;
            bool ownConnection = false;

            if (existingConnection == null)
            {
                conn = new MySqlConnection(_connectionString);
                ownConnection = true;
            }
            else
            {
                conn = existingConnection;
            }

            try
            {
                if (ownConnection) conn.Open();
                using (MySqlCommand cmdMalzemeler = new MySqlCommand("sp_GetUrunMalzemeleriByUrunId", conn))
                {
                    cmdMalzemeler.CommandType = CommandType.StoredProcedure;
                    cmdMalzemeler.Parameters.AddWithValue("@p_UrunID", urunId);
                    using (MySqlDataReader readerMalzemeler = cmdMalzemeler.ExecuteReader())
                    {
                        while (readerMalzemeler.Read())
                        {
                            urunMalzemeleri.Add(new UrunMalzemeViewModel
                            {
                                ID = readerMalzemeler.GetInt32("ID"),
                                UrunID = readerMalzemeler.GetInt32("UrunID"),
                                MalzemeID = readerMalzemeler.GetInt32("MalzemeID"),
                                MalzemeAdi = readerMalzemeler.GetString("MalzemeAdi"),
                                Miktar = readerMalzemeler.GetInt32("Miktar")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("GetUrunMalzemeleri Hata: " + ex.Message);
            }
            finally
            {
                if (ownConnection && conn.State == ConnectionState.Open) conn.Close();
            }
            return urunMalzemeleri;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UrunDetayMalzemeViewModel viewModel)
        {
            // Only validate Urun's Adi and Fiyat as per original logic for Urun update
            if (string.IsNullOrWhiteSpace(viewModel.Urun.Adi)) ModelState.AddModelError("Urun.Adi", "Ürün adı gereklidir.");
            if (viewModel.Urun.Fiyat <= 0) ModelState.AddModelError("Urun.Fiyat", "Fiyat 0'dan büyük olmalıdır.");

            if (ModelState.IsValidField("Urun.Adi") && ModelState.IsValidField("Urun.Fiyat"))
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("sp_UpdateUrun", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@p_ID", viewModel.Urun.ID);
                            cmd.Parameters.AddWithValue("@p_Adi", viewModel.Urun.Adi);
                            cmd.Parameters.AddWithValue("@p_Fiyat", viewModel.Urun.Fiyat);
                            cmd.Parameters.AddWithValue("@p_Aciklama", (object)viewModel.Urun.Aciklama ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@p_KategoriID", (object)viewModel.Urun.KategoriID ?? DBNull.Value);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0) TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
                            else TempData["ErrorMessage"] = "Ürün güncellenemedi veya bulunamadı.";
                        }
                    }
                    catch (MySqlException ex) when (ex.Number == 1062)
                    {
                        TempData["ErrorMessage"] = "Ürün güncellenirken bir hata oluştu: Bu ürün adı zaten mevcut.";
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Ürün güncellenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            // Always redirect to Edit GET to refresh the page with latest data and messages
            return RedirectToAction("Edit", new { id = viewModel.Urun.ID });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMalzemeToUrun(UrunDetayMalzemeViewModel viewModel)
        {
            if (viewModel.YeniUrunMalzeme.MalzemeID > 0 && viewModel.YeniUrunMalzeme.Miktar > 0)
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    try
                    {
                        conn.Open();
                        bool exists = false;
                        using (MySqlCommand checkCmd = new MySqlCommand("sp_CheckUrunMalzemeExists", conn))
                        {
                            checkCmd.CommandType = CommandType.StoredProcedure;
                            checkCmd.Parameters.AddWithValue("@p_UrunID", viewModel.YeniUrunMalzeme.UrunID);
                            checkCmd.Parameters.AddWithValue("@p_MalzemeID", viewModel.YeniUrunMalzeme.MalzemeID);
                            exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                        }

                        if (exists)
                        {
                            TempData["ErrorMessage"] = "Bu malzeme zaten ürüne eklenmiş. Miktarı güncellemek için mevcut kaydı düzenleyin.";
                        }
                        else
                        {
                            using (MySqlCommand cmd = new MySqlCommand("sp_AddUrunMalzeme", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@p_UrunID", viewModel.YeniUrunMalzeme.UrunID);
                                cmd.Parameters.AddWithValue("@p_MalzemeID", viewModel.YeniUrunMalzeme.MalzemeID);
                                cmd.Parameters.AddWithValue("@p_Miktar", viewModel.YeniUrunMalzeme.Miktar);
                                cmd.ExecuteNonQuery();
                                TempData["SuccessMessage"] = "Malzeme ürüne başarıyla eklendi.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Malzeme eklenirken bir hata oluştu: " + ex.Message;
                    }
                }
            }
            else
            {
                if (viewModel.YeniUrunMalzeme.MalzemeID <= 0) TempData["ErrorMessage"] = "Lütfen bir malzeme seçiniz.";
                if (viewModel.YeniUrunMalzeme.Miktar <= 0) TempData["ErrorMessage"] = (TempData["ErrorMessage"]?.ToString() ?? "") + " Miktar en az 1 olmalıdır.";
            }
            return RedirectToAction("Edit", new { id = viewModel.YeniUrunMalzeme.UrunID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveMalzemeFromUrun(int urunMalzemeId, int urunId) // urunMalzemeId is UrunMalzeme.ID
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_RemoveUrunMalzemeById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_UrunMalzemeID", urunMalzemeId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0) TempData["SuccessMessage"] = "Malzeme üründen başarıyla kaldırıldı.";
                        else TempData["ErrorMessage"] = "Malzeme kaldırılamadı veya bulunamadı.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Malzeme kaldırılırken bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Edit", new { id = urunId });
        }

        public ActionResult Delete(int id)
        {
            UrunViewModel urunViewModel = null;
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand("sp_GetUrunByIdForDeleteView", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_UrunID", id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                urunViewModel = new UrunViewModel
                                {
                                    ID = reader.GetInt32("ID"),
                                    Adi = reader.IsDBNull(reader.GetOrdinal("Adı")) ? null : reader.GetString("Adı"),
                                    Fiyat = reader.IsDBNull(reader.GetOrdinal("Fiyat")) ? 0 : reader.GetDecimal("Fiyat"),
                                    Aciklama = reader.IsDBNull(reader.GetOrdinal("Açıklama")) ? null : reader.GetString("Açıklama"),
                                    KategoriAdi = reader.IsDBNull(reader.GetOrdinal("KategoriAdi")) ? "Belirtilmemiş" : reader.GetString("KategoriAdi")
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ürün silme için yüklenirken bir hata oluştu: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }

            if (urunViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek ürün bulunamadı.";
                return HttpNotFound();
            }
            return View(urunViewModel);
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
                    using (MySqlCommand cmd = new MySqlCommand("sp_DeleteUrunAndMalzemeleri", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@p_UrunID", id);

                        MySqlParameter pSuccess = new MySqlParameter("@p_Success", MySqlDbType.Byte) { Direction = ParameterDirection.Output };
                        MySqlParameter pErrorMessage = new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 255) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(pSuccess);
                        cmd.Parameters.Add(pErrorMessage);

                        cmd.ExecuteNonQuery();

                        bool success = Convert.ToBoolean(pSuccess.Value);
                        string spMessage = pErrorMessage.Value.ToString();

                        if (success) TempData["SuccessMessage"] = spMessage;
                        else TempData["ErrorMessage"] = spMessage;
                    }
                }
                catch (Exception ex) // Catch C# level exceptions if SP call itself fails
                {
                    TempData["ErrorMessage"] = "Ürün silinirken beklenmedik bir hata oluştu: " + ex.Message;
                }
            }
            return RedirectToAction("Index");
        }
    }
}