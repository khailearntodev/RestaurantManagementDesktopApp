using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Ctmonan
{
    public int IddoAnUong { get; set; }

    public int IdnguyenLieu { get; set; }

    public int? SoLuongNguyenLieu { get; set; }

    public virtual Doanuong IddoAnUongNavigation { get; set; } = null!;

    public virtual Nguyenlieu IdnguyenLieuNavigation { get; set; } = null!;
}
