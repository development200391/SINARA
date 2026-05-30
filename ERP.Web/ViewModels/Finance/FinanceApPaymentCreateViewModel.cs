using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceApPaymentCreateViewModel
{
    [Range(1, int.MaxValue)]
    [Display(Name = "Vendor")]
    public int VendorId { get; set; }

    [Display(Name = "Payment Date")]
    public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Payment Method")]
    public FinanceApPaymentMethod PaymentMethod { get; set; } = FinanceApPaymentMethod.Transfer;

    [Range(1, int.MaxValue)]
    [Display(Name = "Bank Account")]
    public int BankAccountId { get; set; }

    [MaxLength(100)]
    [Display(Name = "Reference No")]
    public string? ReferenceNo { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    public IList<FinanceApPaymentApplicationEditViewModel> Applications { get; set; } = [];

    public IReadOnlyList<FinanceIdOptionViewModel> VendorOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> BankAccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceApInvoiceOutstandingOptionViewModel> OutstandingInvoiceOptions { get; set; } = [];
}

public sealed class FinanceApPaymentApplicationEditViewModel
{
    [Range(1, int.MaxValue)]
    [Display(Name = "Invoice")]
    public int InvoiceId { get; set; }

    [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
    [Display(Name = "Applied Amount")]
    public decimal AppliedAmount { get; set; }
}

public sealed class FinanceApInvoiceOutstandingOptionViewModel
{
    public int VendorId { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Label { get; set; } = string.Empty;
}
