using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WzlInvoicePdf.DataModel;

namespace WzlInvoicePdf
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<InvoiceItem> InvoiceItemsCollection { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InvoiceItemsCollection = new ObservableCollection<InvoiceItem>();
            InvoiceItems.ItemsSource = InvoiceItemsCollection;

           // printPdf();

        }

        private static void printPdf()
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Created with PDFsharp";

            // Create an empty page
            PdfPage page = document.AddPage();

            // Get an XGraphics object for drawing
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Create a font
            XFont font = new XFont("Verdana", 20, XFontStyle.BoldItalic);

            // Draw the text
            gfx.DrawString("Hello, World!", font, XBrushes.Black,
              new XRect(0, 0, page.Width, page.Height),
              XStringFormats.Center);

            // Save the document...
            const string filename = "HelloWorld.pdf";
            document.Save(filename);
            // ...and start a viewer.
            Process.Start(filename);
        }

        private void AddItemToInvoice_Click(object sender, RoutedEventArgs e)
        {
            float price, vat;
            float.TryParse(ItemPriceTb.Text, out price);
            float.TryParse(ItemVatTb.Text, out vat);

            var name = ItemNameTb.Text;
            var code = ItemCodeTb.Text;            
            int.TryParse(ItemQuantityTb.Text, out int quantity);

            InvoiceItemsCollection.Add(
                new InvoiceItem()
                {
                    Good = new InvoiceGood()
                    {
                        ItemCode = code,
                        ItemName = name,
                        ItemPrice = price,
                        ItemVat = vat
                    },
                    Quantity = quantity
                }
                );
            ItemNameTb.Text = string.Empty;
            ItemCodeTb.Text = string.Empty;
            ItemPriceTb.Text = string.Empty;
            ItemVatTb.Text = string.Empty;
            ItemQuantityTb.Text = string.Empty;

        }

        private void GenerateInvoiceItem_Click(object sender, RoutedEventArgs e)
        {
            var customer = new Customer()
            {
                Name = CustomerNameTb.Text,
                Address = CustomerAddressTb.Text,
                TaxId = CustomerTaxtIdTb.Text
            };

            var seller = new Seller()
            {
                Name = "Sprzedawca",
                Address = "ul Sliska nr zjechał",
                TaxId = "999-999-999",
                LogoImagePath = "logo.jpg"
            };

            var invoice = new Invoice()
            {
                Customer = customer,
                Seller = seller,
                Items = new List<InvoiceItem>()
            };
            invoice.Items.AddRange(InvoiceItemsCollection); //dodajemy elementy z listy ObservableCollection do kolejnej listy
            Guid guid = Guid.NewGuid(); // unikalny ID
            invoice.ExportToPdf(string.Format("invoice_{0}.pdf" , guid));
            
        }
    }
}
