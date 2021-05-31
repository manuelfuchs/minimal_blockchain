using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MinimalBlockchain.Api
{
    internal class Blockchain
    {
        public List<Block> Chain { get; private set; }
        public List<Transaction> CurrentTransactions { get; private set; }

        public Block LastBlock
        {
            get => this.Chain[^1];
            set
            {
                this.Chain.Remove(this.LastBlock);
                this.Chain.Add(value);
            }
        }

        public Blockchain()
        {
            (this.Chain, this.CurrentTransactions) = (new List<Block>(), new List<Transaction>());

            this.newBlock(previousHash: 1, proof: 100);
        }

        public Block newBlock(int proof, int? previousHash = null)
        {
            this.Chain.Add(new Block(
                Index: this.Chain.Count + 1,
                Timestamp: DateTime.Now,
                Transactions: this.CurrentTransactions,
                Proof: proof,
                PreviousHash: previousHash ?? this.LastBlock.GetHashCode()));

            this.CurrentTransactions = new List<Transaction>();

            return this.LastBlock;
        }

        public void newTransaction(string sender, string recipient, int amount)
        {
            this.CurrentTransactions.Add(new Transaction(sender, recipient, amount));
            this.LastBlock = this.LastBlock with { Index = this.LastBlock.Index + 1 };
        }

        public int proofOfWork(int lastProof)
        {
            bool isProofInvalid(int lastProof, int proof) =>
                SHA256.HashData(Convert.FromBase64String($"{lastProof}{proof}"))
                    .Select(b => $"{b:X2}")
                    .Take(4)
                    .All(digit => digit == "0");

            var proof = 0;
            while (isProofInvalid(lastProof, proof))
                proof += 1;
            
            return proof;
        }
    }
}
