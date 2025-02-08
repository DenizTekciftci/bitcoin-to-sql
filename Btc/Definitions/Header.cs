using System.Security.Cryptography;

namespace Btc;

public class Header
{
    // 4 bytes
    public byte[] MagicBytes { get; set; }
    // 4 bytes
    public UInt32 Size { get; set; }
    
    // 80 bytes
    public BlockHeader BlockHeader { get; set; }
    public Compact TxCount { get; set; }
    
    public string FileId { get; set; }

    public string BlockHash
    {
        get
        {
            var sha256 = SHA256.Create();
            return Convert.ToHexString(
                sha256.ComputeHash(sha256.ComputeHash(BlockHeader.Data))
                    .Reverse()
                    .ToArray()
                );
        }
    }

    public override string ToString()
    {
        return $"MagicBytes: {Convert.ToHexString(MagicBytes)} \nBlockHeader: {Convert.ToHexString(BlockHeader.Data)}\nSize: {Size}, \nTxCount: {TxCount.Value}";
    }
}

public class BlockHeader
{
    public byte[] Version { get; set; }
    public byte[] PreviousBlockHash { get; set; }
    public byte[] MerkleRoot { get; set; }
    public byte[] Time { get; set; }
    public byte[] Bits { get; set; }
    public byte[] Nonce { get; set; }
    
    public byte[] Data => [..Version, ..PreviousBlockHash, ..MerkleRoot, ..Time, ..Bits, ..Nonce];
    public string PrevHash => Convert.ToHexString(PreviousBlockHash.Reverse().ToArray());
    public DateTime TimeStamp => DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToUInt32(Time)).DateTime;
}