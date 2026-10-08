using System.Drawing;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;

namespace WinFormsApp5;

// ================= ENTITY =================
public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public double Grade { get; set; }
}

// ================= DBCONTEXT =================
public class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=LAPTOP-6R5UJ1GK\SQLEXPRESS;Database=QuanLySinhVienDb;Trusted_Connection=True;TrustServerCertificate=True;");
    }
}

// ================= FORM (giao diện tạo bằng code) =================
public class MainForm : Form
{
    private readonly Label lblTieuDe = new();
    private readonly Label lblHoTen = new();
    private readonly TextBox txtHoTen = new();
    private readonly Label lblDiem = new();
    private readonly TextBox txtDiem = new();
    private readonly Button btnThem = new();
    private readonly Button btnSua = new();
    private readonly Button btnXoa = new();
    private readonly Button btnTaiLai = new();
    private readonly Button btnLocDat = new();
    private readonly DataGridView dgvSinhVien = new();
    private readonly Label lblTrangThai = new();

    public MainForm()
    {
        Text = "Quản lý sinh viên - EF Core CRUD";
        ClientSize = new Size(650, 440);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        lblTieuDe.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTieuDe.SetBounds(12, 9, 626, 35);
        lblTieuDe.Text = "QUẢN LÝ SINH VIÊN - EF CORE (CRUD ĐẦY ĐỦ)";
        lblTieuDe.TextAlign = ContentAlignment.MiddleCenter;

        lblHoTen.Text = "Họ tên:";
        lblHoTen.SetBounds(20, 62, 55, 20);
        txtHoTen.SetBounds(80, 59, 260, 23);

        lblDiem.Text = "Điểm:";
        lblDiem.SetBounds(370, 62, 45, 20);
        txtDiem.SetBounds(420, 59, 80, 23);

        CauHinhNut(btnThem, "➕ Thêm", 20, 100, 100);
        CauHinhNut(btnSua, "✏ Sửa", 130, 100, 100);
        CauHinhNut(btnXoa, "🗑 Xóa", 240, 100, 100);
        CauHinhNut(btnTaiLai, "Tải lại danh sách", 350, 100, 130);
        CauHinhNut(btnLocDat, "Lọc SV đạt (LINQ)", 490, 100, 140);

        dgvSinhVien.SetBounds(20, 145, 610, 250);
        dgvSinhVien.AllowUserToAddRows = false;
        dgvSinhVien.AllowUserToDeleteRows = false;
        dgvSinhVien.ReadOnly = true;
        dgvSinhVien.MultiSelect = false;
        dgvSinhVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvSinhVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        lblTrangThai.Text = "Sẵn sàng.";
        lblTrangThai.SetBounds(20, 408, 610, 20);

        Controls.AddRange(new Control[]
        {
            lblTieuDe, lblHoTen, txtHoTen, lblDiem, txtDiem,
            btnThem, btnSua, btnXoa, btnTaiLai, btnLocDat,
            dgvSinhVien, lblTrangThai
        });

        Load += MainForm_Load;
        btnThem.Click += btnThem_Click;
        btnSua.Click += btnSua_Click;
        btnXoa.Click += btnXoa_Click;
        btnTaiLai.Click += async (s, e) => await TaiDanhSach();
        btnLocDat.Click += btnLocDat_Click;
        dgvSinhVien.SelectionChanged += dgvSinhVien_SelectionChanged;
    }

    private static void CauHinhNut(Button b, string text, int x, int y, int w)
    {
        b.Text = text;
        b.SetBounds(x, y, w, 32);
        b.UseVisualStyleBackColor = true;
    }

    private void HienThi(string msg, bool loi = false)
    {
        lblTrangThai.Text = msg;
        lblTrangThai.ForeColor = loi ? Color.Red : Color.DarkGreen;
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        try
        {
            // Tự tạo database + bảng nếu chưa có (không cần Migration)
            using var context = new AppDbContext();
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            HienThi("Lỗi kết nối CSDL: " + ex.Message, true);
            return;
        }
        await TaiDanhSach();
    }

