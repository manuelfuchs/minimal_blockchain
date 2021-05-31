using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MinimalBlockchain.Api
{
    internal record Block(int Index, DateTime Timestamp, List<Transaction> Transactions, int Proof, int PreviousHash)
    {
        public override int GetHashCode() =>
            SHA256.HashData(Convert.FromBase64String(this.ToString()))
                .Select(b => Convert.ToInt32($"{b:X2}"))
                .Sum();
    }
}