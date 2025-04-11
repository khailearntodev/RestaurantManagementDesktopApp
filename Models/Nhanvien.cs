using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Nhanvien
{
    public int Id { get; set; }

    public string? MaNhanVien { get; set; }

    public string HoTen { get; set; } = null!;

    public string Sdt { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public DateTime NgaySinh { get; set; }

    public string? CongViec { get; set; }

    public DateTime NgayVaoLam { get; set; }

    public string LoaiNhanVien { get; set; } = null!;

    public decimal? LuongThang { get; set; }

    public decimal? LuongTheoGio { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Chamcong> Chamcongs { get; set; } = new List<Chamcong>();

    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();

    public virtual ICollection<Taikhoan> Taikhoans { get; set; } = new List<Taikhoan>();
}
