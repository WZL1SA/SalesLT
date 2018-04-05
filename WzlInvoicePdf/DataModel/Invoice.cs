using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using MigraDoc.Rendering;

namespace WzlInvoicePdf.DataModel
{
    public class Invoice
    {
        private object xGraphics;

        public List<InvoiceItem> Items { get; set; }
        public Customer Customer { get; set; }
        public Seller Seller { get; set; }

        public void ExportToPdf(string path)
        {
            var document = new Document();
            document.Info.Title = string.Format("Invoice from {0}", Seller.Name);
            document.Info.Author = Seller.Name;
            document.Info.Subject = "Invoice";

            //metody poniższe zwracają obiekty dodawane
            var section = document.AddSection();
            var image = section.Headers.Primary.AddImage(Seller.LogoImagePath);
            image.Height = Unit.FromCentimeter(2.5); // ustawienie wysokości obrazka na 2,5 cm

            image.LockAspectRatio = true;

            //ustawianie wycentrowania
            image.RelativeHorizontal = RelativeHorizontal.Margin;
            image.RelativeVertical = RelativeVertical.Line;
            image.Top = ShapePosition.Top;
            image.Left = ShapePosition.Right;
            
            image.WrapFormat.Style = WrapStyle.Through; //otaczanie elementami np ramka

            // Create the text frame for the address
            var addressFrame = section.AddTextFrame();
            addressFrame.Height = "3.0cm";
            addressFrame.Width = "7.0cm";
            addressFrame.Left = ShapePosition.Left;
            addressFrame.RelativeHorizontal = RelativeHorizontal.Margin;
            addressFrame.Top = "5.0cm";
            addressFrame.RelativeVertical = RelativeVertical.Page;

            // Put sender in address frame
            var paragraph = addressFrame.AddParagraph( Seller.Name + " " + Seller.Address);
            paragraph.Format.Font.Name = "Times New Roman";
            paragraph.Format.Font.Size = 7;
            paragraph.Format.SpaceAfter = 3;

            var customerParagraph = addressFrame.AddParagraph(Customer.Name);
            customerParagraph.AddLineBreak();
            customerParagraph.AddText(Customer.TaxId);
            customerParagraph.AddLineBreak();
            customerParagraph.AddText(Customer.Address);

            // Add the print date field
            var dateParagraph = section.AddParagraph();
            dateParagraph.Format.SpaceBefore = "8cm";
            dateParagraph.Style = "Reference";
            dateParagraph.Format.Font.Color = getColor(System.Drawing.Color.Black);
            dateParagraph.AddFormattedText("FAKTURA", TextFormat.Bold);
            dateParagraph.AddTab();
            dateParagraph.AddText("Łódź, ");
            dateParagraph.AddDateField("dd-MM-yyyy");





            // Create the item table
            var table = section.AddTable();
            table.Style = "Table";
            table.Borders.Color = getColor(System.Drawing.Color.Black);                 
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;

            // Before you can add a row, you must define the columns
            var column = table.AddColumn("1cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn("2.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            column = table.AddColumn("3cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            //column = table.AddColumn("3.5cm");
            //column.Format.Alignment = ParagraphAlignment.Right;

            column = table.AddColumn("2cm");
            column.Format.Alignment = ParagraphAlignment.Center;

            column = table.AddColumn("4cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            // Create the header of the table
            var row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = getColor(System.Drawing.Color.LightBlue);
            row.Cells[0].AddParagraph("Kod towaru");
            row.Cells[0].Format.Font.Bold = false;
            row.Cells[0].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
            row.Cells[0].MergeDown = 1;
            row.Cells[1].AddParagraph("Nazwa");
            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[1].MergeRight = 2;
            //5 >> 4
            row.Cells[4].AddParagraph("Wartość");
            row.Cells[4].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[4].VerticalAlignment = VerticalAlignment.Bottom;
            row.Cells[4].MergeDown = 1;

            row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = getColor(System.Drawing.Color.LightBlue);
            row.Cells[1].AddParagraph("Ilość");
            row.Cells[1].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[2].AddParagraph("Cena jedn. ");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[3].AddParagraph("VAT");
            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            //row.Cells[4].AddParagraph("Taxable");
            //row.Cells[4].Format.Alignment = ParagraphAlignment.Left;

            table.SetEdge(0, 0, 5, 2, Edge.Box, BorderStyle.Single, 0.75, getColor(System.Drawing.Color.Black));//ustawienie tabeli 5 - ilość kolumn

            float totalValue = 0.0f;
            float totalTax = 0.0f;


            foreach(var item in Items)
            {
                var row1 = table.AddRow();
                var row2 = table.AddRow();

                row1.Cells[0].MergeDown = 1; //łączenie komórek z innymi
                row1.Cells[1].MergeRight = 2;
                row1.Cells[4].MergeDown = 1;

                row1.Cells[0].AddParagraph(item.Good.ItemCode);

                var rowPara = row1.Cells[1].AddParagraph(item.Good.ItemName);
                rowPara.Format.Alignment = ParagraphAlignment.Left;

                row2.Cells[1].AddParagraph(item.Quantity.ToString());
                row2.Cells[2].AddParagraph(item.Good.ItemPrice.ToString());
                row2.Cells[3].AddParagraph(item.Good.ItemVat + "%");

                var taxValue = item.Quantity * item.Good.ItemVat / 100 * item.Good.ItemPrice;
                var value = item.Quantity * item.Good.ItemPrice + taxValue;
                totalValue += value;
                totalTax += taxValue;

                row1.Cells[4].AddParagraph(value.ToString("0.00") + "zł");
            }

            row = table.AddRow();
            row.Borders.Visible = false;

            row = table.AddRow();
            var totalParagraph = row.Cells[3].AddParagraph("TOTAL: ");
            totalParagraph.Format.Font.Bold = true;
            row.Cells[0].MergeRight = 2;
            row.Cells[0].Borders.Visible = false;

            row.Cells[4].AddParagraph(totalValue.ToString("0.00") + "zł");

            //row = table.AddRow();
            //row.Cells[3].AddParagraph("TOTAL: ");
            //row.Cells[4].AddParagraph(totalValue.ToString("0.00") + "zł");

            row = table.AddRow();
            var vatParagraph = row.Cells[3].AddParagraph("VAT: ");
            vatParagraph.Format.Font.Bold = true;
            row.Cells[0].MergeRight = 2;
            row.Cells[0].Borders.Visible = false;

            row.Cells[4].AddParagraph(totalTax.ToString("0.00") + "zł");
            
            //row = table.AddRow();
            //row.Cells[3].AddParagraph("VAT:");
            //row.Cells[4].AddParagraph(totalTax.ToString("0.00") + "zł");

            var pdfDoc = new PdfDocument();
            var page = pdfDoc.AddPage();

            var xdr = XGraphics.FromPdfPage(page);  //generuje obiekt do rysowania kanwas graficzny


            var docRender = new DocumentRenderer(document);   //klasa która przetwara instrukcje i przygotowuje metaopis do wygenerowania pdf
            docRender.PrepareDocument(); //przygotowujemy (pre-rendering) dokument MigraDoc
            docRender.RenderPage(xdr, 1); // dodajemy do canvasu tj. do strony w dokumencie PDF

            pdfDoc.Save(path); // zapis do pliku PDF

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
        }

    }
}
