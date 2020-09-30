using Newtonsoft.Json;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Abeer.Shared
{
    public class Block
    {
        public Block()
        {

        }

        internal Block(BlockChain blockChain, Block previous, object data, string chainIdentifier, long difficulty, byte[] encryptionKey, byte[] encryptionIv)
        {
            Id = Guid.NewGuid();
            Index = previous.Index + 1;
            Identifier = string.Concat(chainIdentifier, Index);
            BlockChainId = blockChain.Id;

            if (data != null)
                Data = Encoding.UTF8.GetString(JsonConvert.SerializeObject(data, Formatting.None,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }).Encrypted(encryptionKey, encryptionIv));

            TimeStamp = DateTime.UtcNow;

            Difficulity = difficulty;
            Nonce = GenerateNonce(difficulty);
            Hash = string.Concat(Nonce, ComputeHash(Id, Index, Identifier, Data, PreviousIndex, PreviousHash, TimeStamp));
        }

        private readonly static Random rdm = new Random();

        internal Block(BlockChain blockChain, object data, string chainIdentifier, long difficulty, byte[] encryptionKey, byte[] encryptionIv)
        {
            Id = Guid.NewGuid();
            Index = 0;
            BlockChainId = blockChain.Id;
            Identifier = string.Concat(chainIdentifier, Index);
            Data = Encoding.UTF8.GetString(JsonConvert.SerializeObject(data, Formatting.None,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }).Encrypted(encryptionKey, encryptionIv));
            TimeStamp = DateTime.UtcNow;
            Difficulity = difficulty;
            Nonce = GenerateNonce(difficulty);
            Hash = string.Concat(Nonce, ComputeHash(Id, Index, Identifier, Data, PreviousIndex, PreviousHash, TimeStamp));
        }

        private string GenerateNonce(long difficulty)
        {
            StringBuilder pinCode = new StringBuilder();

            for (int i = 0; i < difficulty; i++)
            {
                pinCode.Append(rdm.Next(0, 9));
            }

            return pinCode.ToString();
        }

        private string ComputeHash(Guid id, long index, string identifier, string data, long previousIndex, string previousHash, DateTime timeStamp)
        {
            return string.Concat(id, index, identifier, data, previousIndex, previousHash, timeStamp).Sha512().ToString();
        }

        [Key]
        public Guid Id { get; set; }
        public long Index { get; set; }
        public string Identifier { get; set; }
        public Guid BlockChainId { get; set; }
        [ForeignKey(nameof(BlockChainId))]
        public BlockChain BlockChain { get; set; }

        public string Data { get; set; }
        public string Hash { get; set; }
        public long PreviousIndex { get; set; }
        public string PreviousHash { get; set; }
        public DateTime TimeStamp { get; set; }
        public long Difficulity { get; set; }
        public string Nonce { get; set; }
    }
}