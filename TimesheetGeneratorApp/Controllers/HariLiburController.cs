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

        public async Task<IActionResult> Generate(string year)
        {
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

        // GET: HariLibur/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HariLibur/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string year)
        {
            await this.Generate(year);

            return RedirectToAction("Index");
        }

        // GET: HariLibur/Edit/5
        public IActionResult Edit()
        {
            return View();
        }

        // POST: HariLibur/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string year)
        {
            var hariLiburModel = await _context.HariLiburModel.ToListAsync();

            foreach (var item in hariLiburModel)
            {
                _context.HariLiburModel.Remove(item);
                await _context.SaveChangesAsync();
            }

            await this.Generate(year);

            return RedirectToAction("Index");
        }

        private bool HariLiburModelExists(int id)
        {
          return _context.HariLiburModel.Any(e => e.Id == id);
        }
    }
}
