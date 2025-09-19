using System;
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
        private int selectedRowIndex = -1; // Theo dõi hàng được chọn

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetAllPHUKIEN();
            SetupDataGridView();
            SetControlsEnabled(false); // Mặc định disable các control nhập liệu
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
                    nudSoLuong.Value = Convert.ToInt32(row.Cells["SoLuongHienCo"].Value);
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

        private void btnLoc_Click(object sender, EventArgs e)
        {
            // Giữ lại hàm nhưng không làm gì
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
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

        private void btnHuy_Click_1(object sender, EventArgs e)
        {
            
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cbThucAn_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}