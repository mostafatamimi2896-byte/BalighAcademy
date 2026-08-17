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

    public class TermDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public bool IsCurrent { get; set; }
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
                public static async Task UpdateStudent(StudentDto s)
        {
            var body = new StringContent(JsonSerializer.Serialize(s), Encoding.UTF8, "application/json");
            await Program.Http.PutAsync(Program.ApiBase + $"/api/students/{s.Id}", body);
        }

        public static async Task<List<EnrollmentDto>> GetEnrollments(int id)
        {
            var json = await Program.Http.GetStringAsync(Program.ApiBase + $"/api/students/{id}/enrollments");
            return JsonSerializer.Deserialize<List<EnrollmentDto>>(json, O) ?? new List<EnrollmentDto>();
        }

        public static async Task<List<TermDto>> GetTerms()
        {
            var json = await Program.Http.GetStringAsync(Program.ApiBase + "/api/terms");
            return JsonSerializer.Deserialize<List<TermDto>>(json, O) ?? new List<TermDto>();
        }
    }

    public class ShineButton : Button
    {
        bool _hov;
        public ShineButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.SteelBlue;
            Font = new Font("Tahoma", 10, FontStyle.Bold);
            Cursor = Cursors.Hand;
            MouseEnter += (s, e) => { _hov = true; Invalidate(); };
            MouseLeave += (s, e) => { _hov = false; Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using (var path = RoundRect(rect, 10))
            {
                using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                    _hov ? Color.FromArgb(255, 255, 255) : Color.FromArgb(245, 250, 255),
                    _hov ? Color.FromArgb(170, 215, 250) : Color.FromArgb(215, 233, 250), 90))
                {
                    g.FillPath(br, path);
                }
                g.DrawPath(new Pen(_hov ? Color.DodgerBlue : Color.SteelBlue, _hov ? 2 : 1), path);
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(0, 0, Width, Height), ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        static System.Drawing.Drawing2D.GraphicsPath RoundRect(Rectangle r, int rad)
        {
            var p = new System.Drawing.Drawing2D.GraphicsPath();
            p.AddArc(r.X, r.Y, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Y, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.X, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    public class ClockPanel : Panel
    {
        readonly Timer _t = new Timer { Interval = 1000 };
        readonly Label _dig = new Label { Dock = DockStyle.Bottom, Height = 30, Font = new Font("Tahoma", 11, FontStyle.Bold), ForeColor = Color.SteelBlue, TextAlign = ContentAlignment.MiddleCenter };
        public ClockPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Controls.Add(_dig);
            _dig.Text = DateTime.Now.ToString("HH:mm:ss — yyyy/MM/dd");
            _t.Tick += (s, e) => { _dig.Text = DateTime.Now.ToString("HH:mm:ss — yyyy/MM/dd"); Invalidate(); };
            _t.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var availH = Height - 35;
            var r = Math.Min(Width, availH) - 12;
            var c = new PointF(Width / 2, availH / 2 + 4);
            var rect = new Rectangle((int)(c.X - r / 2), (int)(c.Y - r / 2), r, r);
            g.FillEllipse(Brushes.White, rect);
            g.DrawEllipse(new Pen(Color.SteelBlue, 2), rect);
            for (int i = 0; i < 12; i++)
            {
                var a = i * Math.PI / 6;
                g.DrawLine(Pens.SteelBlue,
                    c.X + (r / 2 - 8) * (float)Math.Sin(a), c.Y - (r / 2 - 8) * (float)Math.Cos(a),
                    c.X + (r / 2 - 3) * (float)Math.Sin(a), c.Y - (r / 2 - 3) * (float)Math.Cos(a));
            }
            var now = DateTime.Now;
            var ha = (now.Hour % 12 + now.Minute / 60.0) * Math.PI / 6;
            var ma = now.Minute * Math.PI / 30;
            var sa = now.Second * Math.PI / 30;
            g.DrawLine(new Pen(Color.Black, 3), c, new PointF(c.X + (r / 4) * (float)Math.Sin(ha), c.Y - (r / 4) * (float)Math.Cos(ha)));
            g.DrawLine(new Pen(Color.Black, 2), c, new PointF(c.X + (r / 2 - 12) * (float)Math.Sin(ma), c.Y - (r / 2 - 12) * (float)Math.Cos(ma)));
            g.DrawLine(new Pen(Color.Red, 1), c, new PointF(c.X + (r / 2 - 8) * (float)Math.Sin(sa), c.Y - (r / 2 - 8) * (float)Math.Cos(sa)));
        }
    }

    public class LogoPanel : Panel
    {
        Image _img;
        public LogoPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            Task.Run(async () =>
            {
                foreach (var u in new[] { "https://balighacademy.onrender.com/images/logo.png", "https://balighacademy.onrender.com/logo.png" })
                {
                    try
                    {
                        var s = await Program.Http.GetStreamAsync(u);
                        _img = Image.FromStream(s);
                        if (_img != null) break;
                    }
                    catch { }
                }
                try { Invoke(new Action(Invalidate)); } catch { }
            });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (_img != null)
            {
                var h = Math.Min(Height - 60, _img.Height * 2);
                var w = _img.Width * h / _img.Height;
                g.DrawImage(_img, (Width - w) / 2, 30, w, h);
            }
            else
            {
                var rw = Math.Min(Width / 2, 500);
                var rh = rw / 2;
                var rect = new Rectangle((Width - rw) / 2, 50, rw, rh);
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, Color.FromArgb(30, 144, 255), Color.FromArgb(0, 80, 160), 90))
                {
                    g.FillEllipse(brush, rect);
                }
                g.DrawString("Baligh", new Font("Tahoma", 44, FontStyle.Bold | FontStyle.Italic), Brushes.White, new PointF(Width / 2 - 100, 70));
            }
            g.DrawString("آموزشگاه زبان بلیغ", new Font("Tahoma", 16, FontStyle.Bold), Brushes.SteelBlue, new PointF(Width / 2 - 80, Height - 60));
        }
    }

    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "نرم‌افزار مدیریت آموزشگاه بلیغ (۲٫۲)";
            Width = 1100; Height = 700;
            RightToLeft = RightToLeft.Yes;
            Font = new Font("Tahoma", 10);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(235, 243, 254);

            var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.SteelBlue };
            var title = new Label
            {
                Text = "آموزشگاه زبان بلیغ",
                Font = new Font("Tahoma", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(title);

            var menu = new FlowLayoutPanel
            {
                Dock = DockStyle.Top, Height = 95,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.FromArgb(214, 230, 250),
                Padding = new Padding(10, 12, 10, 5)
            };
            string[] names = { "ثبت نام", "کارنامه‌ها", "اساتید", "حسابداری", "گزارشات", "مدیریت", "ابزارها" };
            foreach (var n in names)
            {
                var b = new ShineButton { Text = n, Width = 125, Height = 68, Margin = new Padding(4) };
                if (n == "ثبت نام") b.Click += (s, e) => new StudentsForm().Show();
                else if (n == "حسابداری") b.Click += (s, e) => new AccountingForm().Show();
                else if (n == "ابزارها") b.Click += (s, e) => new ToolsForm().Show();
                else b.Click += (s, e) => MessageBox.Show("این بخش به‌زودی ساخته می‌شود.", n);
                menu.Controls.Add(b);
            }

            var side = new Panel { Dock = DockStyle.Right, Width = 210, BackColor = Color.FromArgb(222, 235, 252), Padding = new Padding(10) };
            var clock = new ClockPanel { Dock = DockStyle.Bottom, Height = 220 };
            var sideTitle = new Label { Text = "دسترس سریع", Dock = DockStyle.Top, Height = 40, Font = new Font("Tahoma", 12, FontStyle.Bold), ForeColor = Color.SteelBlue, TextAlign = ContentAlignment.MiddleCenter };
            var sideFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            foreach (var n in new[] { "گزارشات کلاسی", "برنامه امتحانات", "صدور کارت", "صدور کارنامه" })
            {
                var sb = new ShineButton { Text = n, Width = 180, Height = 45, Margin = new Padding(5) };
                sb.Click += (s, e) => MessageBox.Show("به‌زودی.", n);
                sideFlow.Controls.Add(sb);
            }
            side.Controls.Add(sideFlow);
            side.Controls.Add(sideTitle);
            side.Controls.Add(clock);

            var status = new Label
            {
                Dock = DockStyle.Bottom, Height = 32,
                Text = "به آموزشگاه زبان بلیغ خوش آمدید — " + DateTime.Now.ToLongDateString(),
                ForeColor = Color.SteelBlue, BackColor = Color.FromArgb(214, 230, 250),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var logo = new LogoPanel { Dock = DockStyle.Fill };

            Controls.Add(logo);
            Controls.Add(side);
            Controls.Add(status);
            Controls.Add(menu);
            Controls.Add(header);
        }
    }

    public class AccountingForm : Form
    {
        public AccountingForm()
        {
            Text = "حسابداری";
            Width = 650; Height = 420;
            RightToLeft = RightToLeft.Yes;
            Font = new Font("Tahoma", 10);
            BackColor = Color.FromArgb(235, 243, 254);
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(20) };
            string[] names = { "امور شهریه (دریافت و پرداخت)", "لیست پرداخت ماهانه", "درآمد", "هزینه‌ها", "حقوق اساتید", "حقوق کارکنان" };
            foreach (var n in names)
            {
                var b = new ShineButton { Text = n, Width = 180, Height = 60, Margin = new Padding(6) };
                if (n.StartsWith("امور شهریه")) b.Click += (s, e) => new FinanceForm().Show();
                else b.Click += (s, e) => MessageBox.Show("به‌زودی.", n);
                flow.Controls.Add(b);
            }
            Controls.Add(flow);
        }
    }

    public class ToolsForm : Form
    {
        public ToolsForm()
        {
            Text = "ابزارها";
            Width = 600; Height = 400;
            RightToLeft = RightToLeft.Yes;
            Font = new Font("Tahoma", 10);
            BackColor = Color.FromArgb(235, 243, 254);
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(20) };
            foreach (var n in new[] { "پشتیبان‌گیری", "بازیابی اطلاعات", "ثبت برنامه", "درباره ما" })
            {
                var b = new ShineButton { Text = n, Width = 160, Height = 55, Margin = new Padding(6) };
                b.Click += (s, e) => MessageBox.Show("به‌زودی.", n);
                flow.Controls.Add(b);
            }
            Controls.Add(flow);
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

            var btnSave = new ShineButton { Text = "ذخیره", Location = new Point(640, 50), Width = 130, Height = 35 };
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
            var btnFind = new ShineButton { Text = "یافتن", Location = new Point(460, 13), Width = 90, Height = 30 };
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

            var btnSave = new ShineButton { Text = "ثبت سند", Location = new Point(20, 50), Width = 120, Height = 35 };
            btnSave.Click += async (s, e) => await SavePay();
            var btnPrint = new ShineButton { Text = "چاپ رسید", Location = new Point(20, 95), Width = 120, Height = 35 };
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
