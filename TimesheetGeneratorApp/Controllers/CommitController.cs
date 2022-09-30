using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TimesheetGeneratorApp.Data;
using TimesheetGeneratorApp.Models;
using TimesheetGeneratorApp.Services;
using TimesheetGeneratorApp.Helper;

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
            if (TempData["generate"] == null)
            {
                return View(await _context.CommitModel.ToListAsync());
            }
            else
            {
                DateTime tgl_mulai = ((DateTime)TempData["generate_tanggal_mulai"]);
                DateTime tgl_selesai = ((DateTime)TempData["generate_tanggal_selesai"]);

                TempData["generate_tanggal_mulai"] = tgl_mulai.ToString("yyyy-MM-dd");
                TempData["generate_tanggal_selesai"] = tgl_selesai.ToString("yyyy-MM-dd");
                ;
                IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data =
                    await _context.CommitModel
                    .Where(m => m.MasterProjectModelId == (int)TempData["generate_project_id"])
                    .Where(m => m.committed_date >= new DateTime(tgl_mulai.Year, tgl_mulai.Month, tgl_mulai.Day, 0, 0, 0))
                    .Where(m => m.committed_date <= new DateTime(tgl_selesai.Year, tgl_selesai.Month, tgl_selesai.Day, 23, 59, 59))
                    .OrderBy(m => m.committed_date)
                    .ThenBy(m => m.author_name)
                    .ToListAsync();
                return View(data);
            }
        }
        public async Task<IActionResult> generate(GenerateCommitModel generateCommit)
        {
            TempData["generate"] = true;
            TempData["generate_tanggal_mulai"] = generateCommit.tanggal_mulai;
            TempData["generate_tanggal_selesai"] = generateCommit.tanggal_selesai;
            TempData["generate_project_id"] = generateCommit.project_id;

            if (generateCommit.btn_generate.Equals("Tampilkan"))
            {
                return RedirectToAction("Index");
            }

            var masterProjectModel = await _context_mp.MasterProjectModel.FirstOrDefaultAsync(data => data.Id == generateCommit.project_id);

            // Remove dates in table CommitModel by range end date
            DateTime endDate = generateCommit.tanggal_selesai.AddDays(1);
            var commitModels = await _context.CommitModel
                .Where(data => data.committed_date >= generateCommit.tanggal_mulai)
                .Where(data => data.committed_date <= endDate)
                .Where(data => data.MasterProjectModelId == masterProjectModel.Id)
                .ToListAsync();

            if (commitModels != null)
            {
                foreach(var commitModel in commitModels)
                {
                    _context.CommitModel.Remove(commitModel);
                }
                await _context.SaveChangesAsync();
            }

            //TODO: Generate API data
            var gitlabData = _gitlabService.getList(masterProjectModel.host_url,
                                                    masterProjectModel.project_id,
                                                    masterProjectModel.accsess_token,
                                                    generateCommit.tanggal_mulai.ToString("yyyy/MM/dd") + "T00:00:00",
                                                    generateCommit.tanggal_selesai.ToString("yyyy/MM/dd")+"T23:59:59",
                                                    "true", "true", "100");

            //Todo : check error system
            if (gitlabData == null)
            {
                return RedirectToAction("");
            }

            //TODO: Save Data Gitlab API to DB
            foreach (GitlabCommitModel item in gitlabData)
            {
                CommitModel cm = new CommitModel();
                cm.message = item.message.Trim();
                cm.committed_date = item.committed_date;
                cm.jumlah_jam = 9;
                cm.jam_mulai = "08:00";
                cm.jam_akhir = "17:00";
                cm.author_name = item.author_name;
                cm.author_email = item.author_email;
                cm.MasterProjectModelId = masterProjectModel.Id;

                _context.CommitModel.Add(cm);
                _context.SaveChanges();
            }

            TempData["message_success"] = "Berhasil melakukan generate data";
            return RedirectToAction("");
        }

        public async Task<IActionResult> download(GenerateCommitModel generateCommit)
        {
            
            DateTime tgl_mulai = generateCommit.tanggal_mulai;
            DateTime tgl_selesai = generateCommit.tanggal_selesai;

            tgl_mulai = new DateTime(tgl_mulai.Year, tgl_mulai.Month, tgl_mulai.Day, 0, 0, 0);
            tgl_selesai = new DateTime(tgl_selesai.Year, tgl_selesai.Month, tgl_selesai.Day, 23, 59, 59);

            IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data =
                await _context.CommitModel
                .OrderBy(m => m.author_email)
                .ThenBy(m => m.committed_date)
                .Where(m => m.MasterProjectModelId == generateCommit.project_id)
                .Where(m => m.committed_date >= tgl_mulai)
                .Where(m => m.committed_date <= tgl_selesai)
                .ToListAsync();

            var master_project = await _context_mp.MasterProjectModel.FirstOrDefaultAsync(m => m.Id == generateCommit.project_id);
            if (master_project == null)
            {
                return RedirectToAction();
                TempData["error_system"] = "TIdak ada data yang akan dicetak";
            }
            if (data.Count() == 0)
            {
                TempData["error_system"] = "TIdak ada data yang akan dicetak";
                return RedirectToAction("");
            }

            if (generateCommit.export_type.Equals("word"))
            {
                ExportWordDataCommit exportWordDataCommit = new ExportWordDataCommit(data,
                    master_project, tgl_mulai, tgl_selesai);

                string pth_fname = exportWordDataCommit.run();
                return LocalRedirect("~/"+pth_fname);
            }
            else
            {
                ExportExcelDataCommit exportDataCommit = new ExportExcelDataCommit();
                return File(exportDataCommit.export_to_excel(master_project,
                    data, tgl_mulai, tgl_selesai
                    ), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Timesheet " + master_project.name + ".xlsx");
            }
        }
        private bool CommitModelExists(int id)
        {
            return _context.CommitModel.Any(e => e.Id == id);
        }
    }

    

}
