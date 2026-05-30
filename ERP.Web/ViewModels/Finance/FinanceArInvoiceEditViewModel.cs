using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceArInvoiceEditViewModel
{
    public int? Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Period")]
    public int PeriodId { get; set; }

    [Display(Name = "Invoice Date")]
    public DateOnly InvoiceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Due Date")]
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(10)]
    [Display(Name = "Currency")]
    public string CurrencyCode { get; set; } = "IDR";

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    [Display(Name = "Exchange Rate")]
    public decimal ExchangeRate { get; set; } = 1m;

    public FinanceArInvoiceStatus Status { get; set; } = FinanceArInvoiceStatus.Draft;
    public DateTimeOffset? SentAt { get; set; }
    public string? SentByName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public bool IsOverdue { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> CustomerOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceCodeOptionViewModel> CurrencyOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> TaxCodeOptions { get; set; } = [];

    public IList<FinanceArInvoiceLineEditViewModel> Lines { get; set; } =
    [
        new FinanceArInvoiceLineEditViewModel()
    ];

    public bool IsReadOnly => Id.HasValue && Status != FinanceArInvoiceStatus.Draft;
}

public sealed class FinanceArInvoiceLineEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "79228162514264337593543950335")]
    [Display(Name = "Quantity")]
    public decimal Quantity { get; set; } = 1m;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "Tax Code")]
    public int? TaxCodeId { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    [Display(Name = "Tax Amount")]
    public decimal TaxAmount { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Account")]
    public int AccountId { get; set; }

    [Display(Name = "Cost Center")]
    public int? CostCenterId { get; set; }
}
