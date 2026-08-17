using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
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

        public static async Task<List<PaymentDto>> GetPayments(int id)
        {
            var json = await Program.Http.GetStringAsync(Program.ApiBase + $"/api/students/{id}/payments");
            return JsonSerializer.Deserialize<List<PaymentDto>>(json, O) ?? new List<PaymentDto>();
        }

        public static async Task CreatePayment(int id, PaymentDto p)
        {
            var body = new StringContent(JsonSerializer.Serialize(p), Encoding.UTF8, "application/json");
            await Program.Http.PostAsync(Program.ApiBase + $"/api/students/{id}/payments", body);
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
                else if (n == "امور شهریه") b.Click += (s, e) => new FinanceForm().Show();
                else b.Click += (s, e) => MessageBox.Show("این بخش به‌زودی ساخته می‌شود.", n);
                panel.Controls.Add(b);
            }
            Controls.Add(panel);
            Controls.Add(lbl);
        }
    }

    public class StudentsForm : Form
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

    public class FinanceForm : Form
    {
        readonly TextBox tCode = new TextBox(), tAmount = new TextBox(), tBank = new TextBox(), tDate = new TextBox(), tReceipt = new TextBox();
        readonly ComboBox cbKind = new ComboBox();
        readonly Label lblVoucher = new Label(), lblBalance = new Label(), lblName = new Label();
        readonly DataGridView grid = new DataGridView();
        StudentDto _current;
        List<PaymentDto> _pays = new List<PaymentDto>();

        public FinanceForm()
        {
            Text = "امور شهریه - دریافت و پرداخت";
            Width = 1000; Height = 700;
            RightToLeft = RightToLeft.Yes;
            Font = new Font("Tahoma", 10);

            var top = new Panel { Dock = DockStyle.Top, Height = 160 };

            var lCode = new Label { Text = "کد یا نام:", AutoSize = true, Location = new Point(700, 18) };
            tCode.Location = new Point(560, 15); tCode.Width = 130;
            var btnFind = new Button { Text = "یافتن", Location = new Point(460, 13), Width = 90 };
            btnFind.Click += async (s, e) => await FindStudent();

            lblName.Text = "نام: -"; lblName.AutoSize = true; lblName.Location = new Point(250, 18);
            lblBalance.Text = "وضعیت مالی: -"; lblBalance.AutoSize = true; lblBalance.Location = new Point(20, 18);
            lblBalance.Font = new Font("Tahoma", 10, FontStyle.Bold);

            var lKind = new Label { Text = "نوع:", AutoSize = true, Location = new Point(700, 55) };
            cbKind.Location = new Point(520, 52); cbKind.Width = 170;
            cbKind.DropDownStyle = ComboBoxStyle.DropDownList;
            cbKind.Items.AddRange(new object[] { "شهریه / Tuition", "پرداخت / Payment", "تخفیف / Discount" });
            cbKind.SelectedIndex = 0;

            var lAmount = new Label { Text = "مبلغ (تومان):", AutoSize = true, Location = new Point(700, 90) };
            tAmount.Location = new Point(520, 87); tAmount.Width = 170;

            var lBank = new Label { Text = "بانک:", AutoSize = true, Location = new Point(330, 55) };
            tBank.Location = new Point(180, 52); tBank.Width = 130;

            var lDate = new Label { Text = "تاریخ:", AutoSize = true, Location = new Point(330, 90) };
            tDate.Location = new Point(180, 87); tDate.Width = 130;

            var lReceipt = new Label { Text = "شماره رسید (دستی):", AutoSize = true, Location = new Point(700, 125) };
            tReceipt.Location = new Point(520, 122); tReceipt.Width = 170;

            var lVoucher = new Label { Text = "شماره سند (خودکار):", AutoSize = true, Location = new Point(330, 125) };
            lblVoucher.Text = "-"; lblVoucher.AutoSize = true; lblVoucher.Location = new Point(180, 127);
            lblVoucher.Font = new Font("Tahoma", 10, FontStyle.Bold);

            var btnSave = new Button { Text = "ثبت سند", Location = new Point(20, 50), Width = 120, BackColor = Color.LightGreen };
            btnSave.Click += async (s, e) => await SavePay();
            var btnPrint = new Button { Text = "چاپ رسید", Location = new Point(20, 90), Width = 120 };
            btnPrint.Click += (s, e) => PrintReceipt();

            top.Controls.AddRange(new Control[] { lCode, tCode, btnFind, lblName, lblBalance, lKind, cbKind, lAmount, tAmount, lBank, tBank, lDate, tDate, lReceipt, tReceipt, lVoucher, lblVoucher, btnSave, btnPrint });

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            Controls.Add(grid);
            Controls.Add(top);
        }

        async Task FindStudent()
        {
            try
            {
                var list = await Api.GetStudents(tCode.Text);
                if (list.Count == 0) { MessageBox.Show("پیدا نشد."); return; }
                _current = list[0];
                lblName.Text = "نام: " + _current.FirstName + " " + _current.LastName + " (" + _current.StudentCode + ")";
                await RefreshBalance();
                await RefreshList();
            }
            catch (Exception ex) { MessageBox.Show("خطا: " + ex.Message); }
        }

        async Task RefreshBalance()
        {
            var f = await Api.GetFinance(_current.Id);
            if (f.Debtor) { lblBalance.Text = "بدهکار: " + f.Balance.ToString("#,0") + " تومان"; lblBalance.ForeColor = Color.Red; }
            else { lblBalance.Text = "تسویه ✔"; lblBalance.ForeColor = Color.Green; }
        }

        async Task RefreshList()
        {
            _pays = await Api.GetPayments(_current.Id);
            grid.DataSource = _pays.Select(p => new
            {
                سند = p.VoucherNo,
                رسید = p.ReceiptNo,
                تاریخ = p.Date,
                نوع = p.Kind,
                مبلغ = p.Amount,
                بانک = p.Bank
            }).ToList();
            if (_pays.Count > 0) lblVoucher.Text = _pays.Max(p => p.VoucherNo).ToString();
        }

        async Task SavePay()
        {
            if (_current == null) { MessageBox.Show("ابتدا زبان‌آموز را پیدا کن."); return; }
            long amount;
            if (!long.TryParse(tAmount.Text, out amount)) { MessageBox.Show("مبلغ معتبر نیست."); return; }
            await Api.CreatePayment(_current.Id, new PaymentDto
            {
                Date = tDate.Text,
                Amount = amount,
                Kind = cbKind.SelectedItem.ToString(),
                Bank = tBank.Text,
                ReceiptNo = tReceipt.Text
            });
            tAmount.Text = tBank.Text = tReceipt.Text = "";
            await RefreshList();
            await RefreshBalance();
        }

        void PrintReceipt()
        {
            if (_current == null || grid.CurrentRow == null) { MessageBox.Show("یک ردیف از جدول را انتخاب کن."); return; }
            var p = _pays[grid.CurrentRow.Index];
            var pd = new PrintDocument();
            pd.PrintPage += (s, e) =>
            {
                var g = e.Graphics;
                var f1 = new Font("Tahoma", 16, FontStyle.Bold);
                var f2 = new Font("Tahoma", 11);
                g.DrawString("آموزشگاه زبان بلیغ", f1, Brushes.Black, 280, 40);
                g.DrawString("رسید پرداخت", f2, Brushes.Black, 330, 75);
                g.DrawString("شماره سند: " + p.VoucherNo, f2, Brushes.Black, 80, 130);
                g.DrawString("شماره رسید: " + p.ReceiptNo, f2, Brushes.Black, 400, 130);
                g.DrawString("تاریخ: " + p.Date, f2, Brushes.Black, 80, 160);
                g.DrawString("نام: " + _current.FirstName + " " + _current.LastName, f2, Brushes.Black, 400, 160);
                g.DrawString("مبلغ: " + p.Amount.ToString("#,0") + " تومان", f2, Brushes.Black, 80, 190);
                g.DrawString("بابت: " + p.Kind, f2, Brushes.Black, 400, 190);
                g.DrawString("مهر و امضای آموزشگاه", f2, Brushes.Black, 420, 280);
            };
            var dlg = new PrintDialog { Document = pd };
            if (dlg.ShowDialog() == DialogResult.OK) pd.Print();
        }
    }
}
