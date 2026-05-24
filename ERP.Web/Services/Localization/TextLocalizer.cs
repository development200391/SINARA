using System.Security.Claims;

namespace ERP.Web.Services.Localization;

public sealed class TextLocalizer(IHttpContextAccessor httpContextAccessor) : ITextLocalizer
{
    private const string LanguageCookie = "sinara_lang";

    private static readonly Dictionary<string, Dictionary<string, string>> Resources = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["common.save"] = "Save",
            ["common.cancel"] = "Cancel",
            ["common.edit"] = "Edit",
            ["common.delete"] = "Delete",
            ["common.search"] = "Search",
            ["common.add_new"] = "Add New",
            ["msg.save_success"] = "Data saved successfully.",
            ["msg.delete_success"] = "Data deleted successfully.",
            ["msg.no_data"] = "No data available.",
            ["msg.access_denied"] = "You do not have permission.",
            ["label.full_name"] = "Full Name",
            ["label.department"] = "Department",
            ["label.position"] = "Position",
            ["label.hire_date"] = "Hire Date",
            ["label.employee_code"] = "Employee Code",
            ["nav.employees"] = "Employees",
            ["nav.attendance"] = "Attendance",
            ["nav.payroll"] = "Payroll",
            ["nav.leave"] = "Leave",
            ["nav.users"] = "Users",
            ["nav.roles"] = "Roles",
            ["nav.menus"] = "Menu Config",
            ["nav.settings"] = "Settings",
            ["nav.audit"] = "Audit Log",
            ["auth.login"] = "Login",
            ["auth.username"] = "Username",
            ["auth.password"] = "Password",
            ["auth.remember_me"] = "Remember Me",
            ["auth.sign_in"] = "Sign In",
            ["layout.home"] = "Home",
            ["layout.profile"] = "Profile",
            ["layout.logout"] = "Logout",
            ["layout.configuration"] = "Configuration",
            ["layout.hr"] = "Human Resources"
        },
        ["id"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["common.save"] = "Simpan",
            ["common.cancel"] = "Batal",
            ["common.edit"] = "Ubah",
            ["common.delete"] = "Hapus",
            ["common.search"] = "Cari",
            ["common.add_new"] = "Tambah Baru",
            ["msg.save_success"] = "Data berhasil disimpan.",
            ["msg.delete_success"] = "Data berhasil dihapus.",
            ["msg.no_data"] = "Tidak ada data.",
            ["msg.access_denied"] = "Anda tidak memiliki izin.",
            ["label.full_name"] = "Nama Lengkap",
            ["label.department"] = "Departemen",
            ["label.position"] = "Jabatan",
            ["label.hire_date"] = "Tanggal Masuk",
            ["label.employee_code"] = "Kode Karyawan",
            ["nav.employees"] = "Karyawan",
            ["nav.attendance"] = "Kehadiran",
            ["nav.payroll"] = "Penggajian",
            ["nav.leave"] = "Cuti",
            ["nav.users"] = "Pengguna",
            ["nav.roles"] = "Peran",
            ["nav.menus"] = "Konfigurasi Menu",
            ["nav.settings"] = "Pengaturan",
            ["nav.audit"] = "Audit Log",
            ["auth.login"] = "Masuk",
            ["auth.username"] = "Username",
            ["auth.password"] = "Kata Sandi",
            ["auth.remember_me"] = "Ingat Saya",
            ["auth.sign_in"] = "Masuk",
            ["layout.home"] = "Beranda",
            ["layout.profile"] = "Profil",
            ["layout.logout"] = "Keluar",
            ["layout.configuration"] = "Konfigurasi",
            ["layout.hr"] = "Sumber Daya Manusia"
        }
    };

    public string CurrentLanguage
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null)
            {
                return "en";
            }

            if (context.Request.Cookies.TryGetValue(LanguageCookie, out var cookieLanguage) && IsSupported(cookieLanguage))
            {
                return cookieLanguage;
            }

            var claimLanguage = context.User.FindFirstValue("language");
            return IsSupported(claimLanguage) ? claimLanguage! : "en";
        }
    }

    public string this[string key]
    {
        get
        {
            var language = CurrentLanguage;

            if (Resources.TryGetValue(language, out var localized) && localized.TryGetValue(key, out var value))
            {
                return value;
            }

            if (Resources["en"].TryGetValue(key, out var fallback))
            {
                return fallback;
            }

            return key;
        }
    }

    private static bool IsSupported(string? code)
    {
        return !string.IsNullOrWhiteSpace(code) && Resources.ContainsKey(code);
    }
}
