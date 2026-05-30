using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceApInvoiceEditViewModel
{
    public int? Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Vendor")]
    public int VendorId { get; set; }

    [MaxLength(100)]
    [Display(Name = "Vendor Invoice No")]
    public string? VendorInvoiceNo { get; set; }

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

    public FinanceApInvoiceStatus Status { get; set; } = FinanceApInvoiceStatus.Draft;
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public bool IsOverdue { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> VendorOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceCodeOptionViewModel> CurrencyOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> TaxCodeOptions { get; set; } = [];

    public IList<FinanceApInvoiceLineEditViewModel> Lines { get; set; } =
    [
        new FinanceApInvoiceLineEditViewModel()
    ];

    public bool IsReadOnly => Id.HasValue && Status != FinanceApInvoiceStatus.Draft;
}

public sealed class FinanceApInvoiceLineEditViewModel
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
