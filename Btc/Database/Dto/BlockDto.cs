using CsvHelper.Configuration.Attributes;

namespace Btc.Database.Dto;

public class BlockDto
{
    [Name("hash")]
    public string Hash { get; set; }
    [Name("prev_hash")]
    public string PrevHash { get; set; }
    [Name("file")]
    public ushort File { get; set; }
    [Name("time")]
    [Format("yyyy-MM-dd HH:mm:ss")]
    public DateTime Time { get; set; }
}