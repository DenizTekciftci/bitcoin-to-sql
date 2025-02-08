using Btc.Database.Dto;

namespace Btc;

public static class Parser
{
    private const ulong reward = (5_000_000_000); // 50 BTC
    private const int halving = 210_000;
    private const string coinbase = "0000000000000000000000000000000000000000000000000000000000000000";

    public static Blk ParseBlk(string basePath, int fileNumber)
    {
        var fileId = fileNumber.ToString().PadLeft(5, '0');
        var path = $"{basePath}\\blk{fileId}.dat";
        using var fileStream = new FileStream(path, FileMode.Open);
        using var binaryReader = new BinaryReader(fileStream);

        var blockHeaders = new Dictionary<string, (string, Header)>();
        var blockTransactions = new Dictionary<string, List<BtcTransaction>>();

        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
        {
            var debug = false;
            var header = ParseBlockHeader(binaryReader);
            header.FileId = fileId;
            blockHeaders.Add(header.BlockHeader.PrevHash, (header.BlockHash, header));
            
            var currentBlockTransactions = ParseBlockTransactions(header, binaryReader, debug);
            blockTransactions.Add(header.BlockHash, currentBlockTransactions);
        }
        
        var undos = ParseRev(basePath, fileNumber);
        var blockHeight = from bt in blockTransactions.ToList()
            join b in new BitcoinContext().Blocks.Where(b => b.File == fileNumber) on bt.Key equals b.Hash
            select (transactions: bt.Value, Height: b.Height);  
        var blockHeightList = blockHeight.OrderBy(h => h.Height).ToList();
        
        var p2pInputsDto = blockHeightList
            .Where(t => t.transactions.Count > 1) // Must have transaction(s) besides coinbase
            .SelectMany(t => t.transactions.Skip(1)) // Skip the coinbase
            .SelectMany(x => x.Inputs.Select(i => (i, x.TXID)))
            .Select((input, i) => new Input
            {
                Txid = input.TXID, // Txid of transaction
                Source = input.i.Txid, // Txid of the UTXO
                Amount = undos[i].Amount,
                Address = undos[i].Address,
            });

        var coinbaseInputs = blockHeightList
            .Select(t => (BtcTransaction: t.transactions.First(), Height: t.Height)) // The coinbase
            .SelectMany(x => x.BtcTransaction.Inputs.Select(i => (Input: i, Txid: x.BtcTransaction.TXID, Height: x.Height)))
            .Select(x => new Input
            {
                Txid = x.Txid,
                Source = x.Input.Txid,
                Amount = reward / (ulong)Math.Pow(2, x.Height / halving),
                Address = coinbase
            });


        return new Blk
        {
            Transactions = blockHeightList.SelectMany(t => t.transactions.Select(t =>
                new Transaction
                {
                    Block = t.Hash,
                    Txid = t.TXID,
                    File = (ushort)fileNumber,
                })
            ),
            Inputs = coinbaseInputs.Concat(p2pInputsDto),
            Outputs = blockHeightList.SelectMany(t => t.transactions.SelectMany(t => t.Outputs.Select(o =>
                        new Output()
                        {
                            Txid = t.TXID,
                            Amount = o.Amount,
                            Address = o.Address,
                        }
                    )
                )
            )
        };
    }
    
    public static IEnumerable<Block> ParseBlocks(string basePath, int fileNumber)
    {
        var fileId = fileNumber.ToString().PadLeft(5, '0');
        var path = $"{basePath}\\blk{fileId}.dat";
        using var fileStream = new FileStream(path, FileMode.Open);
        using var binaryReader = new BinaryReader(fileStream);
        var blocks = new Dictionary<string, Block>();
        
        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
        {
            var header = ParseBlockHeader(binaryReader);
            header.FileId = fileId;
            blocks.Add(header.BlockHeader.PrevHash, new Block
            {
                Hash = header.BlockHash,
                PrevHash = header.BlockHeader.PrevHash,
                File = (ushort)fileNumber,
                Time = header.BlockHeader.TimeStamp,
                Height = -1,
            });
            ParseBlockTransactions(header, binaryReader);
        }
        
        // var context = new BitcoinContext();
        // var lastBlockLoaded = context.Blocks.First(x => blocks.Select(kv => kv.Value.PrevHash).Contains(x.Hash));
        // var height = lastBlockLoaded.Height + 1;
        // var currentBlock = blocks[lastBlockLoaded.Hash];
        // currentBlock.Height = height;
        // while (currentBlock != null)
        // {
        //     height++;
        //     var nextBlock = blocks[currentBlock.Hash];
        //     nextBlock.Height = height;
        //     try
        //     {
        //         currentBlock = blocks[nextBlock.Hash];
        //     }
        //     catch (Exception e)
        //     {
        //         currentBlock = null;
        //     }
        // }
        
        return blocks.Select(x => x.Value);
    }
    
