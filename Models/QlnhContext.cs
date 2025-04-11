using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RestaurantManagement.Models;

public partial class QlnhContext : DbContext
{
    public QlnhContext()
    {
    }

    public QlnhContext(DbContextOptions<QlnhContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<Chamcong> Chamcongs { get; set; }

    public virtual DbSet<Chebien> Chebiens { get; set; }

    public virtual DbSet<Cthd> Cthds { get; set; }

    public virtual DbSet<Ctkho> Ctkhos { get; set; }

    public virtual DbSet<Ctmonan> Ctmonans { get; set; }

    public virtual DbSet<Ctnhapkho> Ctnhapkhos { get; set; }

    public virtual DbSet<Doanuong> Doanuongs { get; set; }

    public virtual DbSet<Hoadon> Hoadons { get; set; }

    public virtual DbSet<Kho> Khos { get; set; }

    public virtual DbSet<Nguyenlieu> Nguyenlieus { get; set; }

    public virtual DbSet<Nhanvien> Nhanviens { get; set; }

    public virtual DbSet<Nhapkho> Nhapkhos { get; set; }

    public virtual DbSet<Taikhoan> Taikhoans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

       //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
       => optionsBuilder.UseSqlServer("Data Source=KIARH\\MSSQLSERVER02;Initial Catalog=QLNH;Integrated Security=True;Trust Server Certificate=True");



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.ToTable("BAN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaBan)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasComputedColumnSql("('B'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.SucChua).HasDefaultValue(0);
        });

        modelBuilder.Entity<Chamcong>(entity =>
        {
            entity.ToTable("CHAMCONG");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GhiChu).HasMaxLength(100);
            entity.Property(e => e.IdnhanVien).HasColumnName("IDNhanVien");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.MaChamCong)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('CC'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.SoGioLam)
                .HasComputedColumnSql("(datediff(minute,[GioVao],[GioRa])/(60.0))", true)
                .HasColumnType("numeric(17, 6)");

