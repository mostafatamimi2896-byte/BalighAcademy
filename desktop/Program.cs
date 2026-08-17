using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BalighDesktop
{
    static class Program
    {
        public static readonly string ApiBase = "https://baligh-api-hyg4.onrender.com";
        public static readonly HttpClient Http = new HttpClient();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentCode { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FirstNameEn { get; set; } = "";
        public string LastNameEn { get; set; } = "";
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }
        public string PhotoBase64 { get; set; }
        public string QuotaType { get; set; } = "عادی / Regular";
        public string Status { get; set; } = "فعال / Active";
    }

    public class FinanceDto
    {
        public long Tuition { get; set; }
        public long Paid { get; set; }
        public long Discount { get; set; }
        public long Balance { get; set; }
        public bool Debtor { get; set; }
    }

    static class Api
    {
        static readonly JsonSerializerOptions O = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public static async Task<List<StudentDto>> GetStudents(string q = "")
        {
            var url = Program.ApiBase + "/api/students" + (string.IsNullOrWhiteSpace(q) ? "" : "?q=" + Uri.EscapeDataString(q));
            var json = await Program.Http.GetStringAsync(url);
            return JsonSerializer.Deserialize<List<StudentDto>>(json, O) ?? new List<StudentDto>();
        }

        public static async Task SaveStudent(StudentDto s)
        {
            var body = new StringContent(JsonSerializer.Serialize(s), Encoding.UTF8, "application/json");
            await Program.Http.PostAsync(Program.ApiBase + "/api/students", body);
        }

        public static async Task<FinanceDto> GetFinance(int id)
        {
            var json = await Program.Http.GetStringAsync(Program.ApiBase + $"/api/students/{id}/finance");
            return JsonSerializer.Deserialize<FinanceDto>(json, O);
        }
    }

    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "نرم‌افزار مدیریت آموزشگاه بلیغ (۲٫۲)";
            Width = 950; Height = 620;
            RightToLeft = RightToLeft.Yes;
            Font = new Font("Tahoma", 10);
            StartPosition = FormStartPosition.CenterScreen;

            var lbl = new Label
            {
                  Text = "آموزشگاه زبان بلیغ",
                Font = new Font("Tahoma", 22, FontStyle.Bold),
                Dock = DockStyle.Top, Height = 80,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.SteelBlue, ForeColor = Color.White
            };
            

            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(20) };
            string[] names = { "ثبت نام", "امور شهریه", "کارنامه", "اساتید", "حسابداری", "گزارشات", "مدیریت", "ابزارها" };
            foreach (var n in names)
            {
                var b = new Button { Text = n, Width = 160, Height = 65, Margin = new Padding(10), Font = new Font("Tahoma", 11, FontStyle.Bold) };
                if (n == "ثبت نام") b.Click += (s, e) => new StudentsForm().Show();
                else b.Click += (s, e) => MessageBox.Show("این بخش به‌زودی ساخته می‌شود.", n);
                panel.Controls.Add(b);
            }
                     Controls.Add(panel);
            Controls.Add(lbl);
        }
    }

    public class StudentsForm
    {
        readonly TextBox tFirst = new TextBox(), tLast = new TextBox(), tFirstEn = new TextBox(), tLastEn = new TextBox(), tMobile = new TextBox();
        readonly TextBox tSearch = new TextBox();
        readonly DataGridView grid = new DataGridView();

        public StudentsForm()
        {
            Text = "ثبت‌نام زبان‌آموزان";
            Width = 1050; Height = 680;
            RightToLeft = RightToLeft.Yes;
            Font = new Font("Tahoma", 10);

            var top = new Panel { Dock = DockStyle.Top, Height = 130 };
            Action<TextBox, string, int, int> put = (tb, label, x, y) =>
            {
                var l = new Label { Text = label, AutoSize = true, Location = new Point(x, y + 3) };
                tb.Location = new Point(x + 95, y); tb.Width = 160;
                top.Controls.Add(l); top.Controls.Add(tb);
            };
            put(tFirst, "نام", 20, 15);
            put(tLast, "نام خانوادگی", 20, 55);
            put(tFirstEn, "First Name", 320, 15);
            put(tLastEn, "Last Name", 320, 55);
            put(tMobile, "موبایل", 640, 15);

            var btnSave = new Button { Text = "ذخیره", Location = new Point(640, 50), Width = 130, BackColor = Color.LightGreen };
            btnSave.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tFirst.Text) || string.IsNullOrWhiteSpace(tLast.Text)) { MessageBox.Show("نام و نام خانوادگی الزامی است."); return; }
                try
                {
                    await Api.SaveStudent(new StudentDto
                    {
                        FirstName = tFirst.Text,
                        LastName = tLast.Text,
                        FirstNameEn = tFirstEn.Text,
                        LastNameEn = tLastEn.Text,
                        Mobile = tMobile.Text
                    });
                    MessageBox.Show("ذخیره شد.");
                    tFirst.Text = tLast.Text = tFirstEn.Text = tLastEn.Text = tMobile.Text = "";
                    await Reload();
                }
                catch (Exception ex) { MessageBox.Show("خطا در ارتباط با سرور:\n" + ex.Message); }
            };
            top.Controls.Add(btnSave);

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
            var lSearch = new Label { Text = "جستجو:", AutoSize = true, Location = new Point(230, 10) };
            tSearch.Location = new Point(70, 7); tSearch.Width = 150;
            tSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await Reload(); };
            searchPanel.Controls.Add(lSearch); searchPanel.Controls.Add(tSearch);

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var id = (int)grid.Rows[e.RowIndex].Cells["Id"].Value;
                try
                {
                    var f = await Api.GetFinance(id);
                    if (f.Debtor) MessageBox.Show("بدهکار: " + f.Balance.ToString("#,0") + " تومان", "وضعیت مالی", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else MessageBox.Show("تسویه — مانده: ۰", "وضعیت مالی", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch { }
            };

            Controls.Add(grid);
            Controls.Add(searchPanel);
            Controls.Add(top);

            Load += async (s, e) => await Reload();
        }

        async Task Reload()
        {
            try
            {
                var list = await Api.GetStudents(tSearch.Text);
                grid.DataSource = list.Select(x => new
                {
                    Id = x.Id,
                    کد = x.StudentCode,
                    نام = x.FirstName,
                    خانوادگی = x.LastName,
                    First = x.FirstNameEn,
                    Last = x.LastNameEn,
                    موبایل = x.Mobile,
                    وضعیت = x.Status
                }).ToList();
                if (grid.Columns.Count > 0) grid.Columns["Id"].Visible = false;
            }
            catch (Exception ex) { MessageBox.Show("خطا در ارتباط با سرور:\n" + ex.Message); }
        }
    }
}
