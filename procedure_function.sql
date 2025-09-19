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
