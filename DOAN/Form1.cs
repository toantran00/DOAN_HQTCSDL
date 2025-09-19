using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace DOAN
{
    public partial class Form1 : Form
    {
        string strSql = @"Data Source=Asus\SQLEXPRESS;Initial Catalog=DOAN_THUCUNG;Integrated Security=True";
        SqlConnection conn = null;

        // Biến trạng thái
        private bool isAdding = false;
        private bool isEditing = false;
        private bool isEditingDonHang = false; // Thêm biến trạng thái sửa đơn hàng
        private int selectedRowIndex = -1; // Theo dõi hàng được chọn
        private int selectedDonHangIndex = -1; // Theo dõi đơn hàng được chọn
        private int soLuongCu = 0;
        private int soLuongTonKhoCu = 0; // Thêm biến để lưu số lượng tồn kho hiện tại



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetAllPHUKIEN();
            SetupDataGridView();
            SetControlsEnabled(false); // Mặc định disable các control nhập liệu
            UpdateTongSoLuongMua(); // Thêm dòng này để load tổng số lượng khi khởi động
            GetAllDonHangChiTiet(); 

        }
        private void SetMuaControlsEnabled(bool editing)
        {
            // QUAN TRỌNG: Chỉ enable nút Sửa khi có selection VÀ không đang edit
            btnMuaSua.Enabled = !editing && selectedDonHangIndex >= 0;
            btnMuaLuu.Enabled = editing;
            btnMuaHuy.Enabled = editing;

            // QUAN TRỌNG: nudSoLuongMua LUÔN LUÔN ENABLE để có thể nhập số lượng
            nudSoLuongMua.Enabled = true;

            dgvBuy.Enabled = !editing;

            // Disable các nút quản lý sản phẩm khi đang sửa đơn hàng
            btnThem.Enabled = !editing;
            btnSua.Enabled = !editing && selectedRowIndex >= 0;
            btnXoa.Enabled = !editing && selectedRowIndex >= 0;

            // QUAN TRỌNG: Khóa nút đặt hàng khi đang sửa
            btnDatHang.Enabled = !editing;

            // Khóa các control chọn sản phẩm khi đang sửa
            dgvDSPHUKIEN.Enabled = !editing;
        }

        private void SetupDataGridView()
        {
            // Đảm bảo DataGridView có thể chọn hàng
            dgvDSPHUKIEN.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDSPHUKIEN.MultiSelect = false;
        }

        // Phương thức enable/disable các control nhập liệu
        private void SetControlsEnabled(bool enabled)
        {
            txtTenThietBi.Enabled = enabled;
            cbbLoaiThietBi.Enabled = enabled;
            cbbTrangThai.Enabled = enabled;
            dtpNgayBaoTri.Enabled = enabled;
            txtViTriSuDung.Enabled = enabled;
            nudSoLuong.Enabled = enabled;
            txtGhiChu.Enabled = enabled;
            txtGiaBan.Enabled = enabled;
            cbbMucDichSuDung.Enabled = enabled;

            // Các nút chức năng
            btnLuu.Enabled = enabled;
            btnHuy.Enabled = enabled;

            // Các nút Thêm, Sửa, Xóa chỉ enable khi không ở chế độ chỉnh sửa
            btnThem.Enabled = !enabled;
            btnSua.Enabled = !enabled && selectedRowIndex >= 0;
            btnXoa.Enabled = !enabled && selectedRowIndex >= 0;

            // Enable/disable DataGridView tùy thuộc vào trạng thái
            dgvDSPHUKIEN.Enabled = !enabled;
        }

        public void GetAllPHUKIEN()
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }
                SqlCommand cmd = new SqlCommand("sp_GetAllThietBi", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                conn.Open();
                adapter.Fill(dt);
                conn.Close();

                dgvDSPHUKIEN.DataSource = dt;

                // Xóa lựa chọn sau khi tải lại dữ liệu
                dgvDSPHUKIEN.ClearSelection();
                selectedRowIndex = -1;

                // Cập nhật trạng thái các nút
                SetControlsEnabled(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void dgvDSPHUKIEN_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && !isAdding && !isEditing)
            {
                selectedRowIndex = e.RowIndex;
                DataGridViewRow row = dgvDSPHUKIEN.Rows[e.RowIndex];

                // Điền thông tin vào các ô thông tin phụ kiện
                txtMaThietBi.Text = row.Cells["MaThietBi"].Value.ToString();
                txtTenThietBi.Text = row.Cells["TenThietBi"].Value.ToString();
                cbbLoaiThietBi.Text = row.Cells["LoaiThietBi"].Value.ToString();
                cbbTrangThai.Text = row.Cells["TrangThai"].Value.ToString();

                if (row.Cells["NgayBaoTri"].Value != DBNull.Value)
                {
                    dtpNgayBaoTri.Value = Convert.ToDateTime(row.Cells["NgayBaoTri"].Value);
                }

                txtViTriSuDung.Text = row.Cells["ViTriSuDung"].Value.ToString();

                if (row.Cells["SoLuongHienCo"].Value != DBNull.Value)
                {
                    int soLuongConLai = Convert.ToInt32(row.Cells["SoLuongHienCo"].Value);
                    txtMaxSoLuongConLai.Text = soLuongConLai.ToString();

                    // Đặt giới hạn tối đa cho NumericUpDown
                    nudSoLuongMua.Maximum = soLuongConLai;

                    // Đảm bảo giá trị hiện tại không vượt quá maximum mới
                    if (nudSoLuongMua.Value > nudSoLuongMua.Maximum)
                    {
                        nudSoLuongMua.Value = nudSoLuongMua.Maximum;
                    }
                }
                else
                {
                    txtMaxSoLuongConLai.Text = "0";
                    nudSoLuongMua.Maximum = 0;

                    // Đảm bảo giá trị hiện tại không vượt quá maximum mới
                    if (nudSoLuongMua.Value > nudSoLuongMua.Maximum)
                    {
                        nudSoLuongMua.Value = nudSoLuongMua.Maximum;
                    }
                }

                txtGhiChu.Text = row.Cells["GhiChu"].Value?.ToString() ?? "";

                if (row.Cells["GiaBan"].Value != DBNull.Value)
                {
                    txtGiaBan.Text = row.Cells["GiaBan"].Value.ToString();
                }

                cbbMucDichSuDung.Text = row.Cells["MucDichSuDung"].Value?.ToString() ?? "";

                // Reset trạng thái
                isAdding = false;
                isEditing = false;
                txtMaThietBi.Enabled = false;

                // Cập nhật trạng thái các nút
                SetControlsEnabled(false);
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            // Lưu trạng thái hiện tại (nếu có hàng được chọn)
            int previouslySelectedIndex = selectedRowIndex;

            // Xóa các ô nhập liệu để chuẩn bị thêm mới
            ClearInputFields();

            // Đặt trạng thái là đang thêm mới
            isAdding = true;
            isEditing = false;

            // Tự động tạo mã mới và không cho sửa
            int nextId = GetNextDeviceId();
            txtMaThietBi.Text = nextId.ToString();
            txtMaThietBi.Enabled = false;

            // Bỏ chọn hàng trong DataGridView
            dgvDSPHUKIEN.ClearSelection();
            selectedRowIndex = -1;

            // Enable các control nhập liệu
            SetControlsEnabled(true);

            // Lưu lại chỉ số hàng được chọn trước đó để có thể khôi phục nếu hủy
            // (có thể sử dụng biến class để lưu trữ nếu cần)
        }

        private int GetNextDeviceId()
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_GetNextDeviceId", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                int nextId = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();

                return nextId;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lấy mã thiết bị: " + ex.Message);
                return 1;
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (selectedRowIndex < 0 || string.IsNullOrEmpty(txtMaThietBi.Text))
            {
                MessageBox.Show("Vui lòng chọn một thiết bị để sửa!");
                return;
            }

            isEditing = true;
            isAdding = false;
            txtMaThietBi.Enabled = false;

            // Enable các control nhập liệu
            SetControlsEnabled(true);

            // Hiển thị thông báo khi vào chế độ sửa
            MessageBox.Show("Đã vào chế độ sửa. Hãy thay đổi thông tin và nhấn Lưu!", "Thông báo",
                             MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có đang trong chế độ thêm hoặc sửa không
            if (!isAdding && !isEditing)
            {
                MessageBox.Show("Vui lòng chọn chức năng Thêm hoặc Sửa trước khi Lưu!");
                return;
            }

            try
            {
                SqlCommand cmd;

                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                conn.Open();

                // Insert hoặc Update dữ liệu tùy vào trạng thái
                if (isAdding) // Thêm mới
                {
                    cmd = new SqlCommand("InsertPhuKien", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Truyền tham số cho stored procedure
                    cmd.Parameters.AddWithValue("@TenThietBi", txtTenThietBi.Text);
                    cmd.Parameters.AddWithValue("@LoaiThietBi", cbbLoaiThietBi.Text);
                    cmd.Parameters.AddWithValue("@TrangThai", cbbTrangThai.Text);
                    cmd.Parameters.AddWithValue("@NgayBaoTri", dtpNgayBaoTri.Value);
                    cmd.Parameters.AddWithValue("@ViTriSuDung", txtViTriSuDung.Text);
                    cmd.Parameters.AddWithValue("@SoLuongHienCo", nudSoLuong.Value);
                    cmd.Parameters.AddWithValue("@GhiChu", txtGhiChu.Text);
                    cmd.Parameters.AddWithValue("@GiaBan", Convert.ToDecimal(txtGiaBan.Text));
                    cmd.Parameters.AddWithValue("@MucDichSuDung", cbbMucDichSuDung.Text);
                }
                else // Sửa dữ liệu
                {
                    cmd = new SqlCommand("UpdatePhuKien", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Truyền tham số cho stored procedure
                    cmd.Parameters.AddWithValue("@MaThietBi", Convert.ToInt32(txtMaThietBi.Text));
                    cmd.Parameters.AddWithValue("@TenThietBi", txtTenThietBi.Text);
                    cmd.Parameters.AddWithValue("@LoaiThietBi", cbbLoaiThietBi.Text);
                    cmd.Parameters.AddWithValue("@TrangThai", cbbTrangThai.Text);
                    cmd.Parameters.AddWithValue("@NgayBaoTri", dtpNgayBaoTri.Value);
                    cmd.Parameters.AddWithValue("@ViTriSuDung", txtViTriSuDung.Text);
                    cmd.Parameters.AddWithValue("@SoLuongHienCo", nudSoLuong.Value);
                    cmd.Parameters.AddWithValue("@GhiChu", txtGhiChu.Text);
                    cmd.Parameters.AddWithValue("@GiaBan", Convert.ToDecimal(txtGiaBan.Text));
                    cmd.Parameters.AddWithValue("@MucDichSuDung", cbbMucDichSuDung.Text);
                }

                // Thực thi stored procedure
                cmd.ExecuteNonQuery();
                conn.Close();

                // Làm mới danh sách và các nút chức năng
                ClearInputFields();
                isAdding = false;
                isEditing = false;

                // Disable các control nhập liệu
                SetControlsEnabled(false);

                MessageBox.Show("Dữ liệu đã được lưu thành công!");
                GetAllPHUKIEN();
            }
            catch (SqlException sqlEx)
            {
                // Bắt lỗi từ SQL Server hoặc trigger - thông báo lỗi sẽ hiển thị từ trigger
                MessageBox.Show(sqlEx.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Nhập đúng dữ liệu số: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private void ClearInputFields()
        {
            txtMaThietBi.Text = "";
            txtTenThietBi.Text = "";
            cbbLoaiThietBi.SelectedIndex = -1;
            cbbTrangThai.SelectedIndex = -1;
            dtpNgayBaoTri.Value = DateTime.Now;
            txtViTriSuDung.Text = "";
            nudSoLuong.Value = 0;
            txtGhiChu.Text = "";
            txtGiaBan.Text = "";
            cbbMucDichSuDung.SelectedIndex = -1;
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            // Hủy bỏ thao tác hiện tại
            isAdding = false;
            isEditing = false;

            // Khôi phục trạng thái ban đầu của txtMaThietBi
            txtMaThietBi.Enabled = false;

            // Nếu có hàng được chọn trước đó, hiển thị lại thông tin của hàng đó
            if (selectedRowIndex >= 0 && selectedRowIndex < dgvDSPHUKIEN.Rows.Count)
            {
                // Chọn lại hàng trong DataGridView
                dgvDSPHUKIEN.Rows[selectedRowIndex].Selected = true;

                // Hiển thị thông tin của hàng được chọn
                DataGridViewRow row = dgvDSPHUKIEN.Rows[selectedRowIndex];

                txtMaThietBi.Text = row.Cells["MaThietBi"].Value.ToString();
                txtTenThietBi.Text = row.Cells["TenThietBi"].Value.ToString();
                cbbLoaiThietBi.Text = row.Cells["LoaiThietBi"].Value.ToString();
                cbbTrangThai.Text = row.Cells["TrangThai"].Value.ToString();

                if (row.Cells["NgayBaoTri"].Value != DBNull.Value)
                {
                    dtpNgayBaoTri.Value = Convert.ToDateTime(row.Cells["NgayBaoTri"].Value);
                }

                txtViTriSuDung.Text = row.Cells["ViTriSuDung"].Value.ToString();

                if (row.Cells["SoLuongHienCo"].Value != DBNull.Value)
                {
                    nudSoLuong.Value = Convert.ToInt32(row.Cells["SoLuongHienCo"].Value);
                }

                txtGhiChu.Text = row.Cells["GhiChu"].Value?.ToString() ?? "";

                if (row.Cells["GiaBan"].Value != DBNull.Value)
                {
                    txtGiaBan.Text = row.Cells["GiaBan"].Value.ToString();
                }

                cbbMucDichSuDung.Text = row.Cells["MucDichSuDung"].Value?.ToString() ?? "";
            }
            else
            {
                // Nếu không có hàng nào được chọn, xóa các ô nhập liệu
                ClearInputFields();
            }

            // Disable các control nhập liệu (chỉ được xem)
            SetControlsEnabled(false);

            // Đảm bảo DataGridView có thể chọn lại
            dgvDSPHUKIEN.Enabled = true;
            dgvDSPHUKIEN.Focus();
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string searchText = txtSearchName.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Vui lòng nhập tên thiết bị cần tìm kiếm!");
                txtSearchName.Focus();
                return;
            }

            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_SearchThietBiTheoTen", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@TenThietBi", searchText);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                conn.Open();
                adapter.Fill(dt);
                conn.Close();

                dgvDSPHUKIEN.DataSource = dt;

                // Hiển thị thông báo nếu không tìm thấy
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show($"Không tìm thấy thiết bị nào có tên chứa '{searchText}'");
                }
                else
                {
                    MessageBox.Show($"Tìm thấy {dt.Rows.Count} thiết bị phù hợp");
                }

                // Xóa lựa chọn sau khi tìm kiếm
                dgvDSPHUKIEN.ClearSelection();
                selectedRowIndex = -1;
                ClearInputFields();
                SetControlsEnabled(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (selectedRowIndex < 0 || string.IsNullOrEmpty(txtMaThietBi.Text))
            {
                MessageBox.Show("Vui lòng chọn một thiết bị để xóa!");
                return;
            }

            // Hiển thị hộp thoại xác nhận
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xóa thiết bị này?",
                                                 "Xác nhận xóa",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    if (conn == null)
                    {
                        conn = new SqlConnection(strSql);
                    }

                    SqlCommand cmd = new SqlCommand("DeleteThietBi", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaThietBi", Convert.ToInt32(txtMaThietBi.Text));

                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    conn.Close();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Xóa thiết bị thành công!");

                        // Làm mới danh sách
                        ClearInputFields();
                        GetAllPHUKIEN();

                        // Cập nhật trạng thái các nút
                        SetControlsEnabled(false);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy thiết bị để xóa!");
                    }
                }
                catch (SqlException sqlEx)
                {
                    // Bắt lỗi từ SQL (ví dụ: khóa ngoại)
                    if (sqlEx.Number == 547) // Lỗi khóa ngoại
                    {
                        MessageBox.Show("Không thể xóa thiết bị vì đang được sử dụng trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi SQL: {sqlEx.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa thiết bị: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (conn != null && conn.State == ConnectionState.Open)
                        conn.Close();
                }
            }
        }

        // ========== FILTER FUNCTIONALITY ==========

        private void HandleLoaiThietBiCheckboxes(CheckBox selectedCheckbox)
        {
            cbThucAn.Checked = (selectedCheckbox == cbThucAn);
            cbTuiDung.Checked = (selectedCheckbox == cbTuiDung);
            cbXich.Checked = (selectedCheckbox == cbXich);
            cbQuanAo.Checked = (selectedCheckbox == cbQuanAo);
        }

        private void HandleMucGiaCheckboxes(CheckBox selectedCheckbox)
        {
            cbDuoi200.Checked = (selectedCheckbox == cbDuoi200);
            cb200to400.Checked = (selectedCheckbox == cb200to400);
            cbTren400000.Checked = (selectedCheckbox == cbTren400000);
        }

        private void HandleDoiTuongCheckboxes(CheckBox selectedCheckbox)
        {
            cbCho.Checked = (selectedCheckbox == cbCho);
            cbMeo.Checked = (selectedCheckbox == cbMeo);
        }

        // LoaiThietBi group
        private void cbThucAn_CheckedChanged(object sender, EventArgs e)
        {
            if (cbThucAn.Checked) HandleLoaiThietBiCheckboxes(cbThucAn);
        }

        private void cbTuiDung_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTuiDung.Checked) HandleLoaiThietBiCheckboxes(cbTuiDung);
        }

        private void cbXich_CheckedChanged(object sender, EventArgs e)
        {
            if (cbXich.Checked) HandleLoaiThietBiCheckboxes(cbXich);
        }

        private void cbQuanAo_CheckedChanged(object sender, EventArgs e)
        {
            if (cbQuanAo.Checked) HandleLoaiThietBiCheckboxes(cbQuanAo);
        }

        // MucGia group
        private void cbDuoi200_CheckedChanged(object sender, EventArgs e)
        {
            if (cbDuoi200.Checked) HandleMucGiaCheckboxes(cbDuoi200);
        }

        private void cb200to400_CheckedChanged(object sender, EventArgs e)
        {
            if (cb200to400.Checked) HandleMucGiaCheckboxes(cb200to400);
        }

        private void cbTren400000_CheckedChanged(object sender, EventArgs e)
        {
            if (cbTren400000.Checked) HandleMucGiaCheckboxes(cbTren400000);
        }

        // DoiTuong group
        private void cbCho_CheckedChanged(object sender, EventArgs e)
        {
            if (cbCho.Checked) HandleDoiTuongCheckboxes(cbCho);
        }

        private void cbMeo_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMeo.Checked) HandleDoiTuongCheckboxes(cbMeo);
        }

        // Trả về chuỗi phân cách bằng dấu phẩy thay vì List<string>
        private string GetSelectedLoaiThietBi()
        {
            var selected = new List<string>();
            if (cbThucAn.Checked) selected.Add("Thức ăn");
            if (cbTuiDung.Checked) selected.Add("Túi đựng");
            if (cbXich.Checked) selected.Add("Xích");
            if (cbQuanAo.Checked) selected.Add("Quần áo");
            return selected.Count > 0 ? string.Join(",", selected) : null;
        }

        private string GetSelectedMucGia()
        {
            var selected = new List<string>();
            if (cbDuoi200.Checked) selected.Add("Dưới 200.000");
            if (cb200to400.Checked) selected.Add("200.000-400.000");
            if (cbTren400000.Checked) selected.Add("Trên 400.000");
            return selected.Count > 0 ? string.Join(",", selected) : null;
        }

        private string GetSelectedDoiTuong()
        {
            var selected = new List<string>();
            if (cbCho.Checked) selected.Add("Chó");
            if (cbMeo.Checked) selected.Add("Mèo");
            return selected.Count > 0 ? string.Join(",", selected) : null;
        }

        private void btnLoc_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected filter values as comma-separated strings
                string loaiThietBi = GetSelectedLoaiThietBi();
                string mucGia = GetSelectedMucGia();
                string doiTuong = GetSelectedDoiTuong();

                // If no filters are selected, show all data
                if (string.IsNullOrEmpty(loaiThietBi) &&
                    string.IsNullOrEmpty(mucGia) &&
                    string.IsNullOrEmpty(doiTuong))
                {
                    GetAllPHUKIEN();
                    MessageBox.Show("Hiển thị tất cả thiết bị (không có bộ lọc nào được chọn)");
                    return;
                }

                // Execute the filter function với logic OR
                FilterThietBi(loaiThietBi, mucGia, doiTuong);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lọc dữ liệu: " + ex.Message);
            }
        }

        private void FilterThietBi(string loaiThietBi, string mucGia, string doiTuong)
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                // Xây dựng câu lệnh SQL để gọi hàm lọc
                string query = @"
            SELECT * 
            FROM dbo.fn_FilterThietBi(
                @LoaiThietBiList, 
                @MucGiaList, 
                @DoiTuongList
            )
            ORDER BY TenThietBi";

                SqlCommand cmd = new SqlCommand(query, conn);

                // Truyền tham số vào câu lệnh SQL
                cmd.Parameters.AddWithValue("@LoaiThietBiList", string.IsNullOrEmpty(loaiThietBi) ? (object)DBNull.Value : loaiThietBi);
                cmd.Parameters.AddWithValue("@MucGiaList", string.IsNullOrEmpty(mucGia) ? (object)DBNull.Value : mucGia);
                cmd.Parameters.AddWithValue("@DoiTuongList", string.IsNullOrEmpty(doiTuong) ? (object)DBNull.Value : doiTuong);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                conn.Open();
                adapter.Fill(dt);
                conn.Close();

                dgvDSPHUKIEN.DataSource = dt;

                // Hiển thị thông báo kết quả lọc
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Không tìm thấy thiết bị nào phù hợp với bộ lọc đã chọn");
                }
                else
                {
                    // Tạo thông báo hiển thị các bộ lọc đã chọn
                    string filters = "";
                    if (!string.IsNullOrEmpty(loaiThietBi)) filters += "Loại: " + loaiThietBi.Replace(",", ", ") + "\n";
                    if (!string.IsNullOrEmpty(mucGia)) filters += "Mức giá: " + mucGia.Replace(",", ", ") + "\n";
                    if (!string.IsNullOrEmpty(doiTuong)) filters += "Đối tượng: " + doiTuong.Replace(",", ", ") + "\n";

                    MessageBox.Show($"Tìm thấy {dt.Rows.Count} thiết bị phù hợp với:\n{filters}");
                }

                // Clear selection and input fields
                dgvDSPHUKIEN.ClearSelection();
                selectedRowIndex = -1;
                ClearInputFields();
                SetControlsEnabled(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lọc thiết bị: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void btnClearFilters_Click(object sender, EventArgs e)
        {
            // Clear all checkboxes
            cbThucAn.Checked = false;
            cbTuiDung.Checked = false;
            cbXich.Checked = false;
            cbQuanAo.Checked = false;

            cbDuoi200.Checked = false;
            cb200to400.Checked = false;
            cbTren400000.Checked = false;

            cbCho.Checked = false;
            cbMeo.Checked = false;

            // Reload all data
            GetAllPHUKIEN();
            MessageBox.Show("Đã xóa tất cả bộ lọc", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void GetAllDonHangChiTiet()
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_GetAllDonHangChiTiet", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                conn.Open();
                adapter.Fill(dt);
                conn.Close();

                dgvBuy.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách đơn hàng: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }
        private void btnDatHang_Click(object sender, EventArgs e)
        {
            // Kiểm tra nếu đang sửa đơn hàng thì không cho đặt hàng mới
            if (isEditingDonHang)
            {
                MessageBox.Show("Đang chỉnh sửa đơn hàng. Vui lòng hoàn tất hoặc hủy bỏ trước khi đặt hàng mới!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedRowIndex < 0 || string.IsNullOrEmpty(txtMaThietBi.Text))
            {
                MessageBox.Show("Vui lòng chọn một thiết bị để đặt mua!");
                return;
            }

            if (nudSoLuongMua.Value <= 0)
            {
                MessageBox.Show("Số lượng mua phải lớn hơn 0!");
                return;
            }

            try
            {
                int maThietBi = Convert.ToInt32(txtMaThietBi.Text);
                int soLuongMua = (int)nudSoLuongMua.Value;
                decimal giaBan = Convert.ToDecimal(txtGiaBan.Text);

                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_ThemDonHangChiTiet", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaThietBi", maThietBi);
                cmd.Parameters.AddWithValue("@SoLuongDatMua", soLuongMua);
                cmd.Parameters.AddWithValue("@GiaBan", giaBan);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                // Chỉ thông báo thành công đơn giản
                MessageBox.Show("Đặt hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Cập nhật lại danh sách đơn hàng
                GetAllDonHangChiTiet();

                // Cập nhật lại danh sách thiết bị (số lượng đã thay đổi)
                GetAllPHUKIEN();

                // Cập nhật tổng số lượng mua
                UpdateTongSoLuongMua();

                // Reset số lượng mua về 1
                nudSoLuongMua.Value = 1;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000) // Lỗi từ RAISERROR
                {
                    MessageBox.Show(sqlEx.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Lỗi SQL: {sqlEx.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đặt hàng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }
        // Thêm phương thức mới để cập nhật tổng số lượng mua
        private void UpdateTongSoLuongMua()
        {
            try
            {
                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_GetTongSoLuongMua", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();
                int tongSoLuong = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();

                // Gán tổng số lượng vào txtTongSoLuongMua
                txtTongSoLuongMua.Text = tongSoLuong.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lấy tổng số lượng: " + ex.Message);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // Các phương thức không sử dụng (giữ lại để tránh lỗi)
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void checkBox1_CheckedChanged(object sender, EventArgs e) { }
        private void checkBox10_CheckedChanged(object sender, EventArgs e) { }
        private void btnHuy_Click_1(object sender, EventArgs e) { }
        private void checkBox7_CheckedChanged(object sender, EventArgs e) { }
        private void checkBox2_CheckedChanged(object sender, EventArgs e) { }

        private void dgvDSPHUKIEN_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvBuy_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && !isEditingDonHang)
            {
                selectedDonHangIndex = e.RowIndex;
                DataGridViewRow row = dgvBuy.Rows[e.RowIndex];

                // Hiển thị thông tin đơn hàng được chọn
                txtMaThietBi.Text = row.Cells["Mã TB"].Value.ToString();
                txtTenThietBi.Text = row.Cells["Tên Thiết Bị"].Value.ToString();

                // Lấy số lượng từ DataGridView
                string soLuongStr = row.Cells["Số Lượng"].Value.ToString();
                if (int.TryParse(soLuongStr, out int soLuong))
                {
                    // ĐẢM BẢO giá trị nằm trong phạm vi cho phép
                    if (soLuong >= nudSoLuongMua.Minimum && soLuong <= nudSoLuongMua.Maximum)
                    {
                        nudSoLuongMua.Value = soLuong;
                    }
                    else
                    {
                        // Nếu vượt quá phạm vi, đặt giá trị bằng maximum hoặc minimum
                        nudSoLuongMua.Value = soLuong > nudSoLuongMua.Maximum ? nudSoLuongMua.Maximum : nudSoLuongMua.Minimum;
                    }
                    soLuongCu = soLuong;

                    // Lấy số lượng tồn kho hiện tại từ dgvDSPHUKIEN
                    // Tìm hàng tương ứng trong danh sách thiết bị dựa trên mã thiết bị
                    string maThietBi = row.Cells["Mã TB"].Value.ToString();
                    foreach (DataGridViewRow thietBiRow in dgvDSPHUKIEN.Rows)
                    {
                        if (thietBiRow.Cells["MaThietBi"].Value.ToString() == maThietBi)
                        {
                            if (thietBiRow.Cells["SoLuongHienCo"].Value != DBNull.Value)
                            {
                                soLuongTonKhoCu = Convert.ToInt32(thietBiRow.Cells["SoLuongHienCo"].Value);
                                txtMaxSoLuongConLai.Text = soLuongTonKhoCu.ToString();

                                // CẬP NHẬT LẠI phạm vi cho NumericUpDown ngay tại đây
                                nudSoLuongMua.Maximum = soLuongTonKhoCu + soLuongCu;
                                break;
                            }
                        }
                    }
                }

                // Enable nút Sửa
                SetMuaControlsEnabled(false);
            }
        }

        private void btnMuaSua_Click(object sender, EventArgs e)
        {
            if (selectedDonHangIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn một đơn hàng để sửa!");
                return;
            }

            isEditingDonHang = true;
            SetMuaControlsEnabled(true);
            int tongSoLuongCoTheDat = soLuongCu + soLuongTonKhoCu;
            txtMaxSoLuongConLai.Text = tongSoLuongCoTheDat.ToString();

            // Cập nhật giới hạn tối đa cho NumericUpDown và đảm bảo giá trị hiện tại nằm trong phạm vi
            nudSoLuongMua.Maximum = tongSoLuongCoTheDat;

            // Đảm bảo giá trị hiện tại không vượt quá maximum mới
            if (nudSoLuongMua.Value > nudSoLuongMua.Maximum)
            {
                nudSoLuongMua.Value = nudSoLuongMua.Maximum;
            }
        }

        private void btnMuaLuu_Click(object sender, EventArgs e)
        {
            if (selectedDonHangIndex < 0 || string.IsNullOrEmpty(txtMaThietBi.Text))
            {
                MessageBox.Show("Vui lòng chọn một đơn hàng để lưu!");
                return;
            }

            if (nudSoLuongMua.Value <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0!");
                return;
            }

            try
            {
                int maThietBi = Convert.ToInt32(txtMaThietBi.Text);
                int soLuongMoi = (int)nudSoLuongMua.Value;

                if (conn == null)
                {
                    conn = new SqlConnection(strSql);
                }

                SqlCommand cmd = new SqlCommand("sp_SuaSoLuongDonHang", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MaThietBi", maThietBi);
                cmd.Parameters.AddWithValue("@SoLuongMoi", soLuongMoi);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Cập nhật số lượng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Cập nhật lại tất cả danh sách
                GetAllDonHangChiTiet();
                GetAllPHUKIEN();
                UpdateTongSoLuongMua();

                // Reset trạng thái và XÓA SELECTION
                isEditingDonHang = false;
                selectedDonHangIndex = -1; // QUAN TRỌNG: Reset selection
                dgvBuy.ClearSelection(); // QUAN TRỌNG: Xóa selection trong DataGridView
                SetMuaControlsEnabled(false);

                // Reset các control về trạng thái mặc định
                txtMaThietBi.Text = "";
                txtTenThietBi.Text = "";
                nudSoLuongMua.Value = 1;
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 50000)
                {
                    MessageBox.Show(sqlEx.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Lỗi SQL: {sqlEx.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private void btnMuaHuy_Click(object sender, EventArgs e)
        {
            // Hủy bỏ thao tác sửa
            isEditingDonHang = false;
            selectedDonHangIndex = -1; // QUAN TRỌNG: Reset selection
            dgvBuy.ClearSelection(); // QUAN TRỌNG: Xóa selection trong DataGridView

            SetMuaControlsEnabled(false);

            // Reset các control về trạng thái mặc định
            txtMaThietBi.Text = "";
            txtTenThietBi.Text = "";
            nudSoLuongMua.Value = 1;
        }

        private void dgvBuy_SelectionChanged(object sender, EventArgs e)
        {

            // Nếu không có hàng nào được chọn và đang không ở chế độ sửa
            if (dgvBuy.SelectedRows.Count == 0 && !isEditingDonHang)
            {
                selectedDonHangIndex = -1;
                SetMuaControlsEnabled(false);

                // Reset các control
                txtMaThietBi.Text = "";
                txtTenThietBi.Text = "";
                nudSoLuongMua.Value = 1;
            }
        }
    }
}