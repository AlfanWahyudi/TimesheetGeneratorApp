using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using TimesheetGeneratorApp.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace TimesheetGeneratorApp.Helper
{
    public class ExportWordDataCommit
    {
        IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data;
        MasterProjectModel mp;
        DateTime tgl_mulai;
        DateTime tgl_selesai;
        String key_date_format = "yyyyMMdd";
        public ExportWordDataCommit(IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data,
            MasterProjectModel mp, DateTime tgl_mulai,
            DateTime tgl_selesai){

            this.data = data;
            this.mp = mp;
            this.tgl_mulai = tgl_mulai;
            this.tgl_selesai = tgl_selesai;
        }

        public void run()
        {
            using (var doc = WordprocessingDocument.Create("wwwroot/export/word/Export_word.docx", WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainDocumentPart = doc.AddMainDocumentPart();
                mainDocumentPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();

                Body body = mainDocumentPart.Document.AppendChild(new Body());

                Dictionary<string, TabelCommit> dict_table_commit = new Dictionary<string, TabelCommit>();
                int row_table = 1;
                //Todo : Repeat data
                foreach (CommitModel commit in this.data) {
                    
                    //TODO : create new table
                    if(dict_table_commit.ContainsKey(commit.author_email) == false)
                    {
                        TabelCommit tabelCommit = new TabelCommit();

                        Table tbl = new Table();
                        // Set the style and width for the table.
                        TableProperties tableProp = new TableProperties();
                        TableStyle tableStyle = new TableStyle() { Val = "TableGrid" };

                        // Make the table width 100% of the page width.
                        TableWidth tableWidth = new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct };
                        // Apply
                        tableProp.Append(tableStyle, tableWidth);
                        tbl.AppendChild(tableProp);

                        // Add 3 columns to the table.
                        TableGrid tg = new TableGrid(new GridColumn(), new GridColumn(), new GridColumn());
                        tbl.AppendChild(tg);

                        //create header
                        create_header_table(row_table, body, tbl, commit.author_name, "");
                        //init row date
                        create_row_date(tbl, tabelCommit);
                        // Add the table to the document
                        body.AppendChild(tbl);

                        tabelCommit.tbl = tbl;
                        dict_table_commit.Add(commit.author_email, tabelCommit);
                        row_table += 1;
                    }
                    else //TODO : Modify Table
                    {

                    }
                    
                }
            }
        }
        //TODO : Membuat Header Tabel
        void create_header_table(int row_table ,Body body,Table tbl, String nama, String jabatan)
        {
            Paragraph pr_name = new Paragraph(
                    new Run(
                        new Text(row_table.ToString()+". "+nama)
                        ));
            body.AppendChild(pr_name);

            Paragraph pr_jabatan = new Paragraph(
                new Run(
                    new Text("Jabatan : ")
                    ));
            body.AppendChild(pr_jabatan);

            // Create 1 row to the table.
            TableRow tr_head = new TableRow();
            // Add a cell to each column in the row.
            TableCell tc1 = new TableCell(new Paragraph(new Run(new Text("No"))));
            TableCell tc2 = new TableCell(new Paragraph(new Run(new Text("Tanggal"))));
            TableCell tc3 = new TableCell(new Paragraph(new Run(new Text("Kegiatan"))));
            tr_head.Append(tc1, tc2, tc3);
            tbl.Append(tr_head);
        }

        //TODO : create row date
        void create_row_date(Table tbl, TabelCommit tabelCommit)
        {
            var day = this.tgl_mulai.Date;
            int i = 1;
            tabelCommit.tc_1 = new Dictionary<string, TableCell>();
            tabelCommit.tc_2 = new Dictionary<string, TableCell>();
            tabelCommit.tc_3 = new Dictionary<string, TableCell>();

            while (day <= this.tgl_selesai.Date)
            {
                TableRow tr = new TableRow();
                TableCell tc_1 = new TableCell(
                    new Paragraph(
                        new Run(
                            new Text(i.ToString())
                            )));
                TableCell tc_2 = new TableCell(
                    new Paragraph(
                        new Run(
                            new Text(day.ToString("dddd, dd MMMM yyyy"))
                            )));
                TableCell tc_3 = new TableCell(
                    new Paragraph(
                        new Run(
                            new Text("")
                            )));

                tr.Append(tc_1, tc_2, tc_3);
                tbl.Append(tr);
                tabelCommit.tc_1.Add(day.ToString(key_date_format), tc_1);
                tabelCommit.tc_2.Add(day.ToString(key_date_format), tc_2);
                tabelCommit.tc_3.Add(day.ToString(key_date_format), tc_3);

                day = day.AddDays(1);
                i += 1;
            }
        }

        //TODO : Menambahkan row Tabel
        void modify_row_table()
        {

        }
    }

    class TabelCommit
    {
        public Table tbl { set; get; }
        public Dictionary<string, TableCell> tc_1 { set; get; } //merujuk pada row dari cell 1
        public Dictionary<string, TableCell> tc_2 { set; get; }//merujuk pada row dari cell 2
        public Dictionary<string, TableCell> tc_3 { set; get; }//merujuk pada row dari cell 3
        public Dictionary<string, bool> update { set; get; }

    }
}
