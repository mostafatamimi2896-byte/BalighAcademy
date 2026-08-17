using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace BalighAcademy.Services
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FirstNameEn { get; set; } = "";
        public string LastNameEn { get; set; } = "";
        public string? Gender { get; set; }
        public string? BirthDate { get; set; }
        public string? Mobile { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public string? PhotoBase64 { get; set; }
        public string QuotaType { get; set; } = "عادی / Regular";
        public string Status { get; set; } = "فعال / Active";
        public DateTime CreatedAt { get; set; }
    }

    public class TermDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public bool IsCurrent { get; set; }
    }

    public class EnrollmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TermId { get; set; }
        public string ClassName { get; set; } = "";
        public string Level { get; set; } = "";
        public string Result { get; set; } = "در حال برگزاری / Ongoing";
        public double Score { get; set; }
    }

       public class PaymentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int VoucherNo { get; set; }
        public string ReceiptNo { get; set; } = "";
        public string Date { get; set; } = "";
        public long Amount { get; set; }
        public string Kind { get; set; } = "شهریه / Tuition";
        public string Bank { get; set; } = "";
        public string Note { get; set; } = "";
    }
    }

    public class FinanceDto
    {
        public long Tuition { get; set; }
        public long Paid { get; set; }
        public long Discount { get; set; }
        public long Balance { get; set; }
        public bool Debtor { get; set; }
    }

    public class StudentService
    {
        private readonly HttpClient _http;
        private readonly string _base;

        public StudentService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _base = (config["ApiBase"] ?? "").TrimEnd('/');
        }

        public async Task<List<StudentDto>> GetAll(string? q = null)
        {
            var url = _base + "/api/students" + (string.IsNullOrWhiteSpace(q) ? "" : "?q=" + Uri.EscapeDataString(q));
            try { return await _http.GetFromJsonAsync<List<StudentDto>>(url) ?? new List<StudentDto>(); }
            catch { await Task.Delay(4000); return await _http.GetFromJsonAsync<List<StudentDto>>(url) ?? new List<StudentDto>(); }
        }

        public async Task<StudentDto?> GetOne(int id)
            => await _http.GetFromJsonAsync<StudentDto>(_base + $"/api/students/{id}");

        public async Task Create(StudentDto s)
        {
            try { await _http.PostAsJsonAsync(_base + "/api/students", s); }
            catch { await Task.Delay(4000); await _http.PostAsJsonAsync(_base + "/api/students", s); }
        }

        public async Task Update(StudentDto s)
            => await _http.PutAsJsonAsync(_base + $"/api/students/{s.Id}", s);

        public async Task Delete(int id)
            => await _http.DeleteAsync(_base + $"/api/students/{id}");

        public async Task<FinanceDto?> GetFinance(int id)
            => await _http.GetFromJsonAsync<FinanceDto>(_base + $"/api/students/{id}/finance");

        public async Task<List<EnrollmentDto>> GetEnrollments(int id)
            => await _http.GetFromJsonAsync<List<EnrollmentDto>>(_base + $"/api/students/{id}/enrollments") ?? new List<EnrollmentDto>();

        public async Task<List<PaymentDto>> GetPayments(int id)
            => await _http.GetFromJsonAsync<List<PaymentDto>>(_base + $"/api/students/{id}/payments") ?? new List<PaymentDto>();

        public async Task<List<TermDto>> GetTerms()
            => await _http.GetFromJsonAsync<List<TermDto>>(_base + "/api/terms") ?? new List<TermDto>();

        public async Task CreateTerm(TermDto t)
            => await _http.PostAsJsonAsync(_base + "/api/terms", t);

        public async Task CreateEnrollment(int id, EnrollmentDto e)
            => await _http.PostAsJsonAsync(_base + $"/api/students/{id}/enrollments", e);

        public async Task DeleteEnrollment(int id)
            => await _http.DeleteAsync(_base + $"/api/enrollments/{id}");

        public async Task CreatePayment(int id, PaymentDto p)
            => await _http.PostAsJsonAsync(_base + $"/api/students/{id}/payments", p);

        public async Task DeletePayment(int id)
            => await _http.DeleteAsync(_base + $"/api/payments/{id}");
    }
}
