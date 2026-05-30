using ERP.Domain.Entities.Config;
using ERP.Domain.Entities.HR;
using ERP.Domain.Entities.Finance;

namespace ERP.Domain.Entities.System;

public sealed class SysUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string LanguagePreference { get; set; } = "en";
    public bool IsActive { get; set; } = true;

    public ICollection<SysUserRole> UserRoles { get; set; } = new List<SysUserRole>();
    public ICollection<SysRefreshToken> RefreshTokens { get; set; } = new List<SysRefreshToken>();
    public ICollection<HrEmployee> Employees { get; set; } = new List<HrEmployee>();
    public ICollection<HrLeaveRequest> ApprovedLeaveRequests { get; set; } = new List<HrLeaveRequest>();
    public ICollection<HrPayrollRun> ProcessedPayrollRuns { get; set; } = new List<HrPayrollRun>();
    public ICollection<FinJournalEntry> PostedFinanceJournals { get; set; } = new List<FinJournalEntry>();
}
