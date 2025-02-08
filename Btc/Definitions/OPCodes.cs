namespace Btc;

// https://learnmeabitcoin.com/technical/script/
public static class OPCodes
{
    public static class Push
    {
        public const byte _0 = 0x00;
        public const byte _1 = 0x01;
        public const byte _20 = 0x14;
        public const byte _32 = 0x20;
        public const byte _33 = 0x21;
        public const byte _65 = 0x41;
    }
    
    public const byte Op_1 = 0x51;

    public const byte CheckSig = 0xAC;
    public const byte Equal = 0x87;
    public const byte EqualVerify = 0x88;
    public const byte Hash160 = 0xA9;
    public const byte Dup = 0x76;
    public const byte Return = 0x6a;
    
}