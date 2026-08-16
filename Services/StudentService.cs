using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace BalighAcademy.Services
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FirstNameEn { get; set; } = "";
        public string LastNameEn { get; set; } = "";
        public string? Mobile { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? BirthDate { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
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

        public async Task<List<StudentDto>> GetAll()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<StudentDto>>(_base + "/api/students") ?? new List<StudentDto>();
            }
            catch
            {
                await Task.Delay(4000);
                return await _http.GetFromJsonAsync<List<StudentDto>>(_base + "/api/students") ?? new List<StudentDto>();
            }
        }

        public async Task Create(StudentDto s)
        {
            try
            {
                await _http.PostAsJsonAsync(_base + "/api/students", s);
            }
            catch
            {
                await Task.Delay(4000);
                await _http.PostAsJsonAsync(_base + "/api/students", s);
            }
        }
    }
}
