using System;
using System.Collections.Generic;

namespace Btc.Database.Dto;

public partial class Block
{
    public string Hash { get; set; } = null!;

    public string PrevHash { get; set; } = null!;

    public ushort File { get; set; }

    public DateTime Time { get; set; }

    public int Height { get; set; }
}
