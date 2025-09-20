using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DOAN
{
    public partial class ThanhToan : Form
    {
        string strSql = @"Data Source=Asus\SQLEXPRESS;Initial Catalog=DOAN_THUCUNG;Integrated Security=True";
        SqlConnection conn = null;
        private decimal phanTramGiamGia = 0;
        private bool isKhachHangMoi = false;

        public ThanhToan()
        {
            InitializeComponent();
        }

        private void ThanhToan_Load(object sender, EventArgs e)
        {
            // Vô hiệu hóa tất cả các control trước khi tải dữ liệu
            DisableAllControls();

            LoadKhachHangComboBox();
            LoadMaHoaDon();
            SetThoiGian();
            SetNhanVien();
        }

        private void DisableAllControls()
        {
            // DISABLE HOÀN TOÀN tất cả các control thông tin
            txtMaHoaDon.Enabled = false;
            txtTenKhachHang.Enabled = false;
            txtThoiGian.Enabled = false;
            txtNhanVien.Enabled = false;
            txtSoDienThoai.Enabled = false;
            txtDiaChi.Enabled = false;
            txtKhachHang.Enabled = false;
            txtKhuyenMai.Enabled = false;
            txtTongTien.Enabled = false;
            txtSoLuong.Enabled = false;

            // DISABLE HOÀN TOÀN DateTimePicker và CheckBox
            dtpNgaySinh.Enabled = false;
            cbNam.Enabled = false;
            cbNu.Enabled = false;

            // DISABLE HOÀN TOÀN DataGridView
            dgvThanhToan.Enabled = false;

            // CHỈ CHO PHÉP ComboBox được chọn
            cbbThanhVien.Enabled = true;

            // Đổi màu nền để thể hiện trạng thái disabled
            txtMaHoaDon.BackColor = System.Drawing.SystemColors.Control;
            txtTenKhachHang.BackColor = System.Drawing.SystemColors.Control;
            txtThoiGian.BackColor = System.Drawing.SystemColors.Control;
            txtNhanVien.BackColor = System.Drawing.SystemColors.Control;
            txtSoDienThoai.BackColor = System.Drawing.SystemColors.Control;
            txtDiaChi.BackColor = System.Drawing.SystemColors.Control;
            txtKhachHang.BackColor = System.Drawing.SystemColors.Control;
            txtKhuyenMai.BackColor = System.Drawing.SystemColors.Control;
            txtTongTien.BackColor = System.Drawing.SystemColors.Control;
            txtSoLuong.BackColor = System.Drawing.SystemColors.Control;
        }

        private void LoadThongTinThanhToan(decimal giamGiaPercent)
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_GetThongTinThanhToan", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@KhuyenMaiPercent", giamGiaPercent);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                conn.Open();
                adapter.Fill(dt);
                conn.Close();

                dgvThanhToan.DataSource = dt;

                // Định dạng cột tiền
                if (dgvThanhToan.Columns.Contains("Đơn Giá"))
                {
                    dgvThanhToan.Columns["Đơn Giá"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                if (dgvThanhToan.Columns.Contains("Thành Tiền"))
                {
                    dgvThanhToan.Columns["Thành Tiền"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }

                // Auto resize columns
                dgvThanhToan.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

                // Tính tổng tiền và tổng số lượng
                TinhTongTienVaSoLuong();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thông tin thanh toán: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void TinhTongTienVaSoLuong()
        {
            decimal tongTien = 0;
            int tongSoLuong = 0;

            foreach (DataGridViewRow row in dgvThanhToan.Rows)
            {
                // Tính tổng số lượng
                if (row.Cells["Số Lượng"].Value != null &&
                    int.TryParse(row.Cells["Số Lượng"].Value.ToString(), out int soLuong))
                {
                    tongSoLuong += soLuong;
                }

                // Tính tổng tiền (đã giảm giá)
                if (row.Cells["Thành Tiền"].Value != null)
                {
                    string thanhTienStr = row.Cells["Thành Tiền"].Value.ToString();
                    thanhTienStr = thanhTienStr.Replace(",", ""); // Xóa dấu phân cách
                    if (decimal.TryParse(thanhTienStr, out decimal thanhTien))
                    {
                        tongTien += thanhTien;
                    }
                }
            }

            // Hiển thị kết quả
            txtSoLuong.Text = tongSoLuong.ToString("N0");
            txtTongTien.Text = tongTien.ToString("N0") + " VNĐ";
        }

        private void LoadKhachHangComboBox()
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_GetAllKhachHang", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                conn.Open();
                adapter.Fill(dt);
                conn.Close();

                cbbThanhVien.DataSource = dt;
                cbbThanhVien.DisplayMember = "HoTen";
                cbbThanhVien.ValueMember = "MaKhachHang";

                // Thiết lập ComboBox không cho nhập text, chỉ chọn từ danh sách
                cbbThanhVien.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách khách hàng: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void LoadMaHoaDon()
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_GetNextMaHoaDon", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                object result = cmd.ExecuteScalar();
                conn.Close();

                txtMaHoaDon.Text = result.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải mã hóa đơn: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void SetThoiGian()
        {
            txtThoiGian.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private void SetNhanVien()
        {
            txtNhanVien.Text = "Trần Văn Công Toàn";
        }

        private decimal TinhPhanTramGiamGia(string khuyenMai)
        {
            // Phân tích chuỗi khuyến mãi để lấy phần trăm giảm giá
            if (string.IsNullOrEmpty(khuyenMai) || khuyenMai == "Không có")
                return 0;

            // Tìm số phần trăm trong chuỗi (ví dụ: "Giảm 7% tổng giá trị")
            var match = Regex.Match(khuyenMai, @"(\d+)%");
            if (match.Success)
            {
                return decimal.Parse(match.Groups[1].Value);
            }

            return 0;
        }

        private void cbbThanhVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbThanhVien.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)cbbThanhVien.SelectedItem;

                // Lấy thông tin khách hàng
                txtTenKhachHang.Text = selectedRow["HoTen"].ToString();
                txtSoDienThoai.Text = selectedRow["SoDienThoai"].ToString();
                txtDiaChi.Text = selectedRow["DiaChi"].ToString();
                txtKhachHang.Text = selectedRow["LoaiKhachHang"].ToString();

                // Xử lý khuyến mãi
                string khuyenMai = selectedRow["KhuyenMaiApDung"].ToString();
                phanTramGiamGia = TinhPhanTramGiamGia(khuyenMai);
                txtKhuyenMai.Text = khuyenMai;

                // Xử lý ngày sinh
                if (selectedRow["NgaySinh"] != DBNull.Value)
                {
                    dtpNgaySinh.Value = Convert.ToDateTime(selectedRow["NgaySinh"]);
                }

                // Xử lý giới tính
                string gioiTinh = selectedRow["GioiTinh"].ToString();
                if (gioiTinh.Equals("Nam", StringComparison.OrdinalIgnoreCase))
                {
                    cbNam.Checked = true;
                    cbNu.Checked = false;
                }
                else if (gioiTinh.Equals("Nữ", StringComparison.OrdinalIgnoreCase))
                {
                    cbNam.Checked = false;
                    cbNu.Checked = true;
                }
                else
                {
                    cbNam.Checked = false;
                    cbNu.Checked = false;
                }

                // Tải lại thông tin thanh toán với giảm giá
                LoadThongTinThanhToan(phanTramGiamGia);
            }
        }

        private void btnQuayLai_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnThanhToan_Click(object sender, EventArgs e)
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                conn.Open();

                int maKhachHang;

                // Xử lý khách hàng mới
                if (isKhachHangMoi)
                {
                    // Thêm khách hàng mới
                    SqlCommand cmdThemKH = new SqlCommand("sp_ThemKhachHangMoi", conn);
                    cmdThemKH.CommandType = CommandType.StoredProcedure;
                    cmdThemKH.Parameters.AddWithValue("@HoTen", txtTenKhachHang.Text);
                    cmdThemKH.Parameters.AddWithValue("@SoDienThoai", txtSoDienThoai.Text);
                    cmdThemKH.Parameters.AddWithValue("@DiaChi", txtDiaChi.Text);
                    cmdThemKH.Parameters.AddWithValue("@NgaySinh", dtpNgaySinh.Value);
                    cmdThemKH.Parameters.AddWithValue("@GioiTinh", cbNam.Checked ? "Nam" : "Nữ");

                    maKhachHang = Convert.ToInt32(cmdThemKH.ExecuteScalar());
                }
                else
                {
                    // Lấy mã khách hàng từ ComboBox
                    DataRowView selectedRow = (DataRowView)cbbThanhVien.SelectedItem;
                    maKhachHang = Convert.ToInt32(selectedRow["MaKhachHang"]);
                }

                // Lấy mã hóa đơn
                int maHoaDon = Convert.ToInt32(txtMaHoaDon.Text);

                // Lấy mã nhân viên (giả sử mã nhân viên cố định là 1)
                int maNhanVien = 1;

                // Lấy thông tin đơn hàng từ DONHANG_CHITIET
                SqlCommand cmdGetDonHang = new SqlCommand("sp_GetThongTinDonHang", conn);
                cmdGetDonHang.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(cmdGetDonHang);
                DataTable dtDonHang = new DataTable();
                adapter.Fill(dtDonHang);

                // Thêm từng sản phẩm vào LICHSUMUAHANG_THIETBI
                foreach (DataRow row in dtDonHang.Rows)
                {
                    int maThietBi = Convert.ToInt32(row["MaThietBi"]);
                    int soLuong = Convert.ToInt32(row["SoLuongDatMua"]);
                    decimal giaBan = Convert.ToDecimal(row["GiaBan"]);
                    decimal tongTien = soLuong * giaBan * (1 - phanTramGiamGia / 100);

                    SqlCommand cmdThemHD = new SqlCommand("sp_ThemHoaDonChiTiet", conn);
                    cmdThemHD.CommandType = CommandType.StoredProcedure;
                    cmdThemHD.Parameters.AddWithValue("@MaHoaDon", maHoaDon);
                    cmdThemHD.Parameters.AddWithValue("@MaThietBi", maThietBi);
                    cmdThemHD.Parameters.AddWithValue("@MaKhachHang", maKhachHang);
                    cmdThemHD.Parameters.AddWithValue("@MaNhanVien", maNhanVien);
                    cmdThemHD.Parameters.AddWithValue("@SoLuong", soLuong);
                    cmdThemHD.Parameters.AddWithValue("@GiaBan", giaBan);
                    cmdThemHD.Parameters.AddWithValue("@NgayThanhToan", DateTime.Today);
                    cmdThemHD.Parameters.AddWithValue("@KhuyenMaiApDung", txtKhuyenMai.Text);
                    cmdThemHD.Parameters.AddWithValue("@TongTien", tongTien);

                    cmdThemHD.ExecuteNonQuery();
                }

                // Xóa giỏ hàng sau khi thanh toán
                SqlCommand cmdXoaDonHang = new SqlCommand("sp_XoaTatCaDonHang", conn);
                cmdXoaDonHang.CommandType = CommandType.StoredProcedure;
                cmdXoaDonHang.ExecuteNonQuery();

                MessageBox.Show("Thanh toán thành công!", "Thông báo",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thanh toán: " + ex.Message, "Lỗi",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void btnThemThanhVien_Click(object sender, EventArgs e)
        {
            // Cho phép chỉnh sửa thông tin khách hàng
            isKhachHangMoi = true;
            txtTenKhachHang.Enabled = true;
            txtSoDienThoai.Enabled = true;
            txtDiaChi.Enabled = true;
            dtpNgaySinh.Enabled = true;
            cbNam.Enabled = true;
            cbNu.Enabled = true;

            // Xóa ComboBox và thiết lập giá trị mặc định
            cbbThanhVien.Enabled = false;
            cbbThanhVien.SelectedIndex = -1;

            // Thiết lập giá trị mặc định cho khách hàng mới
            txtTenKhachHang.Text = "";
            txtSoDienThoai.Text = "";
            txtDiaChi.Text = "";
            txtKhachHang.Text = "Khách lẻ";
            txtKhuyenMai.Text = "Không có";
            dtpNgaySinh.Value = DateTime.Now.AddYears(-20);
            cbNam.Checked = true;
            cbNu.Checked = false;

            // Đổi màu nền để thể hiện trạng thái có thể chỉnh sửa
            txtTenKhachHang.BackColor = System.Drawing.SystemColors.Window;
            txtSoDienThoai.BackColor = System.Drawing.SystemColors.Window;
            txtDiaChi.BackColor = System.Drawing.SystemColors.Window;
        }
    }
}