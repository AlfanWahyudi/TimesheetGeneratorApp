using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimesheetGeneratorApp.Data;
using TimesheetGeneratorApp.Models;
using TimesheetGeneratorApp.Services;

namespace TimesheetGeneratorApp.Controllers
{
    public class HariLiburController : Controller
    {
        private readonly HariLiburContext _context;

        private HttpClient _httpClient;
        private HariLiburService _hariLiburService; 

        public HariLiburController(HariLiburContext context)
        {
            _context = context;

            _httpClient = new HttpClient();
            _hariLiburService = new HariLiburService(_httpClient);
        }

        // GET: HariLibur
        public async Task<IActionResult> Index()
        {
            return View(await _context.HariLiburModel.ToListAsync());
        }

        public async Task<IActionResult> Generate(string year, string btn_generate)
        {
            TempData["generate"] = true;
            TempData["year"] = year;
            int int_year;
            bool cek_year = int.TryParse(year, out int_year);

            if (cek_year == false)
            {
                TempData["error_system"] = "Perhatikan inputan anda. Tipe data " +
                    "yang diinputkan mesti number atau angka";
                return RedirectToAction("Index");
            }
            if (btn_generate.Equals("Tampilkan"))
            {
                return RedirectToAction("Index");
            }

            var hariLiburModels = await _context.HariLiburModel
                .Where(m => m.holiday_date.Value.Year == int_year).
                ToListAsync();

            if (hariLiburModels != null)
            {
                foreach (var hariLiburModel in hariLiburModels)
                {
                    _context.HariLiburModel.Remove(hariLiburModel);
                    await _context.SaveChangesAsync();
                }
            }

            var hariLiburService = _hariLiburService.getListByYear(year);

            if (hariLiburService == null)
            {
                return NotFound();
            }

            foreach (var item in hariLiburService)
            {
                HariLiburModel hariLiburModel = new HariLiburModel();

                hariLiburModel.holiday_date = item.holiday_date;
                hariLiburModel.holiday_name = item.holiday_name;
                hariLiburModel.is_national_holiday = item.is_national_holiday;

                _context.Add(hariLiburModel);
                await _context.SaveChangesAsync();
            }

            TempData["message_success"] = "Berhasil melakukan generate data";
            return RedirectToAction("index");
        }
    }
}
