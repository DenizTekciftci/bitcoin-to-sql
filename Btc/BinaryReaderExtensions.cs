using NBitcoin;

namespace Btc;

public static class BinaryReaderExtensions
{
    private const int specialScriptsCount = 6; 
    private const int max_script_size = 10000; 
    public static Compact ReadCompact(this BinaryReader reader)
    {
        var d = reader.ReadByte();
        
        if (d <= 252)
        {
            return new Compact(d);
        }
        
        // next 2 bytes
        if (d == 253)
        {
            return new Compact(reader.ReadUInt16());
        }
        
        // next 4 bytes
        if (d == 254)
        {
            return new Compact(reader.ReadUInt32());
        }
        
        // next 8 bytes
        return new Compact(reader.ReadUInt64());
    }

    public static ulong ReadVarInt(this BinaryReader reader)
    {
        ulong n = 0;
        while(true) 
        {
            uint chData = reader.ReadByte();
            n = (n << 7) | (chData & 0x7F);
            if ((chData & 0x80) != 0)
                n++;
            else
                return n;
        }
    }

    public static byte[] ReadCompressedPubKey(this BinaryReader reader)
    {
        var nSize = reader.ReadVarInt();

        if (nSize < specialScriptsCount)
        {
            return DecompressScript(reader, nSize);
        }

        // Too large - return invalid script
        nSize -= specialScriptsCount;
        if (nSize  > max_script_size)
        {
            return [OPCodes.Return];
        }
        else
        {
            return reader.ReadBytes((int)nSize);
        }
    }

    // https://github.com/bitcoin/bitcoin/blob/v0.14.2/src/compressor.cpp#L133
    public static ulong ReadCompressedAmount(this BinaryReader reader)
    {
        var x = reader.ReadVarInt();
        if (x == 0)
            return 0;
        x--;
        
        var e = x % 10;
        x /= 10;
        ulong n = 0;
        if (e < 9) {
            var d = (x % 9) + 1;
            x /= 9;
            n = x*10 + d;
        } else {
            n = x+1;
        }
        while (e != 0) {
            n *= 10;
            e--;
        }
        return n;
    }

    public static byte[] DecompressScript(BinaryReader reader, ulong nSize)
    {
        switch(nSize) {
            case 0x00:
                return [OPCodes.Dup, OPCodes.Hash160, OPCodes.Push._20, ..reader.ReadBytes(20), OPCodes.EqualVerify, OPCodes.CheckSig];
            case 0x01:
                return [OPCodes.Hash160, OPCodes.Push._20, ..reader.ReadBytes(20), OPCodes.Equal];
            case 0x02:
            case 0x03:
                return [OPCodes.Push._33, 0x03, ..reader.ReadBytes(32), OPCodes.CheckSig];
            case 0x04:
            case 0x05:
                try
                {
                    var pubKey = new PubKey([(byte)(int)(nSize-2), ..reader.ReadBytes(32)]);
                    var decompressed = pubKey.Decompress().ToBytes();
                    if (decompressed.Length == 65)
                    {
                        return [OPCodes.Push._65, ..decompressed, OPCodes.CheckSig];
                    }
                }
                catch (Exception e)
                {
                    return [OPCodes.Return];
                }
                break;
        }
        return [OPCodes.Return];
    }
    
}