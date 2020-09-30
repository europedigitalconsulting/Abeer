using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace Abeer.Shared
{
    public class BatchBlockChain : BlockChain
    {
        public BatchBlockChain()
        {
        }

        public BatchBlockChain(string identifier, long difficulty) : base(identifier, difficulty)
        {
        }

        protected override BlockChainType BlockChainType => BlockChainType.Batch; 
    }

    public class CardBlockChain : BlockChain
    {
        public CardBlockChain()
        {
        }

        public CardBlockChain(string identifier, long difficulty) : base(identifier, difficulty)
        {
        }

        protected override BlockChainType BlockChainType => BlockChainType.Card;
    }

    public class TokenBlockChain : BlockChain
    {
        public TokenBlockChain()
        {
        }

        public TokenBlockChain(string identifier, long difficulty) : base(identifier, difficulty)
        {
        }

        protected override BlockChainType BlockChainType => BlockChainType.Token;
    }

    public class WalletBlockChain : BlockChain
    {
        public WalletBlockChain()
        {
        }

        public WalletBlockChain(string identifier, long difficulty) : base(identifier, difficulty)
        {
        }

        protected override BlockChainType BlockChainType => BlockChainType.Wallet;

    }

    public abstract class BlockChain
    {
        public BlockChain()
        {

        }

        public BlockChain(string identifier, long difficulty)
        {
            Identifier = identifier;
            Difficulty = difficulty;

            Iv = KeyGenerator.GetRandomData(128);
            EncryptionKey = KeyGenerator.GetRandomData(256);
        }

        [Key]
        public Guid Id { get; set; }
        public string Identifier { get; set; }
        public byte[] EncryptionKey { get; set; }
        public byte[] Iv { get; set; }
        public long Difficulty { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ChainIndex { get; set; }
        public Guid BlockId { get; set; }
        public List<Block> Blocks { get; set; }

        protected abstract BlockChainType BlockChainType { get; } 

        public Block GenerateGenese(object data, string chainIdentifier)
        {
            return new Block(this, data, chainIdentifier, Difficulty, EncryptionKey, Iv);
        }

        public Block GenerateNextBlock(Block previous, string identifier)
        {
            return new Block(this, previous, null, identifier, Difficulty, EncryptionKey, Iv);
        }

        public Block GenerateNextBlock(Block previous, object data, string identifier)
        {
            return new Block(this, previous, data, identifier, Difficulty, EncryptionKey, Iv);
        }
    }

    public enum BlockChainType:short
    {
        Batch, Card, Token, Wallet
    }
}