    public static Header ParseBlockHeader(BinaryReader reader)
    {
        try
        {
            var header = new Header();
            header.MagicBytes = reader.ReadBytes(4);
            header.Size = reader.ReadUInt32();
            header.BlockHeader = new BlockHeader
            {
                Version = reader.ReadBytes(4),
                PreviousBlockHash = reader.ReadBytes(32),
                MerkleRoot = reader.ReadBytes(32),
                Time = reader.ReadBytes(4),
                Bits = reader.ReadBytes(4),
                Nonce = reader.ReadBytes(4),
            };
            header.TxCount = reader.ReadCompact();
            return header;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    public static List<BtcTransaction> ParseBlockTransactions(Header header, BinaryReader reader, bool debug = false)
    {
        var tx = new List<BtcTransaction>();

        for (uint i = 0; i < header.TxCount.Value; i++)
        {
            var transaction = ParseTransaction(reader, debug);
            transaction.Hash = header.BlockHash;
            tx.Add(transaction);
        }
        return tx;
    }

    public static BtcTransaction ParseTransaction(BinaryReader reader, bool debug = false)
    {
        var transaction = new BtcTransaction();
        transaction.Version = reader.ReadBytes(4);

        // if the transaction is segwit, the following 2 bytes are marker and flag
        // if it is not a segwit, they are part of the input count / transactions
        // so we roll back the position of the stream

        var flag = reader.ReadByte();
        var marker = reader.ReadByte();
        var isSegwit = flag == 0 && marker >= 1;

        if (!isSegwit)
        {
            reader.BaseStream.Position -= 2;
        }

        transaction.InputCount = reader.ReadCompact();

        for (uint i = 0; i < transaction.InputCount.Value; i++)
        {
            var input = new TransactionInput();
            input.TXID = reader.ReadBytes(32);
            input.VOUT = reader.ReadBytes(4);
            input.ScriptSigSize = reader.ReadCompact();
            input.ScriptSig = reader.ReadBytes((int)input.ScriptSigSize.Value);
            input.Sequence = reader.ReadBytes(4);

            transaction.Inputs.Add(input);
        }

        transaction.OutputCount = reader.ReadCompact();

        for (uint i = 0; i < transaction.OutputCount.Value; i++)
        {
            var output = new TransactionOutput();
            output.Amount = reader.ReadUInt64();
            output.ScriptPubKeySize = reader.ReadCompact();
            output.ScriptPubKey = reader.ReadBytes((int)output.ScriptPubKeySize.Value);

            transaction.Outputs.Add(output);
        }

        if (isSegwit)
        {
            for (uint i = 0; i < transaction.InputCount.Value; i++)
            {
                var witness = new TransactionWitness();
                witness.StackItemsCount = reader.ReadCompact();
                for (uint j = 0; j < witness.StackItemsCount.Value; j++)
                {
                    var stackItem = new StackItem();
                    stackItem.Size = reader.ReadCompact();
                    stackItem.Item = reader.ReadBytes((int)stackItem.Size.Value);
                    witness.StackItems.Add(stackItem);
                }

                transaction.Witness.Add(witness);
            }
        }

        transaction.Locktime = reader.ReadBytes(4);
        return transaction;
    }

    public static List<CTxInUndo> ParseRev(string basePath, int fileNumber)
    {
        var fileId = fileNumber.ToString().PadLeft(5, '0');
        var path = $"{basePath}\\rev{fileId}.dat";
        using var fileStream = new FileStream(path, FileMode.Open);
        using var binaryReader = new BinaryReader(fileStream);
        var ctxList = new List<CTxInUndo>();
        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
        {
            try
            {
                var magicBytes = Convert.ToHexString(binaryReader.ReadBytes(4));
                if (magicBytes != "F9BEB4D9")
                    throw new Exception("Parsed wrong");
                var size = binaryReader.ReadUInt32();
                var txCount = binaryReader.ReadCompact();
                
                for (uint i = 0; i < txCount.Value; i++)
                {
                    var inputCount = binaryReader.ReadCompact(); // Could be compact?
                    for (uint j = 0; j < inputCount.Value; j++)
                    {
                        var ctxout = new CTxInUndo();
                        ctxout.Height = binaryReader.ReadVarInt();
                        ctxout.Version = binaryReader.ReadVarInt();
                        ctxout.Amount = binaryReader.ReadCompressedAmount();
                        ctxout.ScriptPubKey = binaryReader.ReadCompressedPubKey();
                        ctxList.Add(ctxout);
                    }
                }

                var signature = binaryReader.ReadBytes(32);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return ctxList;
    }
}