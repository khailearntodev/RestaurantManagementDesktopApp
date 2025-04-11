using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Chamcong
{
    public int Id { get; set; }

    public string? MaChamCong { get; set; }

    public int IdnhanVien { get; set; }

    public DateOnly NgayChamCong { get; set; }

    public TimeOnly GioVao { get; set; }

    public TimeOnly GioRa { get; set; }

    public decimal? SoGioLam { get; set; }

    public string? GhiChu { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Nhanvien IdnhanVienNavigation { get; set; } = null!;
}
