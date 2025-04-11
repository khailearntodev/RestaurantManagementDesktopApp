using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Taikhoan
{
    public int Id { get; set; }

    public string? MaTaiKhoan { get; set; }

    public string TenTaiKhoan { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public int PhanQuyen { get; set; }

    public int IdnhanVien { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Nhanvien IdnhanVienNavigation { get; set; } = null!;
}
