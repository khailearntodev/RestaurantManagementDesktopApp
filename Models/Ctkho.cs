using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Ctkho
{
    public int Idkho { get; set; }

    public int IdnguyenLieu { get; set; }

    public int? SoLuongTonDu { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Kho IdkhoNavigation { get; set; } = null!;

    public virtual Nguyenlieu IdnguyenLieuNavigation { get; set; } = null!;
}
