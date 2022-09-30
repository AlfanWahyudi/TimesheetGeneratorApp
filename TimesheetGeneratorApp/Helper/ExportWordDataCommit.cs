using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using TimesheetGeneratorApp.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using DocumentFormat.OpenXml.Bibliography;

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

        public string run()
        {
            string fname = mp.name+" "+DateTime.Now.ToString("yyyy-MM-dd");
            string pth = "export/word/"+fname+".docx";
            using (var doc = WordprocessingDocument.Create("wwwroot/"+pth, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainDocumentPart = doc.AddMainDocumentPart();
                mainDocumentPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();

                Body body = mainDocumentPart.Document.AppendChild(new Body());

                Dictionary<string, TabelCommit> dict_table_commit = new Dictionary<string, TabelCommit>();
                int row_table = 1;
                //Todo : Repeat data
                foreach (CommitModel commit in this.data) {

                    //TODO : create new table
                    if (dict_table_commit.ContainsKey(commit.author_email) == false)
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
                        for(int i=0; i<5; i++)
                            body.AppendChild(
                                new Paragraph());


                        tabelCommit.tbl = tbl;
                        modify_row_table(tabelCommit, commit);
                        dict_table_commit.Add(commit.author_email, tabelCommit);

                        row_table += 1;
                    }
                    else
                    {
                        //TODO : Modify Table
                        TabelCommit tabelCommitAdd = dict_table_commit[commit.author_email];
                        modify_row_table(tabelCommitAdd, commit);
                        tabelCommitAdd.update = true;
                        dict_table_commit[commit.author_email] = tabelCommitAdd;
                    }
                }
            }
            return pth;
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

            //
          

            
            TableRow tr_head = new TableRow();
            
            Shading shading_head = new Shading()
            {
                Color = "auto",
                Fill = "e63946",
                Val = ShadingPatternValues.Clear
            };
            Color color = new Color() { Val = "ffffff"};
            TableCellVerticalAlignment tcVA = new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center };
            Justification justification = new Justification() { Val = JustificationValues.Center };


            RunProperties rp_head = new RunProperties();
            rp_head.AppendChild(new Bold());
            rp_head.AppendChild((Color)color.Clone());

            ParagraphProperties pp_head = new ParagraphProperties();
            pp_head.AppendChild((Justification)justification.Clone());



            // properti run no
            Paragraph p_no = new Paragraph();
            p_no.ParagraphProperties = (ParagraphProperties)pp_head.Clone();
            Run r_no = new Run(new Text("No"));
            r_no.RunProperties = (RunProperties?)rp_head.Clone();
            p_no.AppendChild(r_no);

            // properti run tanggal
            Paragraph p_tgl = new Paragraph();
            p_tgl.ParagraphProperties = (ParagraphProperties)pp_head.Clone();
            Run r_tgl = new Run(new Text("Tanggal"));
            r_tgl.RunProperties = (RunProperties?)rp_head.Clone();
            p_tgl.AppendChild(r_tgl);
            // properti run kegiatan
            Paragraph p_kegiatan = new Paragraph();
            p_kegiatan.ParagraphProperties = (ParagraphProperties)pp_head.Clone();
            Run r_kegiatan = new Run(new Text("Kegiatan"));
            r_kegiatan.RunProperties = (RunProperties?)rp_head.Clone();
            p_kegiatan.AppendChild(r_kegiatan);

            //rp_no.AppendChild(new Bold());

            // Add a cell to each column in the row.
            TableCell tc1 = new TableCell(p_no);
            TableCell tc2 = new TableCell(p_tgl);
            TableCell tc3 = new TableCell(p_kegiatan);
            
            //TODO : setup properties column no
            TableCellWidth tcw_no = new TableCellWidth();
            TableCellProperties tcp_no = new TableCellProperties();
            

            tcw_no.Width = "7%";
            tcw_no.Type = TableWidthUnitValues.Pct;
            tcp_no.AddChild(tcw_no);
            tcp_no.AppendChild((Shading)shading_head.Clone());
            tcp_no.AppendChild((TableCellVerticalAlignment)tcVA.Clone());
            
            tc1.AppendChild(tcp_no);

            //TODO : setup properties column tanggal
            TableCellWidth tcw_tgl = new TableCellWidth();
            TableCellProperties tcp_tgl = new TableCellProperties();
            tcw_tgl.Width = "40%";
            
            tcw_tgl.Type = TableWidthUnitValues.Pct;
            tcp_tgl.AddChild(tcw_tgl);
            tcp_tgl.AppendChild((Shading)shading_head.Clone());
            tcp_tgl.AppendChild((TableCellVerticalAlignment)tcVA.Clone());
            tc2.AppendChild(tcp_tgl);


            TableCellProperties tcp_kegiatan = new TableCellProperties();
            tcp_kegiatan.AppendChild((Shading)shading_head.Clone());
            tcp_kegiatan.AppendChild((TableCellVerticalAlignment)tcVA.Clone());
            tc3.TableCellProperties = tcp_kegiatan;

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
                tabelCommit.update = false;

                day = day.AddDays(1);
                i += 1;
            }
        }

        //TODO : Menambahkan row Tabel
        void modify_row_table(TabelCommit tabelCommit, CommitModel commit)
        {
            DateTime d = (DateTime)commit.committed_date;
            string key = d.Date.ToString(key_date_format);
            
            tabelCommit.tc_3[key].AppendChild(new Paragraph(
                new Run(
                        new Text(commit.message)
                        )));
            

        }
    }

    class TabelCommit
    {
        public Table tbl { set; get; }
        public Dictionary<string, TableCell> tc_1 { set; get; } //merujuk pada row dari cell 1
        public Dictionary<string, TableCell> tc_2 { set; get; }//merujuk pada row dari cell 2
        public Dictionary<string, TableCell> tc_3 { set; get; }//merujuk pada row dari cell 3
        public bool update { set; get; }

    }
}
