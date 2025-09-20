-- Procedure lấy tất cả thiết bị
CREATE OR ALTER PROCEDURE sp_GetAllThietBi
AS
BEGIN
    SELECT 
        [MaThietBi],
        [TenThietBi],
        [LoaiThietBi],
        [TrangThai],
        [NgayBaoTri],
        [ViTriSuDung],
        [SoLuongHienCo],
        [GhiChu],
        [GiaBan],
        [MucDichSuDung]
    FROM [dbo].[THIETBI]
    ORDER BY [MaThietBi]
END
GO

-- Procedure lấy ID tiếp theo
CREATE OR ALTER PROCEDURE sp_GetNextDeviceId
AS
BEGIN
    SELECT ISNULL(MAX(MaThietBi), 0) + 1 FROM THIETBI
END
GO

-- Procedure thêm phụ kiện
CREATE OR ALTER PROCEDURE InsertPhuKien
    @TenThietBi NVARCHAR(100),
    @LoaiThietBi NVARCHAR(50),
    @TrangThai NVARCHAR(50),
    @NgayBaoTri DATE,
    @ViTriSuDung NVARCHAR(100),
    @SoLuongHienCo INT,
    @GhiChu NVARCHAR(255),
    @GiaBan DECIMAL(18, 2),
    @MucDichSuDung NVARCHAR(50)
AS
BEGIN
    INSERT INTO [THIETBI] 
    (TenThietBi, LoaiThietBi, TrangThai, NgayBaoTri, ViTriSuDung, SoLuongHienCo, GhiChu, GiaBan, MucDichSuDung)
    VALUES 
    (@TenThietBi, @LoaiThietBi, @TrangThai, @NgayBaoTri, @ViTriSuDung, @SoLuongHienCo, @GhiChu, @GiaBan, @MucDichSuDung);
END
GO

-- Procedure cập nhật phụ kiện
CREATE OR ALTER PROCEDURE UpdatePhuKien
    @MaThietBi INT,
    @TenThietBi NVARCHAR(100),
    @LoaiThietBi NVARCHAR(50),
    @TrangThai NVARCHAR(50),
    @NgayBaoTri DATE,
    @ViTriSuDung NVARCHAR(100),
    @SoLuongHienCo INT,
    @GhiChu NVARCHAR(255),
    @GiaBan DECIMAL(18, 2),
    @MucDichSuDung NVARCHAR(50)
AS
BEGIN
    UPDATE [THIETBI]
    SET 
        TenThietBi = @TenThietBi,
        LoaiThietBi = @LoaiThietBi,
        TrangThai = @TrangThai,
        NgayBaoTri = @NgayBaoTri,
        ViTriSuDung = @ViTriSuDung,
        SoLuongHienCo = @SoLuongHienCo,
        GhiChu = @GhiChu,
        GiaBan = @GiaBan,
        MucDichSuDung = @MucDichSuDung
    WHERE MaThietBi = @MaThietBi;
END
GO

