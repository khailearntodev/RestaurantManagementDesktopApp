using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Cthd
{
    public int IdhoaDon { get; set; }

    public int IddoAnUong { get; set; }

    public int? SoLuong { get; set; }

    public decimal? GiaMon { get; set; }

    public bool? IsDeleted { get; set; } = false;
    
    public bool? IsReady { get; set; }

    public virtual Doanuong IddoAnUongNavigation { get; set; } = null!;

    public virtual Hoadon IdhoaDonNavigation { get; set; } = null!;
}
