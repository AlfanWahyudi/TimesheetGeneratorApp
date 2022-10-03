using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using TimesheetGeneratorApp.Models;
using System.Text.RegularExpressions;

namespace TimesheetGeneratorApp.Helper
{
    public class ExportWordDataCommit
    {
        IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data;
        private List<HariLiburModel> hariLiburModels;
        MasterProjectModel mp;
        DateTime tgl_mulai;
        DateTime tgl_selesai;
        String key_date_format = "yyyyMMdd";
        public ExportWordDataCommit(
            MasterProjectModel mp,
            List<HariLiburModel> hariLiburModels,
            IEnumerable<TimesheetGeneratorApp.Models.CommitModel> data,
             DateTime tgl_mulai,
            DateTime tgl_selesai){
            this.hariLiburModels = hariLiburModels;
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

                NumberingDefinitionsPart numberingPart =
                    mainDocumentPart.AddNewPart<NumberingDefinitionsPart>("id_1");
                Numbering element =
                  new Numbering(
                    new AbstractNum(
                      new Level(
                        new NumberingFormat() { Val = NumberFormatValues.Bullet }
                        //new LevelText() { Val = "." }
                      )
                      { LevelIndex = 0 }
                    )
                    { AbstractNumberId = 1 },
                    new NumberingInstance(
                      new AbstractNumId() { Val = 1 }
                    )
                    { NumberID = 1 });
                element.Save(numberingPart);

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
                        //tableProp.AppendChild(
                        //     new TableCellSpacing() { Width = "200", Type = TableWidthUnitValues.Dxa }
                        //);
                       
                        // Make the table width 100% of the page width.
                        TableWidth tableWidth = new TableWidth() { Width = "5000", 
                            Type = TableWidthUnitValues.Pct };
                        // Apply
                        tableProp.Append(tableStyle, tableWidth);
                        tbl.AppendChild(tableProp);

                        // Add 3 columns to the table.
                        TableGrid tg = new TableGrid(new GridColumn(), new GridColumn(), new GridColumn());
                        tbl.AppendChild(tg);

                        //// Create Table Borders

                        TableBorders tblBorders = new TableBorders();
                        string border_color = "f27059";
                        TopBorder topBorder = new TopBorder();
                        topBorder.Val = new EnumValue<BorderValues>(BorderValues.Thick);
                        topBorder.Color = border_color;
                        tblBorders.AppendChild(topBorder);

                        BottomBorder bottomBorder = new BottomBorder();
                        bottomBorder.Val = new EnumValue<BorderValues>(BorderValues.Thick);
                        bottomBorder.Color = border_color;
                        tblBorders.AppendChild(bottomBorder);

                        RightBorder rightBorder = new RightBorder();
                        rightBorder.Val = new EnumValue<BorderValues>(BorderValues.Thick);
                        rightBorder.Color = border_color;
                        tblBorders.AppendChild(rightBorder);

                        LeftBorder leftBorder = new LeftBorder();
                        leftBorder.Val = new EnumValue<BorderValues>(BorderValues.Thick);
                        leftBorder.Color = border_color;
                        tblBorders.AppendChild(leftBorder);

                        InsideHorizontalBorder insideHBorder = new InsideHorizontalBorder();
                        insideHBorder.Val = new EnumValue<BorderValues>(BorderValues.Thick);
                        insideHBorder.Color = border_color;
                        tblBorders.AppendChild(insideHBorder);

                        InsideVerticalBorder insideVBorder = new InsideVerticalBorder();
                        insideVBorder.Val = new EnumValue<BorderValues>(BorderValues.Thick);
                        insideVBorder.Color = border_color;
                        tblBorders.AppendChild(insideVBorder);

                        tableProp.AppendChild(tblBorders);

                        //create header
                        create_header_table(row_table, body, tbl, commit.author_name, "");
                        //init row date
                        create_row_date(tbl, tabelCommit);
                        // Add the table to the document
                        body.AppendChild(tbl);
                        for(int i=0; i<1; i++)
                            body.AppendChild(
                                new Paragraph());


                        tabelCommit.tbl = tbl;
                        tabelCommit.index_table = row_table;
                        modify_row_table(tabelCommit, commit);
                        dict_table_commit.Add(commit.author_email, tabelCommit);

                        row_table += 1;
                    }
                    else
                    {
                        //TODO : Modify Table
                        TabelCommit tabelCommitAdd = dict_table_commit[commit.author_email];
                        modify_row_table(tabelCommitAdd, commit);
                        dict_table_commit[commit.author_email] = tabelCommitAdd;
                    }
                }
            }
            return pth;
        }
        //TODO : Membuat Header Tabel
        void create_header_table(int row_table ,Body body,Table tbl, String nama, String jabatan)
        {
            SpacingBetweenLines spacing = new SpacingBetweenLines() { Line = "240", LineRule = LineSpacingRuleValues.Auto, Before = "0", After = "0" };
            ParagraphProperties pp_default = new ParagraphProperties();
            pp_default.AppendChild(spacing);
            

            Run r_name = new Run();
            r_name.AppendChild(
                new RunProperties(new Bold())
                );
            r_name.AppendChild(new Text(
                row_table.ToString() + ". " + nama
                ));
            Paragraph pr_name = new Paragraph(
                    r_name);
            pr_name.ParagraphProperties = (ParagraphProperties)pp_default.Clone();
            if (row_table > 1)
                pr_name.ParagraphProperties.AppendChild(new PageBreakBefore());
            body.AppendChild(pr_name);

            Paragraph pr_jabatan = new Paragraph(
                new Run(
                    new Text("Jabatan : ")
                    ));
            pr_jabatan.ParagraphProperties = (ParagraphProperties)pp_default.Clone();
            body.AppendChild(pr_jabatan);
            
            TableRow tr_head = new TableRow(new TableRowProperties(
                new TableRowHeight() { Val = 800, HeightType= HeightRuleValues.Exact},
                new TableHeader()
                ));
            
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
            pp_head.AppendChild((ParagraphProperties)pp_default.Clone());
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
            Shading shading_holiday = new Shading()
            {
                Color = "auto",
                Fill = "cad2c5",
                Val = ShadingPatternValues.Clear
            };
            TableCellProperties tcp_def = new TableCellProperties();
            tcp_def.AppendChild(
                new TableCellMargin(
                    new LeftMargin() { Width = "100", Type = TableWidthUnitValues.Dxa }
                    ));
            
            //Membuat settingan default paragraph
            SpacingBetweenLines spacing = new SpacingBetweenLines() 
            { Line = "240", LineRule = LineSpacingRuleValues.Auto, 
                Before = "250", After = "100" }; //membuat spasi pada paragraph
            Justification justification = new Justification() { Val = JustificationValues.Center };
            ParagraphProperties pp_default = new ParagraphProperties();
            pp_default.AppendChild(justification);
            pp_default.AppendChild((SpacingBetweenLines)spacing.Clone());

            ParagraphProperties pp_date = new ParagraphProperties();
            pp_date.AppendChild((SpacingBetweenLines)spacing.Clone());

            var day = this.tgl_mulai.Date;
            int i = 1;
            tabelCommit.tc_3 = new Dictionary<string, TableCell>();
            tabelCommit.update = new Dictionary<string, bool>();

            while (day <= this.tgl_selesai.Date)
            {
                TableRow tr = new TableRow();
                //column no
                Paragraph p_no = new Paragraph();
                p_no.ParagraphProperties = (ParagraphProperties)pp_default.Clone();
                p_no.AppendChild(new Run(
                            new Text(i.ToString())
                            ));
                //TableCellProperties tcp_1 = new TableCellProperties();
                //tcp_1.AppendChild((Shading)shading_holiday.Clone());

                TableCell tc_1 = new TableCell(p_no);
                //tc_1.TableCellProperties = (TableCellProperties)tcp_def.Clone();
                //tc_1.TableCellProperties = tcp_1;

                //column date
                Paragraph p_date = new Paragraph();
                p_date.ParagraphProperties = (ParagraphProperties)pp_date.Clone();
                p_date.AppendChild(new Run(
                            new Text(day.ToString("dddd, dd MMM yyyy"))
                            ));
                TableCell tc_2 = new TableCell(p_date);
                tc_2.TableCellProperties = (TableCellProperties)tcp_def.Clone();


                TableCell tc_3 = new TableCell(
                    new Paragraph(
                        new Run(
                            new Text("")
                            )));

                tr.Append(tc_1, tc_2, tc_3);
                tbl.Append(tr);
                tabelCommit.tc_3.Add(day.ToString(key_date_format), tc_3);
                tabelCommit.update.Add(day.ToString(key_date_format), false);


                string dayName = day.ToString("dddd").ToLower();
                if(dayName.Equals("sabtu") == true |
                   dayName.Equals("minggu") == true | isNasionalHoliday(day) == true)
                {
                    tc_2.TableCellProperties.AppendChild((Shading)shading_holiday.Clone());
                    tc_1.TableCellProperties = new TableCellProperties((Shading)shading_holiday.Clone());
                    tc_3.TableCellProperties = new TableCellProperties((Shading)shading_holiday.Clone());
                }

                day = day.AddDays(1);
                i += 1;
            }
        }

        //TODO : Menambahkan row Tabel
        void modify_row_table(TabelCommit tabelCommit, CommitModel commit)
        {
            DateTime d = (DateTime)commit.committed_date;
            string key = d.Date.ToString(key_date_format);
            if (tabelCommit.update[key] == false)
            {
                Shading shading_holiday = new Shading()
                {
                    Color = "auto",
                    Fill = "cad2c5",
                    Val = ShadingPatternValues.Clear
                };
                TableCellProperties tcp_def = new TableCellProperties();
                tcp_def.AppendChild(
                    new TableCellMargin(
                        new LeftMargin() { Width = "100", Type = TableWidthUnitValues.Dxa },
                        new RightMargin() { Width = "100", Type = TableWidthUnitValues.Dxa },
                        new BottomMargin() { Width = "100", Type = TableWidthUnitValues.Dxa }
                        ));
                tabelCommit.tc_3[key].RemoveAllChildren();

                string dayName = d.ToString("dddd").ToLower();

                if (dayName.Equals("sabtu") == true |
                   dayName.Equals("minggu") == true | isNasionalHoliday(d) == true)
                {
                    tcp_def.AppendChild((Shading)shading_holiday.Clone());
                }

                tabelCommit.tc_3[key].TableCellProperties = tcp_def;

                tabelCommit.update[key] = true;
            }
            SpacingBetweenLines spacing = new SpacingBetweenLines()
            {
                Line = "240",
                LineRule = LineSpacingRuleValues.Auto,
                Before = "100",
                After = "0"
            }; //membuat spasi pada paragraph

            Regex pattern = new Regex("[\n]{2}");
            Regex pattern_last_newline = new Regex("[.+\n]$");

            string message = pattern.Replace(commit.message, "\n");
            message = pattern_last_newline.Replace(message, "");
            message = message.Replace("\t", "");

            
            ParagraphProperties pp = new ParagraphProperties(spacing);

            Paragraph p = new Paragraph(
                pp,
                new Run(
                  new RunProperties(),
                  new Text(message)
                  { Space = SpaceProcessingModeValues.Preserve })
                );
            tabelCommit.tc_3[key].AppendChild(p);

        }

        //TODO : Chek libur nasional
        bool isNasionalHoliday(DateTime d)
        {
            foreach (var hariLiburNasional in hariLiburModels)
            {
                if (d.Date == hariLiburNasional.holiday_date.Value.Date)
                    return true;
            }
            return false;
        }
    }

    class TabelCommit
    {
        public Table tbl { set; get; }
        public Dictionary<string, TableCell> tc_3 { set; get; }//merujuk pada row dari cell 3
        public Dictionary<string,bool> update { set; get; }
        public int index_table { set; get; }

    }
}
