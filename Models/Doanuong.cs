using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Doanuong
{
    public int Id { get; set; }

    public string? MaDoAnUong { get; set; }

    public string TenDoAnUong { get; set; } = null!;

    public string? AnhDoAnUong { get; set; }

    public decimal? DonGia { get; set; }

    public bool? TinhTrang { get; set; }

    public int? ThoiGianChuanBi { get; set; }

    public bool Loai { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Cthd> Cthds { get; set; } = new List<Cthd>();

    public virtual ICollection<Ctmonan> Ctmonans { get; set; } = new List<Ctmonan>();
}
