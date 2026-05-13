using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entites
{
    public class InvoiceModel
    {
        public int Id { get; set; }
        public string Customer { get; set; } = string.Empty;
        public decimal Amount { get; set; } 
        public DateTime Date { get; set; }

    }
    public class NewInvoiceModel
    {
        public string Customer { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

}
