using System;
using System.Collections.Generic;

namespace Btc.Database.Dto;

public partial class Output
{
    public string Txid { get; set; } = null!;

    public string Address { get; set; } = null!;

    public ulong Amount { get; set; }
}
