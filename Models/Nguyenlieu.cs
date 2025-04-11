using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Nguyenlieu
{
    public int Id { get; set; }

    public string? MaNguyenLieu { get; set; }

    public string TenNguyenLieu { get; set; } = null!;

    public string? DonVi { get; set; } = null!;

    public decimal? DonGia { get; set; }

    public bool? TinhTrang { get; set; }

    public bool? Loai { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Ctkho> Ctkhos { get; set; } = new List<Ctkho>();

    public virtual ICollection<Ctmonan> Ctmonans { get; set; } = new List<Ctmonan>();

    public virtual ICollection<Ctnhapkho> Ctnhapkhos { get; set; } = new List<Ctnhapkho>();
}
