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
            try
            {
                return await _http.GetFromJsonAsync<List<StudentDto>>(url) ?? new List<StudentDto>();
            }
            catch
            {
                await Task.Delay(4000);
                return await _http.GetFromJsonAsync<List<StudentDto>>(url) ?? new List<StudentDto>();
            }
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
    }
}
