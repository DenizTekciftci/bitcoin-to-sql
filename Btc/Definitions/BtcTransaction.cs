using System.Security.Cryptography;
using System.Text;

namespace Btc;

public class BtcTransaction
{
    // The hash of the block in which the transaction was mined
    public string Hash { get; set; }
    // 4 bytes
    public byte[] Version { get; set; }
    public byte Marker { get; set; }
    public byte Flag { get; set; }
    public Compact InputCount { get; set; }
    public List<TransactionInput> Inputs { get; set; } = new();
    public Compact OutputCount { get; set; }
    public List<TransactionOutput> Outputs { get; set; } = new();
    public List<TransactionWitness> Witness { get; set; } = new();
    
    public byte[] Locktime { get; set; }

    public byte[] Data
    {
        get
        {
            return [..Version, ..InputCount.Data, ..Inputs.SelectMany(i => i.Data).ToArray(), ..OutputCount.Data, ..Outputs.SelectMany(o => o.Data).ToArray(), ..Locktime];
        }
    }
    
    public string TXID
    {
        get
        {
            if (txid == null)
            {
                var sha256 = SHA256.Create();
                txid = Convert.ToHexString(sha256.ComputeHash(sha256.ComputeHash(Data)).Reverse().ToArray());
            }
            
            return txid;
        }
    }
    
    private string? txid { get; set; } = null;
    
    // 4 bytes

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Version: {Convert.ToHexString(Version)}");
        sb.AppendLine($"TXID: {TXID}");
        sb.AppendLine($"Input count: {InputCount.Value}");
        foreach (var input in Inputs)
            sb.AppendLine(input.ToString());
        
        sb.AppendLine($"Output count: {OutputCount.Value}");
        foreach (var output in Outputs)
            sb.AppendLine(output.ToString());
        return sb.ToString();
    }
}

public class TransactionInput
{
    // 32 bytes
    public byte[] TXID { get; set; }
    // 4 bytes
    public byte[] VOUT { get; set; }
    public Compact ScriptSigSize { get; set; }
    // Variable
    public byte[] ScriptSig { get; set; }
    // 4 bytes
    public byte[] Sequence { get; set; }
    
    public string? Address { get; set; }
    public ulong? Amount { get; set; }
    public string Txid => Convert.ToHexString(TXID.Reverse().ToArray());
    public byte[] Data => [..TXID, ..VOUT, ..ScriptSigSize.Data, ..ScriptSig, ..Sequence];
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"TXID: {Txid}");
        sb.AppendLine($"VOUT: {BitConverter.ToUInt32(VOUT)}");
        sb.AppendLine($"ScriptSigSize: {ScriptSigSize.Value}");
        sb.AppendLine($"ScriptSig: {Convert.ToHexString(ScriptSig)}");
        sb.AppendLine($"Sequence: {Convert.ToHexString(Sequence)}");
        return sb.ToString();
    }
}

public class TransactionOutput
{
    public ulong Amount { get; set; }
    public Compact ScriptPubKeySize { get; set; }
    public byte[] ScriptPubKey { get; set; }
    
    public string Address => AddressGenerator.GenerateAddress(ScriptPubKey);
    
    public byte[] Data => [..BitConverter.GetBytes(Amount), ..ScriptPubKeySize.Data, ..ScriptPubKey];

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Amount: {Amount / 100_000_000.0} BTC");
        sb.AppendLine($"ScriptPubKeySize: {ScriptPubKeySize.Value}");
        sb.AppendLine($"ScriptPubKey: {Convert.ToHexString(ScriptPubKey)}");
        sb.AppendLine($"Address: {Address}");
        return sb.ToString();
    }
}

public class TransactionWitness
{
    public Compact StackItemsCount { get; set; }

    public List<StackItem> StackItems { get; set; } = new();
}

public class StackItem
{
    public Compact Size { get; set; }
    // Variable
    public byte[] Item { get; set; } 
}

