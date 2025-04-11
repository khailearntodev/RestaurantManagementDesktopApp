using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Nhapkho
{
    public int Id { get; set; }

    public string? MaNhapKho { get; set; }

    public string? NguonNhap { get; set; } = null!;

    public DateTime? NgayNhap { get; set; }

    public decimal? GiaNhap { get; set; }

    public string? SdtlienLac { get; set; } = null!;

    public int Idkho { get; set; }

    public virtual ICollection<Ctnhapkho> Ctnhapkhos { get; set; } = new List<Ctnhapkho>();

    public virtual Kho IdkhoNavigation { get; set; } = null!;
}