            entity.HasOne(d => d.IdnhanVienNavigation).WithMany(p => p.Chamcongs)
                .HasForeignKey(d => d.IdnhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHAMCONG_IDNhanVien");
        });

        modelBuilder.Entity<Chebien>(entity =>
        {
            entity.ToTable("CHEBIEN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdhoaDon).HasColumnName("IDHoaDon");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.MaCheBien)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('CB'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);

            entity.HasOne(d => d.IdhoaDonNavigation).WithMany(p => p.Chebiens)
                .HasForeignKey(d => d.IdhoaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CHEBIEN_IDHoaDon");
        });

        modelBuilder.Entity<Cthd>(entity =>
        {
            entity.HasKey(e => new { e.IdhoaDon, e.IddoAnUong });

            entity.ToTable("CTHD");
            entity.Property(e => e.IsReady).HasDefaultValue(false);
            entity.Property(e => e.IdhoaDon).HasColumnName("IDHoaDon");
            entity.Property(e => e.IddoAnUong).HasColumnName("IDDoAnUong");
            entity.Property(e => e.GiaMon).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);

            entity.HasOne(d => d.IddoAnUongNavigation).WithMany(p => p.Cthds)
                .HasForeignKey(d => d.IddoAnUong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTHD_IDDoAnUong");

            entity.HasOne(d => d.IdhoaDonNavigation).WithMany(p => p.Cthds)
                .HasForeignKey(d => d.IdhoaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTHD_IDHoaDon");
        });

        modelBuilder.Entity<Ctkho>(entity =>
        {
            entity.HasKey(e => new { e.Idkho, e.IdnguyenLieu }).HasName("PK_CTK");

            entity.ToTable("CTKHO");

            entity.Property(e => e.Idkho).HasColumnName("IDKho");
            entity.Property(e => e.IdnguyenLieu).HasColumnName("IDNguyenLieu");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.SoLuongTonDu).HasDefaultValue(0);

            entity.HasOne(d => d.IdkhoNavigation).WithMany(p => p.Ctkhos)
                .HasForeignKey(d => d.Idkho)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTKHO_IDKho");

            entity.HasOne(d => d.IdnguyenLieuNavigation).WithMany(p => p.Ctkhos)
                .HasForeignKey(d => d.IdnguyenLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTKHO_IDNguyenLieu");
        });

        modelBuilder.Entity<Ctmonan>(entity =>
        {
            entity.HasKey(e => new { e.IddoAnUong, e.IdnguyenLieu }).HasName("PK_CTMA");

            entity.ToTable("CTMONAN");

            entity.Property(e => e.IddoAnUong).HasColumnName("IDDoAnUong");
            entity.Property(e => e.IdnguyenLieu).HasColumnName("IDNguyenLieu");

            entity.HasOne(d => d.IddoAnUongNavigation).WithMany(p => p.Ctmonans)
                .HasForeignKey(d => d.IddoAnUong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTMONAN_IDDoAnUong");

            entity.HasOne(d => d.IdnguyenLieuNavigation).WithMany(p => p.Ctmonans)
                .HasForeignKey(d => d.IdnguyenLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTMONAN_IDNguyenLieu");
        });

        modelBuilder.Entity<Ctnhapkho>(entity =>
        {
            entity.HasKey(e => new { e.IdnhapKho, e.IdnguyenLieu }).HasName("PK_CTNK");

            entity.ToTable("CTNHAPKHO");

            entity.Property(e => e.IdnhapKho).HasColumnName("IDNhapKho");
            entity.Property(e => e.IdnguyenLieu).HasColumnName("IDNguyenLieu");
            entity.Property(e => e.GiaNguyenLieu).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdnguyenLieuNavigation).WithMany(p => p.Ctnhapkhos)
                .HasForeignKey(d => d.IdnguyenLieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTNHAPKHO_IDNguyenLieu");

            entity.HasOne(d => d.IdnhapKhoNavigation).WithMany(p => p.Ctnhapkhos)
                .HasForeignKey(d => d.IdnhapKho)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTNHAPKHO_IDNhapKho");
        });

        modelBuilder.Entity<Doanuong>(entity =>
        {
            entity.ToTable("DOANUONG");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DonGia).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.MaDoAnUong)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasComputedColumnSql("('DAU'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.TenDoAnUong).HasMaxLength(100);
            entity.Property(e => e.TinhTrang).HasDefaultValue(true);
        });

        modelBuilder.Entity<Hoadon>(entity =>
        {
            entity.ToTable("HOADON");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Idban).HasColumnName("IDBan");
            entity.Property(e => e.IdnhanVien).HasColumnName("IDNhanVien");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('HD'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgayHoaDon).HasColumnType("datetime");
            entity.Property(e => e.TongGia).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdbanNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.Idban)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HOADON_IDBan");

            entity.HasOne(d => d.IdnhanVienNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.IdnhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HOADON_IDNhanVien");
        });

        modelBuilder.Entity<Kho>(entity =>
        {
            entity.ToTable("KHO");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaKho)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasComputedColumnSql("('K'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.TenKho).HasMaxLength(20);
        });

        modelBuilder.Entity<Nguyenlieu>(entity =>
        {
            entity.ToTable("NGUYENLIEU");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DonGia)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.DonVi).HasMaxLength(10);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.MaNguyenLieu)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('NL'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.TenNguyenLieu).HasMaxLength(50);
        });

        modelBuilder.Entity<Nhanvien>(entity =>
        {
            entity.ToTable("NHANVIEN");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CongViec).HasMaxLength(20);
            entity.Property(e => e.DiaChi).HasMaxLength(50);
            entity.Property(e => e.HoTen).HasMaxLength(50);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LoaiNhanVien).HasMaxLength(20);
            entity.Property(e => e.LuongThang).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.LuongTheoGio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MaNhanVien)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('NV'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.NgayVaoLam).HasColumnType("datetime");
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .HasColumnName("SDT");
        });

        modelBuilder.Entity<Nhapkho>(entity =>
        {
            entity.ToTable("NHAPKHO");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GiaNhap)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Idkho).HasColumnName("IDKho");
            entity.Property(e => e.MaNhapKho)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('NK'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.NgayNhap).HasColumnType("datetime");
            entity.Property(e => e.NguonNhap).HasMaxLength(50);
            entity.Property(e => e.SdtlienLac)
                .HasMaxLength(20)
                .HasColumnName("SDTLienLac");

            entity.HasOne(d => d.IdkhoNavigation).WithMany(p => p.Nhapkhos)
                .HasForeignKey(d => d.Idkho)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NHAPKHO_IDKho");
        });

        modelBuilder.Entity<Taikhoan>(entity =>
        {
            entity.ToTable("TAIKHOAN");

            entity.HasIndex(e => e.TenTaiKhoan, "UQ__TAIKHOAN__B106EAF8508F8E65").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdnhanVien).HasColumnName("IDNhanVien");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.MaTaiKhoan)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasComputedColumnSql("('TK'+right('00000'+CONVERT([varchar](5),[ID]),(5)))", true);
            entity.Property(e => e.MatKhau).HasMaxLength(100);
            entity.Property(e => e.TenTaiKhoan).HasMaxLength(20);

            entity.HasOne(d => d.IdnhanVienNavigation).WithMany(p => p.Taikhoans)
                .HasForeignKey(d => d.IdnhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TAIKHOAN_IDNhanVien");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
