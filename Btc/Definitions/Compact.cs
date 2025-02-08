namespace Btc;

public class Compact
{
    public Compact(byte val)
    {
        Value = val;
        _data = [val];
    }
    
    public Compact(ushort val)
    {
        Value = val;
        _data = [253, ..BitConverter.GetBytes(val)];
    }
    
    public Compact(uint val)
    {
        Value = val;
        _data = [254, ..BitConverter.GetBytes(val)];
    }
    
    public Compact(ulong val)
    {
        Value = val;
        _data = [255, ..BitConverter.GetBytes(val)];
    }
    
    public ulong Value { get; }
    public byte[] Data => _data;
    private byte[] _data;
    
}