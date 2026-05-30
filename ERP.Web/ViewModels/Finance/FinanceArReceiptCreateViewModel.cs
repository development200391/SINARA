using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceArReceiptCreateViewModel
{
    [Range(1, int.MaxValue)]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [Display(Name = "Receipt Date")]
    public DateOnly ReceiptDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Payment Method")]
    public FinanceArReceiptMethod PaymentMethod { get; set; } = FinanceArReceiptMethod.Transfer;

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

    public IList<FinanceArReceiptApplicationEditViewModel> Applications { get; set; } = [];

    public IReadOnlyList<FinanceIdOptionViewModel> CustomerOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> BankAccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceArInvoiceOutstandingOptionViewModel> OutstandingInvoiceOptions { get; set; } = [];
}

public sealed class FinanceArReceiptApplicationEditViewModel
{
    [Range(1, int.MaxValue)]
    [Display(Name = "Invoice")]
    public int InvoiceId { get; set; }

    [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
    [Display(Name = "Applied Amount")]
    public decimal AppliedAmount { get; set; }
}

public sealed class FinanceArInvoiceOutstandingOptionViewModel
{
    public int CustomerId { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Label { get; set; } = string.Empty;
}
