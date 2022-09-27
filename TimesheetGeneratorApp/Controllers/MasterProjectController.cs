using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimesheetGeneratorApp.Data;
using TimesheetGeneratorApp.Models;

namespace TimesheetGeneratorApp.Controllers
{
    public class MasterProjectController : Controller
    {
        private readonly MasterProjectContext _context;

        public MasterProjectController(MasterProjectContext context)
        {
            _context = context;
        }

        // GET: MasterProject
        public async Task<IActionResult> Index()
        {
              return View(await _context.MasterProjectModel.ToListAsync());
        }

        // GET: MasterProject/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MasterProjectModel == null)
            {
                return NotFound();
            }

            var masterProjectModel = await _context.MasterProjectModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (masterProjectModel == null)
            {
                return NotFound();
            }

            return View(masterProjectModel);
        }

        // GET: MasterProject/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MasterProject/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,name,project_id,version_control,host_url,accsess_token,username,password")] MasterProjectModel masterProjectModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(masterProjectModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
            TempData["error_system"] = errors;

            return View(masterProjectModel);
        }

        // GET: MasterProject/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MasterProjectModel == null)
            {
                return NotFound();
            }

            var masterProjectModel = await _context.MasterProjectModel.FindAsync(id);
            if (masterProjectModel == null)
            {
                return NotFound();
            }
            return View(masterProjectModel);
        }

        // POST: MasterProject/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,name,project_id,version_control,host_url,accsess_token,username,password")] MasterProjectModel masterProjectModel)
        {
            if (id != masterProjectModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(masterProjectModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MasterProjectModelExists(masterProjectModel.Id))
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
            return View(masterProjectModel);
        }

        // GET: MasterProject/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MasterProjectModel == null)
            {
                return NotFound();
            }

            var masterProjectModel = await _context.MasterProjectModel
                .FirstOrDefaultAsync(m => m.Id == id);
            if (masterProjectModel == null)
            {
                return NotFound();
            }

            return View(masterProjectModel);
        }

        // POST: MasterProject/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MasterProjectModel == null)
            {
                return Problem("Entity set 'MasterProjectContext.MasterProjectModel'  is null.");
            }
            var masterProjectModel = await _context.MasterProjectModel.FindAsync(id);
            if (masterProjectModel != null)
            {
                _context.MasterProjectModel.Remove(masterProjectModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MasterProjectModelExists(int id)
        {
          return _context.MasterProjectModel.Any(e => e.Id == id);
        }
    }
}
