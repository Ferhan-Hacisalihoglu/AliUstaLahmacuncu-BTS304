using AliUsta.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.Web.Mvc;

namespace AliUsta.Controllers
{
    [Authorize]
    public class AlimController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public ActionResult Index()
        {
            List<AlimViewModel> alimlar = new List<AlimViewModel>();
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                // Use stored procedure sp_GetAllAlimlarWithDetails
                using (MySqlCommand cmd = new MySqlCommand("sp_GetAllAlimlarWithDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            alimlar.Add(new AlimViewModel
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                TedarikciAdi = reader["TedarikciAdi"].ToString(),
                                MalzemeAdi = reader["MalzemeAdi"].ToString(),
                                Miktar = Convert.ToInt32(reader["Miktar"]),
                                Tarih = Convert.ToDateTime(reader["Tarih"]),
                                TedarikciID = Convert.ToInt32(reader["TedarikciID"]),
                                MalzemeID = Convert.ToInt32(reader["MalzemeID"])
                            });
                        }
                    }
                }
            }
            return View(alimlar);
        }

        public ActionResult Create(int? tedarikciId)
        {
            var model = new AlimViewModel
            {
                Tedarikciler = GetTedarikcilerList(), // This now uses an SP
                Tarih = DateTime.Now
            };

            if (tedarikciId.HasValue && tedarikciId.Value > 0)
            {
                model.SelectedTedarikciID = tedarikciId.Value;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AlimViewModel model)
        {
            if (model.SelectedTedarikciID == 0)
            {
                ModelState.AddModelError("SelectedTedarikciID", "Lütfen bir tedarikçi seçiniz.");
            }

            if (!ModelState.IsValid)
            {
                model.Tedarikciler = GetTedarikcilerList();
                return View(model);
            }

            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                // Use stored procedure sp_ProcessAlimCreation
                using (MySqlCommand cmd = new MySqlCommand("sp_ProcessAlimCreation", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_TedarikciID", model.SelectedTedarikciID);
                    cmd.Parameters.AddWithValue("@p_Miktar", model.Miktar);
                    cmd.Parameters.AddWithValue("@p_Tarih", model.Tarih);

                    MySqlParameter pSuccess = new MySqlParameter("@p_Success", MySqlDbType.Byte)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pSuccess);

                    MySqlParameter pErrorMessage = new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 255)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pErrorMessage);

                    try
                    {
                        cmd.ExecuteNonQuery(); // Transaction is handled within the SP

                        bool success = Convert.ToBoolean(pSuccess.Value); // MySqlDbType.Byte maps to bool or byte
                        string spMessage = pErrorMessage.Value.ToString();

                        if (success)
                        {
                            TempData["SuccessMessage"] = spMessage;
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ModelState.AddModelError("", spMessage);
                        }
                    }
                    catch (MySqlException ex) // Catch DB specific exceptions
                    {
                        ModelState.AddModelError("", "Veritabanı işlemi sırasında bir hata oluştu: " + ex.Message);
                    }
                    catch (Exception ex) // Catch any other general exceptions
                    {
                        ModelState.AddModelError("", "İşlem sırasında genel bir hata oluştu: " + ex.Message);
                    }
                }
            }

            model.Tedarikciler = GetTedarikcilerList(); // Repopulate if error
            return View(model);
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AlimViewModel alimViewModel = null;
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                // Use stored procedure sp_GetAlimByIdWithDetails
                using (MySqlCommand cmd = new MySqlCommand("sp_GetAlimByIdWithDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_AlimID", id.Value);
                    con.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            alimViewModel = new AlimViewModel
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                TedarikciAdi = reader["TedarikciAdi"].ToString(),
                                MalzemeAdi = reader["MalzemeAdi"].ToString(),
                                Miktar = Convert.ToInt32(reader["Miktar"]),
                                Tarih = Convert.ToDateTime(reader["Tarih"]),
                                TedarikciID = Convert.ToInt32(reader["TedarikciID"]),
                                MalzemeID = Convert.ToInt32(reader["MalzemeID"])
                            };
                        }
                    }
                }
            }

            if (alimViewModel == null)
            {
                TempData["ErrorMessage"] = "Silinecek alım kaydı bulunamadı.";
                return HttpNotFound();
            }
            return View(alimViewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                con.Open();
                // Use stored procedure sp_ProcessAlimDeletion
                using (MySqlCommand cmd = new MySqlCommand("sp_ProcessAlimDeletion", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_AlimID", id);

                    MySqlParameter pSuccess = new MySqlParameter("@p_Success", MySqlDbType.Byte)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pSuccess);

                    MySqlParameter pErrorMessage = new MySqlParameter("@p_ErrorMessage", MySqlDbType.VarChar, 255)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pErrorMessage);

                    try
                    {
                        cmd.ExecuteNonQuery(); // Transaction is handled within the SP

                        bool success = Convert.ToBoolean(pSuccess.Value);
                        string spMessage = pErrorMessage.Value.ToString();

                        if (success)
                        {
                            TempData["SuccessMessage"] = spMessage;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = spMessage;
                        }
                    }
                    catch (MySqlException ex)
                    {
                        TempData["ErrorMessage"] = "Veritabanı silme işlemi sırasında bir hata oluştu: " + ex.Message;
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Silme işlemi sırasında genel bir hata oluştu: " + ex.Message;
                    }
                }
            }
            return RedirectToAction("Index");
        }

        private IEnumerable<SelectListItem> GetTedarikcilerList()
        {
            var tedarikciler = new List<SelectListItem>();
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                // Use stored procedure sp_GetTedarikcilerForSelectListWithMalzeme
                using (MySqlCommand cmd = new MySqlCommand("sp_GetTedarikcilerForSelectListWithMalzeme", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tedarikciler.Add(new SelectListItem
                            {
                                Value = reader["ID"].ToString(),
                                Text = $"{reader["TedarikciAdi"]} (Malzeme: {reader["MalzemeAdi"]})"
                            });
                        }
                    }
                }
            }
            return tedarikciler;
        }

        [HttpGet]
        public JsonResult GetMalzemeForTedarikci(int tedarikciId)
        {
            MalzemeViewModel malzeme = null;
            using (MySqlConnection con = new MySqlConnection(_connectionString))
            {
                // Use stored procedure sp_GetMalzemeDetailsByTedarikciId
                using (MySqlCommand cmd = new MySqlCommand("sp_GetMalzemeDetailsByTedarikciId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@p_TedarikciID", tedarikciId);
                    con.Open();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            malzeme = new MalzemeViewModel
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Adi = reader["Adi"].ToString() // SP returns 'Adi'
                            };
                        }
                    }
                }
            }
            if (malzeme != null)
            {
                return Json(new { success = true, malzemeId = malzeme.ID, malzemeAdi = malzeme.Adi }, JsonRequestBehavior.AllowGet);
            }
            // Provide a more specific message if no malzeme is found for the tedarikci
            return Json(new { success = false, message = "Bu tedarikçi için tanımlı bir malzeme bulunamadı." }, JsonRequestBehavior.AllowGet);
        }
    }
}