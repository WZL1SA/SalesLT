using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WzlInvoicePdf.DataModel
{
    public class InvoiceItem
    {
        public int Quantity { get; set; }
        public InvoiceGood Good { get; set; }
    }
}
