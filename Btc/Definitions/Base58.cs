﻿using System.Numerics;

namespace Btc;

public class Base58
{
    private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    
    public static string Encode(byte[] data)
    {
        BigInteger intData = 0;
        for (int i = 0; i < data.Length; i++)
        {
            intData = intData * 256 + data[i];
        }

        // Encode BigInteger to Base58 string
        string result = "";
        while (intData > 0)
        {
            int remainder = (int)(intData % 58);
            intData /= 58;
            result = Digits[remainder] + result;
        }

        // Append `1` for each leading 0 byte
        for (int i = 0; i < data.Length && data[i] == 0; i++)
        {
            result = '1' + result;
        }
        return result;
    }
}