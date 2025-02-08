namespace Btc;

public class CTxInUndo
{
    public ulong Height { get; set; }
    public ulong? Version { get; set; }
    public ulong Amount { get; set; }
    public byte[] ScriptPubKey { get; set; }
    public string Address => AddressGenerator.GenerateAddress(ScriptPubKey);
}