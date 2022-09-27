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
        private readonly MasterProjectContext _context_mp;

        private HttpClient _httpClient;
        private GitlabService _gitlabService;
        private ParameterModel parameterModel;

        public CommitController(CommitContext context, MasterProjectContext context_mp)
        {
            _context = context;

            _context_mp = context_mp;
            _httpClient = new HttpClient();
            _gitlabService = new GitlabService(_httpClient, this);
            parameterModel = new ParameterModel();
        }

        // GET: Commit
        public async Task<IActionResult> Index()
        {
            ViewBag.mp = await _context_mp.MasterProjectModel.ToListAsync();
            if(TempData["generate"] == null)
            {
                return View(await _context.CommitModel.ToListAsync());
            }else{
                IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data = await _context.CommitModel.ToListAsync();
                return View(data);
            }
        }
        public async Task<IActionResult> generate(GenerateCommitModel generateCommit)
        {
            if (generateCommit.btn_generate.Equals("Tampilkan"))
            {
                TempData["generate"] = true;
                TempData["generate_tanggal_mulai"] = generateCommit.tanggal_mulai;
                TempData["generate_tanggal_selesai"] = generateCommit.tanggal_selesai;
                TempData["generate_project_id"] = generateCommit.project_id;
                return RedirectToAction("Index");
            }


            //TODO: Generate API data
            var masterProjectModel = await _context_mp.MasterProjectModel.FirstOrDefaultAsync(data => data.Id == generateCommit.project_id);

            var gitlabData = _gitlabService.getList(masterProjectModel.host_url,
                                                    masterProjectModel.project_id,
                                                    masterProjectModel.accsess_token, 
                                                    generateCommit.tanggal_mulai.ToString(),
                                                    generateCommit.tanggal_selesai.ToString(),
                                                    "true", "true", "100");
            //Todo : check error system
            if(gitlabData == null)
            {
                return RedirectToAction("");
            }

            //TODO: Save Data Gitlab API to DB
            foreach (GitlabCommitModel item in gitlabData)
            {
                CommitModel cm = new CommitModel();
                cm.message = item.message;
                cm.committed_date = null;
                cm.jumlah_jam = 8;
                cm.jam_mulai = null;
                cm.jam_akhir = null;
                cm.author_name = item.author_name;
                cm.MasterProjectModelId = generateCommit.project_id;

                _context.CommitModel.Add(cm);
                _context.SaveChanges();

                //return Ok(
                //    new { Results = gitlabData }
                //);


                //foreach (var item in obj)
                //{
                //    CommitModel cm = new CommitModel();
                //    cm.message = item.message;
                //    cm.committed_date = item.committed_date;
                //    cm.jumlah_jam = 8;
                //    cm.jam_mulai = item.committed_date;
                //    cm.jam_akhir = item.committed_date;
                //    cm.author_name = item.author_name;

                //    _context.CommitModel.Add(cm);
                //}

            }

            TempData["message_success"] = "Berhasil melakukan generate data";
            return RedirectToAction("");

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