-- Trigger cho INSERT
CREATE OR ALTER TRIGGER InsertDeviceTrigger
ON [THIETBI]
INSTEAD OF INSERT
AS
BEGIN
    -- Kiểm tra nếu có bất kỳ bản ghi nào có số lượng âm
    IF EXISTS (SELECT 1 FROM inserted WHERE SoLuongHienCo < 0)
    BEGIN
        RAISERROR('Số lượng thiết bị không được âm. Vui lòng nhập lại!', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu có bất kỳ bản ghi nào có giá âm
    IF EXISTS (SELECT 1 FROM inserted WHERE GiaBan < 0)
    BEGIN
        RAISERROR('Giá bán không được âm. Vui lòng nhập lại!', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu có bất kỳ bản ghi nào có số lượng là NULL hoặc không phải là số
    IF EXISTS (SELECT 1 FROM inserted WHERE SoLuongHienCo IS NULL OR TRY_CAST(SoLuongHienCo AS INT) IS NULL)
    BEGIN
        RAISERROR('Số lượng phải là một số hợp lệ và không được để trống!', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu có bất kỳ bản ghi nào có giá bán là NULL hoặc không phải là số
    IF EXISTS (SELECT 1 FROM inserted WHERE GiaBan IS NULL OR TRY_CAST(GiaBan AS DECIMAL(18, 2)) IS NULL)
    BEGIN
        RAISERROR('Giá bán phải là một số hợp lệ và không được để trống!', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu bất kỳ trường nào bị thiếu dữ liệu (NULL)
    IF EXISTS (SELECT 1 FROM inserted WHERE 
               TenThietBi IS NULL OR 
               LoaiThietBi IS NULL OR 
               TrangThai IS NULL OR 
               ViTriSuDung IS NULL OR 
               NgayBaoTri IS NULL OR 
               SoLuongHienCo IS NULL OR 
               GiaBan IS NULL OR 
               MucDichSuDung IS NULL)
    BEGIN
        RAISERROR('Tất cả các trường thông tin thiết bị phải được nhập đầy đủ!', 16, 1);
        RETURN;
    END

    -- Nếu tất cả đều hợp lệ, thực hiện INSERT
    INSERT INTO [THIETBI] (TenThietBi, LoaiThietBi, TrangThai, NgayBaoTri, 
                          ViTriSuDung, SoLuongHienCo, GhiChu, GiaBan, MucDichSuDung)
    SELECT TenThietBi, LoaiThietBi, TrangThai, NgayBaoTri, 
           ViTriSuDung, SoLuongHienCo, GhiChu, GiaBan, MucDichSuDung
    FROM inserted;
END;
GO

-- Trigger kiểm tra khi cập nhật thiết bị
CREATE OR ALTER TRIGGER UpdateDeviceTrigger
ON [THIETBI]
INSTEAD OF UPDATE
AS
BEGIN
    -- Kiểm tra nếu có bất kỳ bản ghi nào có số lượng âm
    IF EXISTS (SELECT 1 FROM inserted WHERE SoLuongHienCo < 0)
    BEGIN
        RAISERROR('Số lượng thiết bị không được âm. Vui lòng nhập lại!', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu có bất kỳ bản ghi nào có giá âm
    IF EXISTS (SELECT 1 FROM inserted WHERE GiaBan < 0)
    BEGIN
        RAISERROR('Giá bán không được âm. Vui lòng nhập lại!', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu có bất kỳ bản ghi nào có số lượng là NULL hoặc không phải là số
    IF EXISTS (SELECT 1 FROM inserted WHERE SoLuongHienCo IS NULL OR TRY_CAST(SoLuongHienCo AS INT) IS NULL)
    BEGIN
        RAISERROR('Số lượng phải là một số hợp lệ và không được để trống!', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu có bất kỳ bản ghi nào có giá bán là NULL hoặc không phải là số
    IF EXISTS (SELECT 1 FROM inserted WHERE GiaBan IS NULL OR TRY_CAST(GiaBan AS DECIMAL(18, 2)) IS NULL)
    BEGIN
        RAISERROR('Giá bán phải là một số hợp lệ và không được để trống!', 16, 1);
        RETURN;
    END

    -- Kiểm tra nếu bất kỳ trường nào bị thiếu dữ liệu (NULL)
    IF EXISTS (SELECT 1 FROM inserted WHERE 
               TenThietBi IS NULL OR 
               LoaiThietBi IS NULL OR 
               TrangThai IS NULL OR 
               ViTriSuDung IS NULL OR 
               NgayBaoTri IS NULL OR 
               SoLuongHienCo IS NULL OR 
               GiaBan IS NULL OR 
               MucDichSuDung IS NULL)
    BEGIN
        RAISERROR('Tất cả các trường thông tin thiết bị phải được nhập đầy đủ!', 16, 1);
        RETURN;
    END

    -- Nếu tất cả đều hợp lệ, thực hiện UPDATE
    UPDATE t
    SET t.TenThietBi = i.TenThietBi,
        t.LoaiThietBi = i.LoaiThietBi,
        t.TrangThai = i.TrangThai,
        t.NgayBaoTri = i.NgayBaoTri,
        t.ViTriSuDung = i.ViTriSuDung,
        t.SoLuongHienCo = i.SoLuongHienCo,
        t.GhiChu = i.GhiChu,
        t.GiaBan = i.GiaBan,
        t.MucDichSuDung = i.MucDichSuDung
    FROM [THIETBI] t
    INNER JOIN inserted i ON t.MaThietBi = i.MaThietBi;
END;
GO
-- Procedure xóa thiết bị
CREATE OR ALTER PROCEDURE DeleteThietBi
    @MaThietBi INT
AS
BEGIN
    DELETE FROM [THIETBI] WHERE MaThietBi = @MaThietBi;
END;
GO
-- Procedure tìm kiếm thiết bị theo tên
CREATE OR ALTER PROCEDURE sp_SearchThietBiTheoTen
    @TenThietBi NVARCHAR(100)
AS
BEGIN
    SELECT 
        [MaThietBi],
        [TenThietBi],
        [LoaiThietBi],
        [TrangThai],
        [NgayBaoTri],
        [ViTriSuDung],
        [SoLuongHienCo],
        [GhiChu],
        [GiaBan],
        [MucDichSuDung]
    FROM [dbo].[THIETBI]
    WHERE [TenThietBi] LIKE '%' + @TenThietBi + '%'
    ORDER BY [MaThietBi]
END
GO
CREATE OR ALTER FUNCTION fn_FilterThietBi(
    @LoaiThietBiList NVARCHAR(MAX) = NULL,
    @MucGiaList NVARCHAR(MAX) = NULL,
    @DoiTuongList NVARCHAR(MAX) = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT * 
    FROM ThietBi 
    WHERE 
        -- Điều kiện LoaiThietBi
        (@LoaiThietBiList IS NULL OR 
         LoaiThietBi IN (SELECT value FROM STRING_SPLIT(@LoaiThietBiList, ',')))
        
        -- Điều kiện MucGia: Sửa lại để nhóm các điều kiện con
        AND (
            @MucGiaList IS NULL 
            OR (
                (GiaBan < 200000 AND 'Dưới 200.000' IN (SELECT value FROM STRING_SPLIT(@MucGiaList, ',')))
                OR (GiaBan BETWEEN 200000 AND 400000 AND '200.000-400.000' IN (SELECT value FROM STRING_SPLIT(@MucGiaList, ',')))
                OR (GiaBan > 400000 AND 'Trên 400.000' IN (SELECT value FROM STRING_SPLIT(@MucGiaList, ',')))
            )
        )
        
        -- Điều kiện DoiTuong: Tìm kiếm trong TenThietBi và GhiChu
        AND (
            @DoiTuongList IS NULL 
            OR EXISTS (
                SELECT 1 
                FROM STRING_SPLIT(@DoiTuongList, ',') 
                WHERE TenThietBi LIKE '%' + value + '%' OR GhiChu LIKE '%' + value + '%'
            )
        )
)
CREATE OR ALTER PROCEDURE sp_ThemDonHangChiTiet
    @MaThietBi INT,
    @SoLuongDatMua INT,
    @GiaBan DECIMAL(18, 0)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Kiểm tra thiết bị có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM ThietBi WHERE MaThietBi = @MaThietBi)
        BEGIN
            RAISERROR('Thiết bị không tồn tại.', 16, 1);
            RETURN;
        END
        
        -- Kiểm tra số lượng tồn kho
        DECLARE @SoLuongHienCo INT;
        SELECT @SoLuongHienCo = SoLuongHienCo FROM ThietBi WHERE MaThietBi = @MaThietBi;
        
        IF @SoLuongHienCo < @SoLuongDatMua
        BEGIN
            RAISERROR('Số lượng trong kho không đủ. Chỉ còn %d sản phẩm.', 16, 1, @SoLuongHienCo);
            RETURN;
        END
        
        -- Kiểm tra số lượng đặt mua phải lớn hơn 0
        IF @SoLuongDatMua <= 0
        BEGIN
            RAISERROR('Số lượng đặt mua phải lớn hơn 0.', 16, 1);
            RETURN;
        END
        
        -- Kiểm tra xem đã có đơn hàng nào cho thiết bị này chưa
        IF EXISTS (SELECT 1 FROM DONHANG_CHITIET WHERE MaThietBi = @MaThietBi)
        BEGIN
            -- Nếu đã có, cập nhật số lượng và giá
            UPDATE DONHANG_CHITIET 
            SET SoLuongDatMua = SoLuongDatMua + @SoLuongDatMua,
                GiaBan = @GiaBan, -- Cập nhật giá mới nhất
                NgayDat = GETDATE() -- Cập nhật ngày đặt mới nhất
            WHERE MaThietBi = @MaThietBi;
        END
        ELSE
        BEGIN
            -- Nếu chưa có, thêm mới
            INSERT INTO DONHANG_CHITIET (MaThietBi, SoLuongDatMua, GiaBan, NgayDat)
            VALUES (@MaThietBi, @SoLuongDatMua, @GiaBan, GETDATE());
        END
        
        -- Cập nhật số lượng tồn kho
        UPDATE ThietBi 
        SET SoLuongHienCo = SoLuongHienCo - @SoLuongDatMua
        WHERE MaThietBi = @MaThietBi;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO




CREATE or alter PROCEDURE sp_GetAllDonHangChiTiet
AS
BEGIN
    SELECT 
        tb.MaThietBi AS [Mã TB],
        tb.TenThietBi AS [Tên Thiết Bị],
        dh.SoLuongDatMua AS [Số Lượng],
        FORMAT(dh.GiaBan, 'N0') as [Giá Bán],
        dh.NgayDat AS [Ngày Đặt]  -- Hiển thị Ngày Đặt thay vì Thành Tiền
    FROM DONHANG_CHITIET dh
    INNER JOIN ThietBi tb ON dh.MaThietBi = tb.MaThietBi
    ORDER BY dh.NgayDat DESC, dh.MaDonHang DESC;
END
GO
CREATE OR ALTER PROCEDURE sp_GetTongSoLuongMua
AS
BEGIN
    SELECT ISNULL(SUM(SoLuongDatMua), 0) AS TongSoLuongMua
    FROM DONHANG_CHITIET;
END
GO
CREATE OR ALTER PROCEDURE sp_SuaSoLuongDonHang
    @MaThietBi INT,
    @SoLuongMoi INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Kiểm tra đơn hàng có tồn tại không
        IF NOT EXISTS (SELECT 1 FROM DONHANG_CHITIET WHERE MaThietBi = @MaThietBi)
        BEGIN
            RAISERROR('Đơn hàng không tồn tại.', 16, 1);
            RETURN;
        END
        
        -- Kiểm tra số lượng mới phải lớn hơn 0
        IF @SoLuongMoi <= 0
        BEGIN
            RAISERROR('Số lượng mới phải lớn hơn 0.', 16, 1);
            RETURN;
        END
        
        -- Lấy số lượng cũ và số lượng tồn kho hiện tại
        DECLARE @SoLuongCu INT, @SoLuongHienCo INT;
        
        SELECT @SoLuongCu = SoLuongDatMua 
        FROM DONHANG_CHITIET 
        WHERE MaThietBi = @MaThietBi;
        
        SELECT @SoLuongHienCo = SoLuongHienCo 
        FROM ThietBi 
        WHERE MaThietBi = @MaThietBi;
        
        -- Tính toán chênh lệch số lượng
        DECLARE @ChenhLech INT = @SoLuongMoi - @SoLuongCu;
        
        -- Kiểm tra nếu số lượng tồn kho đủ để tăng
        IF @ChenhLech > 0 AND @SoLuongHienCo < @ChenhLech
        BEGIN
            RAISERROR('Số lượng trong kho không đủ để tăng. Chỉ còn %d sản phẩm.', 16, 1, @SoLuongHienCo);
            RETURN;
        END
        
        -- Cập nhật số lượng trong đơn hàng
        UPDATE DONHANG_CHITIET 
        SET SoLuongDatMua = @SoLuongMoi,
            NgayDat = GETDATE() -- Cập nhật ngày đặt mới
        WHERE MaThietBi = @MaThietBi;
        
        -- Cập nhật số lượng tồn kho
        UPDATE ThietBi 
        SET SoLuongHienCo = SoLuongHienCo - @ChenhLech
        WHERE MaThietBi = @MaThietBi;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
CREATE OR ALTER PROCEDURE sp_GetThongTinThanhToan
AS
BEGIN
    SELECT 
        tb.MaThietBi AS [Mã Thiết Bị],
        tb.TenThietBi AS [Tên Thiết Bị],
        dh.SoLuongDatMua AS [Số Lượng],
        FORMAT(dh.GiaBan, 'N0') AS [Đơn Giá],
        FORMAT((dh.SoLuongDatMua * dh.GiaBan), 'N0') AS [Thành Tiền],
        dh.NgayDat AS [Ngày Đặt]
    FROM DONHANG_CHITIET dh
    INNER JOIN ThietBi tb ON dh.MaThietBi = tb.MaThietBi
    ORDER BY dh.NgayDat DESC;
END
GO
-- Procedure xóa tất cả đơn hàng sau khi thanh toán
CREATE OR ALTER PROCEDURE sp_XoaTatCaDonHang
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Xóa tất cả đơn hàng chi tiết
        DELETE FROM DONHANG_CHITIET;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
CREATE OR ALTER PROCEDURE sp_GetAllKhachHang
AS
BEGIN
    SELECT 
        MaKhachHang,
        HoTen,
        SoDienThoai,
        DiaChi,
        NgaySinh,
        GioiTinh,
        LoaiKhachHang,
        ISNULL(KhuyenMaiApDung, 'Không có') AS KhuyenMaiApDung
    FROM KhachHang
    ORDER BY HoTen;
END
GO
CREATE OR ALTER PROCEDURE sp_GetNextMaHoaDon
AS
BEGIN
    SELECT ISNULL(MAX(MaHoaDon), 0) + 1 AS NextMaHoaDon
    FROM LICHSUMUAHANG_THIETBI;
END
GO
CREATE OR ALTER PROCEDURE sp_GetThongTinThanhToan
    @KhuyenMaiPercent DECIMAL(5,2) = 0 -- Phần trăm giảm giá, mặc định là 0
AS
BEGIN
    SELECT 
        tb.TenThietBi AS [Tên Thiết Bị],
        dh.SoLuongDatMua AS [Số Lượng],
        FORMAT(dh.GiaBan, 'N0') AS [Đơn Giá],
        FORMAT((dh.SoLuongDatMua * dh.GiaBan * (1 - @KhuyenMaiPercent/100)), 'N0') AS [Thành Tiền]
    FROM DONHANG_CHITIET dh
    INNER JOIN ThietBi tb ON dh.MaThietBi = tb.MaThietBi
    ORDER BY dh.NgayDat DESC;
END
GO
CREATE OR ALTER PROCEDURE sp_ThemKhachHangMoi
    @HoTen NVARCHAR(100),
    @SoDienThoai VARCHAR(15),
    @DiaChi NVARCHAR(200),
    @NgaySinh DATE,
    @GioiTinh NVARCHAR(10)
AS
BEGIN
    INSERT INTO KhachHang (HoTen, SoDienThoai, DiaChi, NgaySinh, GioiTinh, LoaiKhachHang, KhuyenMaiApDung)
    VALUES (@HoTen, @SoDienThoai, @DiaChi, @NgaySinh, @GioiTinh, N'Khách lẻ', N'Không có');
    
    SELECT SCOPE_IDENTITY() AS MaKhachHang;
END
GO
CREATE OR ALTER PROCEDURE sp_ThemHoaDonChiTiet
    @MaHoaDon INT,
    @MaThietBi INT,
    @MaKhachHang INT,
    @MaNhanVien INT,
    @SoLuong INT,
    @GiaBan DECIMAL(18,2),
    @NgayThanhToan DATE,
    @KhuyenMaiApDung NVARCHAR(100),
    @TongTien DECIMAL(18,2)
AS
BEGIN
    INSERT INTO LICHSUMUAHANG_THIETBI (MaHoaDon, MaThietBi, MaKhachHang, MaNhanVien, SoLuong, GiaBan, NgayThanhToan, KhuyenMaiApDung, TongTien)
    VALUES (@MaHoaDon, @MaThietBi, @MaKhachHang, @MaNhanVien, @SoLuong, @GiaBan, @NgayThanhToan, @KhuyenMaiApDung, @TongTien);
END
GO
-- Procedure lấy thông tin thiết bị từ DONHANG_CHITIET
CREATE OR ALTER PROCEDURE sp_GetThongTinDonHang
AS
BEGIN
    SELECT 
        MaThietBi,
        SoLuongDatMua,
        GiaBan
    FROM DONHANG_CHITIET;
END
GO
