using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Ban
{
    public int Id { get; set; }

    public string? MaBan { get; set; }

    public int? SucChua { get; set; }

    public bool TrangThai { get; set; }

    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();
}
