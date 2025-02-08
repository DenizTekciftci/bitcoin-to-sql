using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;

namespace Btc;

public static class AddressGenerator
{
    private static readonly byte[] P2PKHPrefix = [OPCodes.Dup, OPCodes.Hash160, OPCodes.Push._20];
    private static readonly byte[] P2PKHSuffix = [OPCodes.EqualVerify, OPCodes.CheckSig];
    private static readonly int P2PKHLength = 25;
    
    private static readonly byte[] P2PKPrefix = [OPCodes.Push._65];
    private static readonly byte[] P2PKSuffix = [OPCodes.CheckSig];
    private static readonly int P2PKLength = 67;

    private static readonly byte[] P2WPKHPrefix = [OPCodes.Push._0, OPCodes.Push._20];
    private static readonly int P2WPKHLength = 22;
    
    private static readonly byte[] P2SHPrefix = [OPCodes.Hash160, OPCodes.Push._20];
    private static readonly byte[] P2SHSuffix = [OPCodes.Equal];
    private static readonly int P2SHLength = 23;
    
    private static readonly byte[] P2WSHPrefix = [OPCodes.Push._0, OPCodes.Push._32];
    private static readonly int P2WSHLength = 34;
    
    private static readonly byte[] P2TRPrefix = [OPCodes.Op_1, OPCodes.Push._32];
    private static readonly int P2TRLength = 34;
    
    public static string GenerateAddress(byte[] scriptPubKey)
    {
        if (scriptPubKey.Length < 5)
        {
            return "Unknown";
        }
        
        // P2PKH
        if (scriptPubKey[..3].SequenceEqual(P2PKHPrefix) && scriptPubKey[^2..].SequenceEqual(P2PKHSuffix) && scriptPubKey.Length == P2PKHLength)
                return GenerateAddressFromScriptPubKey(scriptPubKey[3..^2]);

        // P2PK (rarely used, the produced address does not exist, but matches explorers)
        if (scriptPubKey[..1].SequenceEqual(P2PKPrefix) && scriptPubKey[^1..].SequenceEqual(P2PKSuffix) && scriptPubKey.Length == P2PKLength)
            return GenerateAddressFromP2PK(scriptPubKey[1..^1]);
        
        // P2WPKH
        if (scriptPubKey[..2].SequenceEqual(P2WPKHPrefix) && scriptPubKey.Length == P2WPKHLength)
            return Bech32Encode(scriptPubKey[2..], 0);
        
        // P2SH
        if (scriptPubKey[..2].SequenceEqual(P2SHPrefix) && scriptPubKey[^1..].SequenceEqual(P2SHSuffix) && scriptPubKey.Length == P2SHLength)
            return GenerateScriptAddressFromScriptPubKey(scriptPubKey[2..^1]);
        
        // P2WSH
        if (scriptPubKey[..2].SequenceEqual(P2WSHPrefix) && scriptPubKey.Length == P2WSHLength)
            return Bech32Encode(scriptPubKey[2..], 0);
        
        // P2TR
        if (scriptPubKey[..2].SequenceEqual(P2TRPrefix) && scriptPubKey.Length == P2TRLength)
            return Bech32Encode(scriptPubKey[2..], 1);

        return "Unknown";
    }

    public static string GenerateAddressFromScriptPubKey(byte[] scriptPubKey)
    {
        using var sha256 = SHA256.Create();
        byte[] prefixed = [0, ..scriptPubKey];
        var hash1 = sha256.ComputeHash(prefixed);
        var hash2 = sha256.ComputeHash(hash1);
        var checkSum = hash2[..4];
        return Base58.Encode([..prefixed, ..checkSum]);
    }
    
    public static string GenerateScriptAddressFromScriptPubKey(byte[] scriptPubKey)
    {
        using var sha256 = SHA256.Create();
        byte[] prefixed = [5, ..scriptPubKey];
        var hash1 = sha256.ComputeHash(prefixed);
        var hash2 = sha256.ComputeHash(hash1);
        var checkSum = hash2[..4];
        return Base58.Encode([..prefixed, ..checkSum]);
    }
    
    public static string Bech32Encode(byte[] scriptPubKey, byte witnessVersion)
    {
        return Bech32.EncodeBech32(witnessVersion, scriptPubKey, true);
    }
    
    public static string GenerateAddressFromP2PK(byte[] scriptPubKey)
    {
        // Sha256 + RipeMD160 to hash public key to the format of P2PKH
        using var sha256 = SHA256.Create();
        var ripeMd160 = new RipeMD160Digest();
        
        var hash1 = sha256.ComputeHash(scriptPubKey);
        ripeMd160.BlockUpdate(hash1, 0, hash1.Length);
        byte[] result = new byte[ripeMd160.GetDigestSize()];
        ripeMd160.DoFinal(result, 0);
        
        return GenerateAddressFromScriptPubKey(result);
    }
    
}