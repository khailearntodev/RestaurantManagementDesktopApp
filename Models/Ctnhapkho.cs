using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Ctnhapkho
{
    public int IdnhapKho { get; set; }

    public int IdnguyenLieu { get; set; }

    public int? SoLuongNguyenLieu { get; set; }

    public decimal? GiaNguyenLieu { get; set; }

    public virtual Nguyenlieu IdnguyenLieuNavigation { get; set; } = null!;

    public virtual Nhapkho IdnhapKhoNavigation { get; set; } = null!;
}
