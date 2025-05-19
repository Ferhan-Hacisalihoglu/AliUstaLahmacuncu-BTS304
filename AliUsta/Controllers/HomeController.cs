using AliUsta.Models;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Web.Mvc;

namespace AliUsta.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;

        public ActionResult Index()
        {
            var viewModel = new AnaSayfaIstatistikViewModel();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    using (MySqlCommand cmd = new MySqlCommand("GetHomePageDashboardStats", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        DateTime oneYearAgo = DateTime.Now.AddYears(-1);
                        cmd.Parameters.AddWithValue("@p_OneYearAgo", oneYearAgo); 

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) 
                            {
                                viewModel.OrtalamaPuan = reader["AverageRating"] != DBNull.Value ? Convert.ToDouble(reader["AverageRating"]) : 0.0;
                                viewModel.ToplamUrunSayisi = Convert.ToInt32(reader["TotalProducts"]);
                                viewModel.ToplamPersonelSayisi = Convert.ToInt32(reader["TotalStaff"]);
                                viewModel.AktifMusteriSayisiSonYil = Convert.ToInt32(reader["ActiveCustomersLastYear"]);
                            }
                            else
                            {
                                SetDefaultViewModelValues(viewModel, "Saklı yordamdan veri okunamadı.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SetDefaultViewModelValues(viewModel, "İstatistikler yüklenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(viewModel);
        }

        private void SetDefaultViewModelValues(AnaSayfaIstatistikViewModel viewModel, string errorMessage = null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
            }
            viewModel.OrtalamaPuan = 0.0;
            viewModel.ToplamUrunSayisi = 0;
            viewModel.ToplamPersonelSayisi = 0;
            viewModel.AktifMusteriSayisiSonYil = 0;
        }

        [Authorize]
        public ActionResult AdminPanel()
        {
            return View();
        }
    }
}