using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MinimalBlockchain.Api
{
    internal sealed record Block(int Index, DateTime Timestamp, List<Transaction> Transactions, int Proof, string PreviousHash)
    {
        public string GetSha256Hash() => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(this.ToString())));

        public override string ToString()
        {
            return
                $"Block {{ Index: {this.Index}, Timestamp: {this.Timestamp}, Proof: {this.Proof}, PreviousHash: {this.PreviousHash}, " +
                $"Transactions: [ {string.Join(", ", this.Transactions.Select(t => t.ToString()))} ] }}";
        }
    }
}