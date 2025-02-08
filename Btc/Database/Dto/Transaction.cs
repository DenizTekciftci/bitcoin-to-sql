using System;
using System.Collections.Generic;

namespace Btc.Database.Dto;

public partial class Transaction
{
    public string Txid { get; set; } = null!;

    public string Block { get; set; } = null!;

    public ushort File { get; set; }
}
