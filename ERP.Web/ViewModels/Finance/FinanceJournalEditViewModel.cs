using System.ComponentModel.DataAnnotations;
using ERP.Domain.Enums;

namespace ERP.Web.ViewModels.Finance;

public sealed class FinanceJournalEditViewModel
{
    public int? Id { get; set; }
    public string JournalNo { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Period")]
    public int PeriodId { get; set; }

    [Display(Name = "Date")]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Source")]
    public FinanceJournalSource Source { get; set; } = FinanceJournalSource.Manual;

    public int? SourceRefId { get; set; }

    [MaxLength(50)]
    [Display(Name = "Source Ref Type")]
    public string? SourceRefType { get; set; }

    [Required]
    [MaxLength(10)]
    [Display(Name = "Currency")]
    public string CurrencyCode { get; set; } = "IDR";

    [Range(typeof(decimal), "0.000001", "79228162514264337593543950335")]
    [Display(Name = "Exchange Rate")]
    public decimal ExchangeRate { get; set; } = 1m;

    public FinanceJournalStatus Status { get; set; } = FinanceJournalStatus.Draft;
    public DateTimeOffset? PostedAt { get; set; }
    public string? PostedByName { get; set; }
    public int? ReversedJournalId { get; set; }

    public IReadOnlyList<FinanceIdOptionViewModel> PeriodOptions { get; set; } = [];
    public IReadOnlyList<FinanceCodeOptionViewModel> CurrencyOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> AccountOptions { get; set; } = [];
    public IReadOnlyList<FinanceIdOptionViewModel> CostCenterOptions { get; set; } = [];

    public IList<FinanceJournalLineEditViewModel> Lines { get; set; } = [
        new FinanceJournalLineEditViewModel(),
        new FinanceJournalLineEditViewModel()
    ];

    public bool IsReadOnly => Id.HasValue && Status != FinanceJournalStatus.Draft;
}

public sealed class FinanceJournalLineEditViewModel
{
    public int? Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Account")]
    public int AccountId { get; set; }

    [Display(Name = "Cost Center")]
    public int? CostCenterId { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Debit { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal Credit { get; set; }
}
