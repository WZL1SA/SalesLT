using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace WzlDatabaseReport.Report
{
    public class PdfReport
    {
        public string Title { set; get; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime Time { get; set; }
        public string CoverImagePath { get; set; }

        private Document document;

        public PdfDocument CreateReport()
        {
            var pdfDocument = new PdfDocument();

             document = new Document();
            DefineStyles(document);

            CreateTitlePage();

            CreateTableOfContents();

            CreateIntroduction();

            CreateDataTable();


            var renderer = new DocumentRenderer(document);
            renderer.PrepareDocument();

            for(int i =0; i<renderer.FormattedDocument.PageCount; i++)
            {
                //każda sekcja to jest strona
                var gfx = XGraphics.FromPdfPage(pdfDocument.AddPage());
                renderer.RenderPage(gfx, i+1);
            }

          
            return pdfDocument;
            
        }

        private void CreateDataTable()
        {
            if (document == null) return;
            var section = document.AddSection();

            var paragraph = section.AddParagraph("Tabela danych");
            paragraph.AddBookmark("Tabela");
            paragraph.Style = "Title";
            paragraph = section.AddParagraph();


            // Create the item table
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Color = getColor(System.Drawing.Color.Black);
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            // Before you can add a row, you must define the columns
            var column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Left;

            // Create the header of the table
            var row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = getColor(System.Drawing.Color.LightBlue);

            string[] headers = new string[] { "ID", "Imię", "Drugie imię", "Nazwisko", "e-mail", "Hasło" };


            for(int i=0; i <headers.Length; i++)
            {
                row.Cells[i].AddParagraph(headers[i]);
                row.Cells[i].Format.Font.Bold = false;
                row.Cells[i].Format.Alignment = ParagraphAlignment.Center;
                row.Cells[i].VerticalAlignment = VerticalAlignment.Center;
                row.Cells[i].MergeDown = 1;
            }

            

            row = table.AddRow();            
            row.Shading.Color = getColor(System.Drawing.Color.LightBlue);
            

            table.SetEdge(0, 0, 6, 2, Edge.Box, BorderStyle.Single, 0.75, getColor(System.Drawing.Color.Black));//ustawienie tabeli 5 - ilość kolumn



            using (AzureDb context = new AzureDb())
            {
                int k = 0;
                foreach (var item in context.Customer)
                {
                    row = table.AddRow();

                    row.Cells[0].AddParagraph(item.CustomerID.ToString());
                    row.Cells[1].AddParagraph(item.FirstName);
                    row.Cells[2].AddParagraph(item.MiddleName==null ? "" : item.MiddleName);
                    row.Cells[3].AddParagraph(item.LastName);
                    row.Cells[4].AddParagraph(item.EmailAddress == null ? "" : item.EmailAddress);
                    row.Cells[5].AddParagraph(item.PasswordHash);
                    if(k % 2 == 0)
                    {
                        row.Format.Shading.Color = getColor(System.Drawing.Color.LightGray);
                    }else
                    {
                        row.Format.Shading.Color = getColor(System.Drawing.Color.NavajoWhite);
                    }
                    k++;
                }
            }
        }

        private void CreateIntroduction()
        {
            if (document == null) return;
            var section = document.AddSection();

            var paragraph = section.AddParagraph("Wprowadzenie do rap");
            paragraph.AddBookmark("Wprowadzenie");
            paragraph.Style = "Title";
            paragraph = section.AddParagraph();
            paragraph.AddText("Lorem Ipsum jest tekstem stosowanym jako przykładowy wypełniacz w przemyśle poligraficznym. Został po raz pierwszy użyty w XV w. przez nieznanego drukarza do wypełnienia tekstem próbnej książki. Pięć wieków później zaczął być używany przemyśle elektronicznym, pozostając praktycznie niezmienionym. Spopularyzował się w latach 60. XX w. wraz z publikacją arkuszy Letrasetu, zawierających fragmenty Lorem Ipsum, a ostatnio z zawierającym różne wersje Lorem Ipsum oprogramowaniem przeznaczonym do realizacji druków na komputerach osobistych, jak Aldus PageMaker");

            section.AddParagraph();
            paragraph.AddText("Lorem Ipsum jest tekstem stosowanym jako przykładowy wypełniacz w przemyśle poligraficznym. Został po raz pierwszy użyty w XV w. przez nieznanego drukarza do wypełnienia tekstem próbnej książki. Pięć wieków później zaczął być używany przemyśle elektronicznym, pozostając praktycznie niezmienionym. Spopularyzował się w latach 60. XX w. wraz z publikacją arkuszy Letrasetu, zawierających fragmenty Lorem Ipsum, a ostatnio z zawierającym różne wersje Lorem Ipsum oprogramowaniem przeznaczonym do realizacji druków na komputerach osobistych, jak Aldus PageMaker");
            paragraph.Format.Font.Color = getColor(System.Drawing.Color.DarkGreen);
        }

        private void CreateTableOfContents()
        {
            if (document == null) return;
            var section = document.AddSection();

            var paragraph = section.AddParagraph("Spis treści");
            paragraph = section.AddParagraph();

            var hyperlink = paragraph.AddHyperlink("Wprowadzenie"); // teksty klikalne
            hyperlink.AddText("Wprowadzenie do raportu");
            paragraph.AddLineBreak();

            hyperlink = paragraph.AddHyperlink("Tabela"); // teksty klikalne
            hyperlink.AddText("Tabela wysw");
            paragraph.AddLineBreak();

            hyperlink = paragraph.AddHyperlink("Wykres"); // teksty klikalne
            hyperlink.AddText("Wykres wysw");
            paragraph.AddLineBreak();

            hyperlink = paragraph.AddHyperlink("Changes"); // teksty klikalne
            hyperlink.AddText("Wykaz zmian");
            paragraph.AddLineBreak();
        }

        private void CreateTitlePage()
        {
            if (document == null) return;

            var section = document.AddSection();

            var image = section.AddImage(CoverImagePath);

            image.Width = Unit.FromCentimeter(8);
            image.LockAspectRatio = true;

            //ustawianie wycentrowania
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.RelativeVertical = RelativeVertical.Line;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Left;
            image.WrapFormat.Style = WrapStyle.Through; //otaczanie elementami np ramka


            // Create the text frame for the address
            var titleFrame = section.AddTextFrame();
            titleFrame.Height = "3.0cm";
            titleFrame.Width = "7.0cm";
            titleFrame.Left = ShapePosition.Left;
            titleFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            titleFrame.Top = "10.0cm";
            titleFrame.RelativeVertical = RelativeVertical.Page;


            var titleBox = titleFrame.AddParagraph(Title);
            titleBox.Style = "Title";

            var authorBox = titleFrame.AddParagraph(Author);
            authorBox.Style = "Normal";


        }

        void DefineStyles(Document document)
        {
            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Verdana";

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called Table based on style Normal
            style = document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Verdana";
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;

            // Create a new style called Reference based on style Normal
            style = document.Styles.AddStyle("Reference", "Normal");
            style.ParagraphFormat.SpaceBefore = "5mm";
            style.ParagraphFormat.SpaceAfter = "5mm";
            style.ParagraphFormat.TabStops.AddTabStop("16cm", TabAlignment.Right);


            //definicja nowego stylu
            style = document.Styles.AddStyle("Title", "Normal");
            style.ParagraphFormat.Font.Size = 20;
            style.ParagraphFormat.Font.Name = "Calibri";
            style.ParagraphFormat.SpaceBefore = "10mm";
            style.ParagraphFormat.SpaceAfter = "10mm";


        }

        //konwerter kolorów system drwing na kolory MigraDoc
        Color getColor(System.Drawing.Color color)
        {
            return new Color(
                color.R,
                color.G,
                color.B
                );
        }
    }
}
