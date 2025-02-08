namespace Btc.Database.Dto;

public class Blk
{
    public IEnumerable<Block> Blocks { get; set; }
    public IEnumerable<Transaction> Transactions { get; set; }
    public IEnumerable<Input> Inputs { get; set; }
    public IEnumerable<Output> Outputs { get; set; }
}