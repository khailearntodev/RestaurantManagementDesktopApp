CREATE DATABASE QLNH
GO

USE QLNH 
GO
set dateformat dmy
CREATE TABLE [NHANVIEN]
(
	[ID] int identity(1,1) constraint[PK_NHANVIEN] primary key,
	[MaNhanVien] as ('NV' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[HoTen] nvarchar(50) not null,
	[SDT] nvarchar(20) not null,
	[DiaChi] nvarchar(50) not null,
	[NgaySinh] datetime not null,
	[CongViec] nvarchar(20),
	[NgayVaoLam] datetime not null,
	[LoaiNhanVien] nvarchar(20) not null, -- Full-time or Part-time
	[LuongThang] decimal(10,2), -- danh cho Fulltime, co the null đối với NV Partime 
	[LuongTheoGio] decimal(10,2),-- danh cho part tume, có thể null đối với NV FUlltime
	[SoNgayLamViec] int,  -- part-time
	[IsDeleted] bit default 0
)

CREATE TABLE [TAIKHOAN]
(
	[ID] int identity(1,1) constraint[PK_TAIKHOAN] primary key,
	[MaTaiKhoan] as ('TK' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[TenTaiKhoan] nvarchar(20) unique not null,
	[MatKhau] nvarchar(100) not null,
	[PhanQuyen] int not null, -- 0 : admin, 1 : nhan vien
	[IDNhanVien] int not null, -- FK
	[IsDeleted] bit default 0
)

CREATE TABLE [BAN]
(
	[ID] int identity(1,1) constraint[PK_BAN] primary key,
	[MaBan] as ('B' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[SucChua] int default 0,
	[TrangThai] bit -- 0: Het cho, 1: trong
)

CREATE TABLE [DOANUONG]
(
	[ID] int identity(1,1) constraint[PK_DOANUONG] primary key,
	[MaDoAnUong] as ('DAU' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[TenDoAnUong] nvarchar(100) not null,
	[AnhDoAnUong] nvarchar(max),
	[DonGia] decimal(10,2) not null default 0,
	[TinhTrang] bit default 1 not null, -- 0: het, 1: con
	[ThoiGianChuanBi] int not null default 0,
	[Loai] bit not null, -- 0: do an, 1: do uong
	[IsDeleted] bit default 0
)

CREATE TABLE [HOADON]
(
	[ID] int identity(1,1) constraint[PK_HOADON] primary key,
	[MaHoaDon] as ('HD' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[IDBan] int not null, -- FK
	[IDNhanVien] int not null, -- FK
	[NgayHoaDon] datetime not null,
	[TongGia] decimal(10,2) not null default 0, -- sum(CTHD.GiaMon)
	[IsDeleted] bit default 0
)

CREATE TABLE [CTHD]
(
	[IDHoaDon] int not null, -- FK
	[IDDoAnUong] int not null, -- FK
	constraint [PK_CTHD] primary key([IDHoaDon], [IDDoAnUong]),
	[SoLuong] int not null default 0,
	[GiaMon] decimal not null default 0, -- = DoAnUong.DonGia * SoLuong (ĐỀ XUẤT BỎ)
	[IsDeleted] bit default 0
)

CREATE TABLE [NGUYENLIEU]
(
	[ID] int identity(1,1) constraint[PK_NGUYENLIEU] primary key,
	[MaNguyenLieu] as ('NL' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[TenNguyenLieu] nvarchar(50) not null,
	[DonVi] nvarchar(10) not null,
	[DonGia] decimal(10,2) default 0,
	[TinhTrang] bit not null, -- 0: het, 1: con
	[Loai] bit not null, -- 0: nguyen lieu tho (rau, ga, hai san,...), 1: do uong
	[IsDeleted] bit default 0
	-- Đồ uống cũng đc xem như là nguyên liệu, có thể truyền qua bảng DOANUONG với Loai = 1 (do uong)
)

CREATE TABLE [CTMONAN] -- Chi co do an moi co chi tiet mon an
(
	[IDDoAnUong] int not null, -- FK
	[IDNguyenLieu] int not null, -- FK
	constraint [PK_CTMA] primary key([IDDoAnUong], [IDNguyenLieu]),
	[SoLuongNguyenLieu] int not null default 0
)

CREATE TABLE [KHO]
(
	[ID] int identity(1,1) constraint[PK_KHO] primary key,
	[MaKho] as ('K' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[TenKho] nvarchar(20) not null
)

CREATE TABLE [NHAPKHO]
(
	[ID] int identity(1,1) constraint[PK_NHAPKHO] primary key,
	[MaNhapKho] as ('NK' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[NguonNhap] nvarchar(50) not null,
	[NgayNhap] datetime,
	[GiaNhap] decimal default 0, -- sum(CTNhapKho.GiaNL)
	[SDTLienLac] nvarchar(20) not null,
	[IDKho] int not null, -- FK
)

CREATE TABLE [CTNHAPKHO]
(
	[IDNhapKho] int not null, -- FK
	[IDNguyenLieu] int not null, -- FK
	constraint [PK_CTNK] primary key([IDNhapKho], [IDNguyenLieu]),
	[SoLuongNguyenLieu] int not null default 0,
	[GiaNguyenLieu] decimal(18,2) not null default 0 -- = NguyenLieu.DonGia * SoLuong
)

CREATE TABLE [CTKHO]
(
	[IDKho] int not null, -- FK
	[IDNguyenLieu] int not null, -- FK
	constraint [PK_CTK] primary key ([IDKho], [IDNguyenLieu]),
	[SoLuongTonDu] int default 0,
	[IsDeleted] bit default 0
)

CREATE TABLE [CHEBIEN]
(
	[ID] int identity(1,1) constraint[PK_CHEBIEN] primary key,
	[MaCheBien] as ('CB' + RIGHT('00000' + CAST([ID] as varchar(5)), 5)) persisted,
	[IDHoaDon] int not null, -- fk, ko biet nen tham chieu den bang HOADON hay CTHD
	[ThoiGianChuanBi] int not null default 0,
	[IsDeleted] bit default 0
)

CREATE TABLE [CHAMCONG] -- THÊM
(
    [ID] INT IDENTITY(1,1) CONSTRAINT [PK_CHAMCONG] PRIMARY KEY, -- Khóa chính
    [MaChamCong] AS ('CC' + RIGHT('00000' + CAST([ID] AS VARCHAR(5)), 5)) PERSISTED, 
    [IDNhanVien] INT NOT NULL, -- FK tham chiếu đến nhân viên
    [NgayChamCong] DATE NOT NULL, -- Ngày chấm công
    [GioVao] TIME NOT NULL, -- Giờ vào
    [GioRa] TIME NOT NULL, -- Giờ ra
    [SoGioLam] AS DATEDIFF(MINUTE, [GioVao], [GioRa]) / 60.0 PERSISTED, -- Số giờ làm việc, tính tự động
	[GhiChu] NVARCHAR(100),
	[IsDeleted] bit default 0
)
-----------------------------------------------------------------------------------------------

-- Foreign key
alter table [TAIKHOAN] add constraint [FK_TAIKHOAN_IDNhanVien]
foreign key ([IDNhanVien]) references [NHANVIEN]([ID]);

alter table [HOADON] add constraint [FK_HOADON_IDBan]
foreign key ([IDBan]) references [BAN]([ID]);

alter table [HOADON] add constraint [FK_HOADON_IDNhanVien]
foreign key ([IDNhanVien]) references [NHANVIEN]([ID]);

alter table [CTHD] add constraint [FK_CTHD_IDHoaDon]
foreign key ([IDHoaDon]) references [HOADON]([ID]);

alter table [CTHD] add constraint [FK_CTHD_IDDoAnUong]
foreign key ([IDDoAnUong]) references [DOANUONG]([ID]);

alter table [CTMONAN] add constraint [FK_CTMONAN_IDDoAnUong]
foreign key ([IDDoAnUong]) references [DOANUONG]([ID]);

alter table [CTMONAN] add constraint [FK_CTMONAN_IDNguyenLieu]
foreign key ([IDNguyenLieu]) references [NGUYENLIEU]([ID]);

alter table [NHAPKHO] add constraint [FK_NHAPKHO_IDKho]
foreign key ([IDKho]) references [KHO]([ID]);

alter table [CTNHAPKHO] add constraint [FK_CTNHAPKHO_IDNhapKho]
foreign key ([IDNhapKho]) references [NHAPKHO]([ID]);

alter table [CTNHAPKHO] add constraint [FK_CTNHAPKHO_IDNguyenLieu]
foreign key ([IDNguyenLieu]) references [NGUYENLIEU]([ID]);

alter table [CTKHO] add constraint [FK_CTKHO_IDKho]
foreign key ([IDKho]) references [KHO]([ID]);

alter table [CTKHO] add constraint [FK_CTKHO_IDNguyenLieu]
foreign key ([IDNguyenLieu]) references [NGUYENLIEU]([ID]);

alter table [CHEBIEN] add constraint [FK_CHEBIEN_IDHoaDon]
foreign key ([IDHoaDon]) references [HOADON]([ID]);

ALTER TABLE [CHAMCONG] ADD CONSTRAINT [FK_CHAMCONG_IDNhanVien] 
FOREIGN KEY ([IDNhanVien]) REFERENCES [NHANVIEN]([ID]);

----------------------------------------------------------------------------
--- Example data
-- Inserting employees (both full-time and part-time)
INSERT INTO NHANVIEN (HoTen, SDT, DiaChi, NgaySinh, CongViec, NgayVaoLam, LoaiNhanVien, LuongThang, LuongTheoGio, SoNgayLamViec)
VALUES 
(N'Nguyễn Văn An', '0912345678', N'123 Đường A, Quận 1, TP.HCM', '15-05-1990', N'Quản Lý', '15-01-2022', 'Full-time', 15000000, 0, NULL),
(N'Trần Thị Bích', '0987654321', N'456 Đường B, Quận 2, TP.HCM', '20-08-1995', N'Phục Vụ', '01-03-2022', 'Full-time', 10000000, 0, NULL),
(N'Lê Văn Cường', '0901234567', N'789 Đường C, Quận 3, TP.HCM', '10-11-1997', N'Đầu Bếp', '25-02-2022', 'Full-time', 12000000, 0, NULL),
(N'Phạm Thị Dung', '0978123456', N'321 Đường D, Quận 4, TP.HCM', '25-06-1993', N'Order', '01-06-2023', 'Part-time', 0, 50000, 15);

-- insert account 
insert into TAIKHOAN (TenTaiKhoan, MatKhau, PhanQuyen, IDNhanVien)
values 
('admin', 'admin123', 0, 2),
('staff1', 'staff123', 1, 3),
('staff2', 'staff1234', 1, 4),
('staff3', 'staff12345', 1, 5)

---- insert BAN
INSERT INTO BAN (SucChua, TrangThai)
VALUES 
(4, 1),  
(2, 1),  
(6, 1),  
(4, 1),  
(8, 1),  
(5, 1),
(10, 1)
select * from BAN

insert into KHO (TenKho)
values (N'Kho nguyên liệu thô'),
(N'Kho nước uống')
select * from KHO