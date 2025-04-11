using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Hoadon
{
    public int Id { get; set; }

    public string? MaHoaDon { get; set; }

    public int Idban { get; set; }

    public int IdnhanVien { get; set; }

    public DateTime NgayHoaDon { get; set; }

    public decimal? TongGia { get; set; }

    public bool? IsDeleted { get; set; } = false;

    public virtual ICollection<Chebien> Chebiens { get; set; } = new List<Chebien>();

    public virtual ICollection<Cthd> Cthds { get; set; } = new List<Cthd>();

    public virtual Ban IdbanNavigation { get; set; } = null!;

    public virtual Nhanvien IdnhanVienNavigation { get; set; } = null!;
}
