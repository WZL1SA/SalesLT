using System;
using System.Collections.Generic;
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
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WzlDatabaseReport.Report;

namespace WzlDatabaseReport
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateReportBtn_Click(object sender, RoutedEventArgs e)
        {
            var pdfReport = new PdfReport()
            {
                Title = "Raport z bazy AzurDB, tabela Customer",
                Author = "Witek M",
                CoverImagePath = "logo.jpg"
            };
            var report = pdfReport.CreateReport();

            report.Save(string.Format("report_{0}.pdf", Guid.NewGuid()));

            MessageBox.Show("Zapisano plik", "Sukces", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
