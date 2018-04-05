namespace WzlInvoicePdf.DataModel
{
    public class InvoiceGood
    {
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public float ItemVat { get; set; }
        public float ItemPrice { get; set; }
    }
}