    // ===== READ =====
    private async Task TaiDanhSach()
    {
        try
        {
            using var context = new AppDbContext();
            List<Student> danhSach = await context.Students.ToListAsync();
            dgvSinhVien.DataSource = null;
            dgvSinhVien.DataSource = danhSach;
            HienThi($"Đã tải {danhSach.Count} sinh viên.");
        }
        catch (Exception ex) { HienThi("Lỗi: " + ex.Message, true); }
    }

    private void dgvSinhVien_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvSinhVien.CurrentRow?.DataBoundItem is Student sv)
        {
            txtHoTen.Text = sv.FullName;
            txtDiem.Text = sv.Grade.ToString();
        }
    }

    // ===== CREATE =====
    private async void btnThem_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtHoTen.Text))
        {
            MessageBox.Show("Vui lòng nhập họ tên!");
            return;
        }
        if (!double.TryParse(txtDiem.Text, out double diem) || diem < 0 || diem > 10)
        {
            MessageBox.Show("Điểm không hợp lệ!");
            return;
        }
        try
        {
            using (var context = new AppDbContext())
            {
                context.Students.Add(new Student { FullName = txtHoTen.Text.Trim(), Grade = diem });
                await context.SaveChangesAsync();
            }
            txtHoTen.Clear();
            txtDiem.Clear();
            await TaiDanhSach();
            HienThi("Đã thêm sinh viên mới!");
        }
        catch (Exception ex) { HienThi("Lỗi: " + ex.Message, true); }
    }

    // ===== UPDATE =====
    private async void btnSua_Click(object? sender, EventArgs e)
    {
        if (dgvSinhVien.CurrentRow?.DataBoundItem is not Student svDangChon)
        {
            MessageBox.Show("Vui lòng chọn một dòng để sửa!");
            return;
        }
        if (string.IsNullOrWhiteSpace(txtHoTen.Text))
        {
            MessageBox.Show("Vui lòng nhập họ tên!");
            return;
        }
        if (!double.TryParse(txtDiem.Text, out double diemMoi) || diemMoi < 0 || diemMoi > 10)
        {
            MessageBox.Show("Điểm không hợp lệ!");
            return;
        }
        try
        {
            using (var context = new AppDbContext())
            {
                Student? sv = await context.Students.FindAsync(svDangChon.Id);
                if (sv != null)
                {
                    sv.FullName = txtHoTen.Text.Trim();
                    sv.Grade = diemMoi;
                    await context.SaveChangesAsync();
                }
            }
            await TaiDanhSach();
            HienThi("Đã cập nhật thông tin!");
        }
        catch (Exception ex) { HienThi("Lỗi: " + ex.Message, true); }
    }

    // ===== DELETE =====
    private async void btnXoa_Click(object? sender, EventArgs e)
    {
        if (dgvSinhVien.CurrentRow?.DataBoundItem is not Student svDangChon)
        {
            MessageBox.Show("Vui lòng chọn một dòng để xóa!");
            return;
        }
        DialogResult ketQua = MessageBox.Show(
            $"Bạn có chắc muốn xóa \"{svDangChon.FullName}\"?",
            "Xác nhận xóa",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (ketQua != DialogResult.Yes) return;

        try
        {
            using (var context = new AppDbContext())
            {
                Student? sv = await context.Students.FindAsync(svDangChon.Id);
                if (sv != null)
                {
                    context.Students.Remove(sv);
                    await context.SaveChangesAsync();
                }
            }
            txtHoTen.Clear();
            txtDiem.Clear();
            await TaiDanhSach();
            HienThi("Đã xóa sinh viên!");
        }
        catch (Exception ex) { HienThi("Lỗi: " + ex.Message, true); }
    }

    // ===== LINQ to Entities =====
    private async void btnLocDat_Click(object? sender, EventArgs e)
    {
        try
        {
            using var context = new AppDbContext();
            List<Student> ketQua = await context.Students
                .Where(sv => sv.Grade >= 5)
                .OrderByDescending(sv => sv.Grade)
                .ToListAsync();
            dgvSinhVien.DataSource = null;
            dgvSinhVien.DataSource = ketQua;
            HienThi($"Tìm thấy {ketQua.Count} sinh viên đạt (>=5).");
        }
        catch (Exception ex) { HienThi("Lỗi: " + ex.Message, true); }
    }
}

// ================= MAIN =================
static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
