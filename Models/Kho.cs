using System;
using System.Collections.Generic;

namespace RestaurantManagement.Models;

public partial class Kho
{
    public int Id { get; set; }

    public string? MaKho { get; set; }

    public string? TenKho { get; set; } = null!;

    public virtual ICollection<Ctkho> Ctkhos { get; set; } = new List<Ctkho>();

    public virtual ICollection<Nhapkho> Nhapkhos { get; set; } = new List<Nhapkho>();
}
