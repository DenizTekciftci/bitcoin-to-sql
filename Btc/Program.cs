using System.Diagnostics;
using Btc;
using Btc.Database.Dto;
using EFCore.BulkExtensions;

const string basePath = "path_to_blk_files";
const bool blocksOnly = false;
const int threadCount = 5;
const int start = 0;
const int end = 10;

await Load(start, end, blocksOnly, threadCount);
// UpdateBlockHeights()
return;

Task Load(int start, int end, bool blockOnly = false, int threadCount = 1)
{
    var sw = new Stopwatch();
    sw.Start();
    var current = start;
    while (current <= end)
    {
        var batchStart = current;
        var threads = Enumerable.Range(0, threadCount)
            .Select(offset =>
                {
                    return blockOnly ? new Thread(() => ReadParseWriteBlocks(batchStart + offset)) : new Thread(() => ReadParseWriteEverything(batchStart + offset));
                }
            )
            .ToList();

        threads.ForEach(thread => thread.Start());
        threads.ForEach(thread => thread.Join());

        Console.WriteLine("batch complete");
        current += threadCount;
    }
    sw.Stop();
    var elapsed1 = sw.ElapsedMilliseconds;
    
    Console.WriteLine($"Elapsed time: {(int)(elapsed1 / 60_000)} min {(int)(elapsed1 % 60_000 / 1000)} seconds");
    return Task.CompletedTask;
}

void ReadParseWriteEverything(int fileNumber)
{
    var file = fileNumber;
    Console.WriteLine($"Parsing file: {file}");
    var blk = Parser.ParseBlk(basePath, file);
    
    var config = new BulkConfig
    {
        BatchSize = 100_000,
    };

    if(blk.Inputs.Any(i => i.Amount == null))
        Console.WriteLine("Missing");
        
    var transactionsTask = new BitcoinContext().BulkInsertAsync(blk.Transactions, config);
    var outputsTask = new BitcoinContext().BulkInsertAsync(blk.Outputs, config);
    var inputsTask = new BitcoinContext().BulkInsertAsync(blk.Inputs, config);
    Task.WaitAll(transactionsTask, outputsTask, inputsTask);
}

void ReadParseWriteBlocks(int fileNumber)
{
    var file = fileNumber;
    Console.WriteLine($"Parsing file: {file}");
    var blk = Parser.ParseBlocks(basePath, file);
    
    var config = new BulkConfig
    {
        BatchSize = 100_000,
    };

    new BitcoinContext().BulkInsert(blk, config);
}

void UpdateBlockHeights()
{
    using var context = new BitcoinContext();
    var allBlocks = context.Blocks;
    var currentBlock = allBlocks.OrderByDescending(b => b.Height).First();

    while (true)
    {
        var nextBlock = allBlocks.FirstOrDefault(b => b.PrevHash == currentBlock.Hash);
        if (nextBlock == null)
            break;
        nextBlock.Height = currentBlock.Height + 1;
        currentBlock = nextBlock;
        context.Blocks.Update(currentBlock);
    }

    context.SaveChanges();
}
