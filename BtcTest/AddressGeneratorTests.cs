using Btc;

namespace BtcTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void P2PKHScriptPubKey_ProducesAddress()
    {
        var hexScriptPubKey = "76A9149FCE4EAFA61717D924B1D09B30283DEEA3F1E3FD88AC";
        var bytes = Convert.FromHexString(hexScriptPubKey);
        var address = AddressGenerator.GenerateAddress(bytes);
        Assert.That(Equals(address, "1FZyazKTHZsvqdrGZse2PCihsBsQsb1Y6d"));
    }
    
    [Test]
    public void P2PKScriptPubKey_ProducesAddress()
    {
        var hexScriptPubKey = "4104678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5fac";
        var bytes = Convert.FromHexString(hexScriptPubKey);
        var address = AddressGenerator.GenerateAddress(bytes);
        Assert.That(Equals(address, "1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa"));
    }
    
    [Test]
    public void P2SHScriptPubKey_ProducesAddress()
    {
        var hexScriptPubKey = "a914748284390f9e263a4b766a75d0633c50426eb87587";
        var bytes = Convert.FromHexString(hexScriptPubKey);
        var address = AddressGenerator.GenerateAddress(bytes);
        Assert.That(Equals(address, "3CK4fEwbMP7heJarmU4eqA3sMbVJyEnU3V"));
    }
    
    [Test]
    public static void DecodeAndEncodeBechAddress()
    {
        byte[] a = Bech32.DecodeBech32("bc1qcash96s5jqppzsp8hy8swkggf7f6agex98an7h", out _, out _, out _);
        string b = Bech32.EncodeBech32(0, a, true);
        Assert.That(Equals("bc1qcash96s5jqppzsp8hy8swkggf7f6agex98an7h", b));
    }
    
    [Test]
    public static void Bech32Test()
    {
        byte[] a = Convert.FromHexString("841b80d2cc75f5345c482af96294d04fdd66b2b7");
        string b = Bech32.EncodeBech32(0, a,  true);
        Assert.That(Equals("bc1qssdcp5kvwh6nghzg9tuk99xsflwkdv4hgvq58q", b));
    }
    
    [Test]
    public static void ParseP2WPKH_Returns_Bech32()
    {
        byte[] bytes = Convert.FromHexString("0014841b80d2cc75f5345c482af96294d04fdd66b2b7");
        var address = AddressGenerator.GenerateAddress(bytes);
        Assert.That(Equals("bc1qssdcp5kvwh6nghzg9tuk99xsflwkdv4hgvq58q", address));
    }
    
    [Test]
    public static void ParseP2WSH_Returns_Bech32()
    {
        byte[] bytes = Convert.FromHexString("002065f91a53cb7120057db3d378bd0f7d944167d43a7dcbff15d6afc4823f1d3ed3");
        var address = AddressGenerator.GenerateAddress(bytes);
        Assert.That(Equals("bc1qvhu3557twysq2ldn6dut6rmaj3qk04p60h9l79wk4lzgy0ca8mfsnffz65", address));
    }
    
    [Test]
    public static void ParseP2TR_Returns_Bech32()
    {
        byte[] bytes = Convert.FromHexString("51200f0c8db753acbd17343a39c2f3f4e35e4be6da749f9e35137ab220e7b238a667");
        var address = AddressGenerator.GenerateAddress(bytes);
        Assert.That(Equals("bc1ppuxgmd6n4j73wdp688p08a8rte97dkn5n70r2ym6kgsw0v3c5ensrytduf", address));
    }
    
    [Test]
    public void OpCodes_test()
    {
        int opCode = OPCodes.CheckSig;
        Assert.That(Equals(opCode, 172));
    }
    
    [Test]
    public void ParseRev00000()
    {
        const string basePath = "D:\\Bitcoin\\blocks";
        var ctxInUndos = Parser.ParseRev(basePath, 0);
        Assert.That(true);
    }
}