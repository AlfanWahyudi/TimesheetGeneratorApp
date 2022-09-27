﻿using System;
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
    public class CommitController : Controller
    {
        private readonly CommitContext _context;

        public CommitController(CommitContext context)
        {
            _context = context;
        }

        public IActionResult test_api()
        {
            return Ok(new { 
                result = "test berhasil"
            });
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