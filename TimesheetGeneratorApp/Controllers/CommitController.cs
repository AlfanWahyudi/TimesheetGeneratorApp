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
            if (data.Count() == 0)
            {
                TempData["error_system"] = "TIdak ada data yang akan dicetak";
                return RedirectToAction("");
            }
            var stream = new MemoryStream();
            using (var xlPackage = new ExcelPackage(stream))
            {
                Dictionary<String, CheckExportSheet> chk_export = new Dictionary<string, CheckExportSheet>();
                foreach (var item in data)
                {
                    string ws_name = item.author_email.Replace("@", "_");
                    ws_name = ws_name.Replace(".", "_");
                    if (chk_export.ContainsKey(ws_name) == false)
                    {
                        //Todo : create new sheet
                        var worksheet = xlPackage.Workbook.Worksheets.Add(item.author_name);
                        worksheet.Cells["A1"].RichText.Add("Nama").Bold = true;
                        worksheet.Cells["B1"].RichText.Add(item.author_name).Bold = true;
                        this.create_template_table(worksheet);


                        CheckExportSheet checkExportSheet = new CheckExportSheet();
                        String d_string = ((DateTime)item.committed_date).ToString("dd-MMM-yyyy");
                        checkExportSheet.row_date = this.init_row_date(worksheet, tgl_mulai, tgl_selesai);
                        checkExportSheet.sheet = worksheet;
                        checkExportSheet.ck_date = d_string;
                        checkExportSheet.currentRow = checkExportSheet.row_date[d_string];
                        checkExportSheet.lastRow = checkExportSheet.row_date[tgl_selesai.ToString("dd-MMM-yyyy")];
                        chk_export[ws_name] = checkExportSheet;

                        this.add_row_table(worksheet, checkExportSheet.currentRow, item, true);
                    }
                    else
                    {
                        //Continue Avaible Sheet
                        CheckExportSheet checkExportSheet = chk_export[ws_name];
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
                        chk_export[ws_name] = checkExportSheet;

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
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Timesheet " + master_project.name + ".xlsx");
        }

        //Todo : Membuat template tabel export excel
        public void create_template_table(ExcelWorksheet sheet)
        {
            sheet.Cells["A" + 4].RichText.Add("Tanggal").Bold = true;
            sheet.Cells["B" + 4].RichText.Add("Kegiatan").Bold = true;
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
        Dictionary<string, int> init_row_date(ExcelWorksheet sheet, DateTime d_start, DateTime d_end)
        {
            Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#ffc8dd");
            Dictionary<string, int> row_date = new Dictionary<string, int>();

            int row_start = 5;
            var day = d_start;
            while (day.Date <= d_end.Date)
            {
                string s_day = day.ToString("dddd");
                sheet.Cells["A" + row_start].Value = day.ToString("dd-MMM-yyyy");
                row_date.Add(day.ToString("dd-MMM-yyyy"), row_start);

                if (s_day.ToLower().Equals("saturday") || s_day.ToLower().Equals("sunday"))
                {
                    sheet.Cells["A" + row_start + ":E" + row_start].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells["A" + row_start + ":E" + row_start].
                        Style.Fill.BackgroundColor.SetColor(colFromHex);
                }
                row_start += 1;
                day = day.AddDays(1);
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
