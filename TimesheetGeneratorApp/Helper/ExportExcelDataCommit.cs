using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using TimesheetGeneratorApp.Controllers;
using TimesheetGeneratorApp.Data;
using TimesheetGeneratorApp.Models;
using System.Drawing;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Color = System.Drawing.Color;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TimesheetGeneratorApp.Helper
{
    public class ExportExcelDataCommit
    {
        private readonly CommitContext _context;
        private readonly MasterProjectContext _context_mp;
        Controller ctr;

        public  ExportExcelDataCommit()
        {
        
        }

        public MemoryStream export_to_excel(MasterProjectModel master_project,
            IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data,
            DateTime tgl_mulai, DateTime tgl_selesai)
        {
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
                        create_template_table(worksheet);


                        CheckExportSheet checkExportSheet = new CheckExportSheet();
                        String d_string = ((DateTime)item.committed_date).ToString("dd-MMM-yyyy");
                        checkExportSheet.row_date = init_row_date(worksheet, tgl_mulai, tgl_selesai);
                        checkExportSheet.sheet = worksheet;
                        checkExportSheet.ck_date = d_string;
                        checkExportSheet.currentRow = checkExportSheet.row_date[d_string];
                        checkExportSheet.lastRow = checkExportSheet.row_date[tgl_selesai.ToString("dd-MMM-yyyy")];
                        chk_export[ws_name] = checkExportSheet;

                        add_row_table(worksheet, checkExportSheet.currentRow, item, true);
                    }
                    else
                    {
                        //Continue Avaible Sheet
                        CheckExportSheet checkExportSheet = chk_export[ws_name];
                        var worksheet = checkExportSheet.sheet;


                        bool add_row = true;
                        string commit_date = ((DateTime)item.committed_date).ToString("dd-MMM-yyyy");
                        var current = checkExportSheet.row_date[commit_date];

                        if (checkExportSheet.ck_date.Equals(commit_date))
                            add_row = false;

                        add_row_table(worksheet, current, item, add_row);
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

            return stream;
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
        public Dictionary<string, int> init_row_date(ExcelWorksheet sheet, DateTime d_start, DateTime d_end)
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
    }

    public class CheckExportSheet
    {
        public int currentRow { set; get; }
        public int lastRow { set; get; }
        public ExcelWorksheet sheet { set; get; }
        public string ck_date { set; get; }
        public Dictionary<string, int> row_date;
    }
}
