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
                    .OrderBy(m => m.author_name)
                    .ThenBy(m => m.committed_date)
                    .ToListAsync();
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

            var masterProjectModel = await _context_mp.MasterProjectModel.FirstOrDefaultAsync(data => data.Id == generateCommit.project_id);

            //TODO: Filter Date If Exist In DB
            var commitModelTanggalMulai = await _context.CommitModel
                .FirstOrDefaultAsync(data => 
                    (data.committed_date.Value.ToString().Contains(generateCommit.tanggal_mulai.ToString("yyy-MM-dd")))
                    && (data.MasterProjectModelId == masterProjectModel.Id)
                );
           
            if (commitModelTanggalMulai != null)
            {
                TempData["error_system"] = "Tanggal mulai yang dimasukkan telah tersedia di database";

                return RedirectToAction("");
            }

            //TODO: Dont Save Data If Exist In DB
            //Get data in Table CommitModel

            //Check data message dari API === message dari DB

            //IF Same Dont Save

            //If Not Same Just Save

            //TODO: Generate API data
            var gitlabData = _gitlabService.getList(masterProjectModel.host_url,
                                                    masterProjectModel.project_id,
                                                    masterProjectModel.accsess_token,
                                                    generateCommit.tanggal_mulai.ToString(),
                                                    generateCommit.tanggal_selesai.ToString(),
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
                cm.MasterProjectModelId = masterProjectModel.Id;

                _context.CommitModel.Add(cm);
                _context.SaveChanges();
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

        public async Task<IActionResult> download(GenerateCommitModel generateCommit)
        {
            DateTime tgl_mulai = generateCommit.tanggal_mulai;
            DateTime tgl_selesai = generateCommit.tanggal_selesai;

            tgl_mulai = new DateTime(tgl_mulai.Year, tgl_mulai.Month, tgl_mulai.Day, 0, 0, 0);
            tgl_selesai = new DateTime(tgl_selesai.Year, tgl_selesai.Month, tgl_selesai.Day, 23, 59, 59);

            IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data = 
                await _context.CommitModel
                .OrderBy(m => m.committed_date)
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
            if(data.Count() == 0)
            {
                TempData["error_system"] = "TIdak ada data yang akan dicetak";
                return RedirectToAction("");
            }
            var stream = new MemoryStream();
            using (var xlPackage = new ExcelPackage(stream))
            {
                Dictionary<String, CheckExportSheet> chk_export = new Dictionary<string, CheckExportSheet>();
                foreach (var item in data) {
                    if (chk_export.ContainsKey(item.author_email) == false)
                    {
                        //Todo : create new sheet
                        var worksheet = xlPackage.Workbook.Worksheets.Add(item.author_email);
                        worksheet.Cells["A1"].RichText.Add("Nama").Bold = true;
                        worksheet.Cells["B1"].RichText.Add(item.author_name +" - "+item.author_email).Bold = true;
                        this.create_template_table(worksheet);
                        

                        CheckExportSheet checkExportSheet = new CheckExportSheet();
                        String d_string = ((DateTime)item.committed_date).ToString("dd-MMM-yyyy");
                        checkExportSheet.row_date =  this.init_row_date(worksheet, tgl_mulai, tgl_selesai);
                        checkExportSheet.sheet = worksheet;
                        checkExportSheet.ck_date = d_string;
                        checkExportSheet.currentRow = checkExportSheet.row_date[d_string];
                        checkExportSheet.lastRow = checkExportSheet.row_date[tgl_selesai.ToString("dd-MMM-yyyy")];
                        chk_export[item.author_name] = checkExportSheet;

                        this.add_row_table(worksheet, checkExportSheet.currentRow, item, true);
                    }
                    else
                    {
                        //Continue Avaible Sheet
                        CheckExportSheet checkExportSheet = chk_export[item.author_name];
                        var worksheet = checkExportSheet.sheet;
                        

                        bool add_row = true;
                        string commit_date = ((DateTime)item.committed_date).ToString("dd-MMM-yyyy");
                        var current = checkExportSheet.row_date[commit_date];
                        //if (checkExportSheet.ck_date == null) {
                        //    lastRow += 1;
                        //}
                        //else
                        //{
                        if (checkExportSheet.ck_date.Equals(commit_date))
                            add_row = false;
                        //    else
                        //        lastRow += 1;
                        //}


                        this.add_row_table(worksheet, current, item, add_row);
                        checkExportSheet.currentRow = current;
                        checkExportSheet.ck_date = commit_date;
                        chk_export[item.author_name] = checkExportSheet;

                    }
                }

                //Todo : menerapkan border tabel
                foreach (var item in chk_export.Values)
                {
                    ExcelWorksheet sheet = item.sheet;
                    sheet.Cells["A4:E" + item.lastRow].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    sheet.Cells["A4:E" + item.lastRow].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    sheet.Cells["A4:E" + item.lastRow].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    sheet.Cells["A4:E" + item.lastRow].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    sheet.Cells["C4:E" + item.lastRow].Style
                        .HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells["A4:A" + item.lastRow].Style
                        .HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells["A4:E" + item.lastRow].Style.WrapText = true;

                    sheet.Cells["C4:E" + item.lastRow].Style
                        .VerticalAlignment = ExcelVerticalAlignment.Top;
                    sheet.Cells["A4:A" + item.lastRow].Style
                        .VerticalAlignment = ExcelVerticalAlignment.Top;
                    sheet.Cells["A4:E" + item.lastRow].Style.WrapText = true;
                }

                xlPackage.Save();
                // Response.Clear();
            }
            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Timesheet "+ master_project.name+ ".xlsx");
        }

        //Todo : Membuat template tabel export excel
        public void create_template_table(ExcelWorksheet sheet)
        {
            sheet.Cells["A" + 4].RichText.Add("Tanggal").Bold = true;
            sheet.Cells["B" + 4].RichText.Add("Kegitan").Bold = true;
            sheet.Cells["C" + 4].RichText.Add("Jam mulai").Bold = true;
            sheet.Cells["D" + 4].RichText.Add("Jam Akhir").Bold = true;
            sheet.Cells["E" + 4].RichText.Add("Jumlah Jam").Bold = true;

            sheet.Cells["A4:E4"].Style
                .HorizontalAlignment = ExcelHorizontalAlignment.Center;

            sheet.Column(1).Width = 14;
            sheet.Column(2).Width = 100;
            sheet.Column(3).Width = 14;
            sheet.Column(4).Width = 13;
            sheet.Column(5).Width = 13;

        }
        //TODO : membuat inisialisasi row tanggal pada excel kemudian mengembalikan index row pada masing-masing tanggal
        Dictionary<string, int> init_row_date(ExcelWorksheet sheet, DateTime d_start, DateTime d_end) {
            Dictionary<string, int> row_date = new Dictionary<string, int>();
            int row_start = 5;
            for (var day = d_start; day.Date <= d_end.Date; day = day.AddDays(1)) {
                sheet.Cells["A" + row_start].Value = day.ToString("dd-MMM-yyyy");
                row_date.Add(day.ToString("dd-MMM-yyyy"), row_start);
                row_start += 1;
            }
            return row_date;
        }
        //Todo : Menambah row excel
        public void add_row_table(ExcelWorksheet sheet, int row, CommitModel model, bool add_row)
        {
            Regex pattern = new Regex("[\n]{2}");
            Regex pattern_last_newline = new Regex("[.+\n]$");

            string message = pattern.Replace(model.message, "\n");
            message = pattern_last_newline.Replace(message, "");
            if (add_row == true)
            {
                
                string d = ((DateTime)model.committed_date).ToString("dd-MMM-yyyy");
                sheet.Cells["A" + row].Value = d;
                sheet.Cells["B" + row].Value = message;
                sheet.Cells["C" + row].Value = model.jam_mulai;
                sheet.Cells["D" + row].Value = model.jam_akhir;
                sheet.Cells["E" + row].Value = model.jumlah_jam;
            }
            else
            {
                sheet.Cells["B" + row].RichText.Add("\n" + message);
            }

        }

        private bool CommitModelExists(int id)
        {
          return _context.CommitModel.Any(e => e.Id == id);
        }
    }

    class CheckExportSheet
    {
        public int currentRow { set; get; }
        public int lastRow { set; get; }
        public ExcelWorksheet sheet { set; get; }
        public string ck_date { set; get; }
        public Dictionary<string, int> row_date;
    }

}
