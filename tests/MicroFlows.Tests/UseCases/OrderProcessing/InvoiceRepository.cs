using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Tests.UseCases.OrderProcessing;

public interface IInvoiceRepository
{
    Task<bool> Save(InvoiceModel invoice);
}

public class InvoiceModel
{
    public string Id { get; set; }
    public string CorrelationId { get; set; }
    public string OrderId { get; set; }
}

//public class InvoiceRepository : IInvoiceRepository
//{
//}
