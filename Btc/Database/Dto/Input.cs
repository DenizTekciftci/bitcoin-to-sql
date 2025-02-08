using System;
using System.Collections.Generic;

namespace Btc.Database.Dto;

public partial class Input
{
    public string Txid { get; set; } = null!;

    public string Source { get; set; } = null!;

    public ulong? Amount { get; set; }

    public string? Address { get; set; }
}
