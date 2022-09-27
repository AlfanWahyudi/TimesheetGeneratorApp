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
    public class CommitController : Controller
    {
        private readonly CommitContext _context;
        private HttpClient _httpClient;
        private GitlabService _gitlabService;
        private ParameterModel parameterModel;

        public CommitController(CommitContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
            _gitlabService = new GitlabService(_httpClient);
            parameterModel = new ParameterModel();
        }

        public string test_api()
        {
            var dataGitlab = _gitlabService.getList(121, "vkembo3-uC1e3y1NPpP3", "2022-09-01", "2022-09-30", "true", "true", "100");
            int count = 0;

            string dataArray = "";

            foreach (var item in dataGitlab)
            {
                count++;
                dataArray = item.ToString();
            }

            return "" + dataGitlab + dataArray + "        Jumlah Data: " + count;
        }

        // GET: Commit
        public async Task<IActionResult> Index()
        {
              return View(await _context.CommitModel.ToListAsync());
        }

        // GET: Commit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.CommitModel == null)
            {
                return NotFound();
            }

            var commitModel = await _context.CommitModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commitModel == null)
            {
                return NotFound();
            }

            return View(commitModel);
        }

        // GET: Commit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Commit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,author_name,message,committed_date,jam_mulai,jam_akhir,jumlah_jam")] CommitModel commitModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(commitModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(commitModel);
        }

        // GET: Commit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.CommitModel == null)
            {
                return NotFound();
            }

            var commitModel = await _context.CommitModel.FindAsync(id);
            if (commitModel == null)
            {
                return NotFound();
            }
            return View(commitModel);
        }

        // POST: Commit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,author_name,message,committed_date,jam_mulai,jam_akhir,jumlah_jam")] CommitModel commitModel)
        {
            if (id != commitModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(commitModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommitModelExists(commitModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(commitModel);
        }

        // GET: Commit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.CommitModel == null)
            {
                return NotFound();
            }

            var commitModel = await _context.CommitModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (commitModel == null)
            {
                return NotFound();
            }

            return View(commitModel);
        }

        // POST: Commit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.CommitModel == null)
            {
                return Problem("Entity set 'CommitContext.CommitModel'  is null.");
            }
            var commitModel = await _context.CommitModel.FindAsync(id);
            if (commitModel != null)
            {
                _context.CommitModel.Remove(commitModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommitModelExists(int id)
        {
          return _context.CommitModel.Any(e => e.Id == id);
        }
    }
}
