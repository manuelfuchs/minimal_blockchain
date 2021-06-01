using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MinimalBlockchain.Api
{
    internal sealed record Block(int Index, DateTime Timestamp, List<Transaction> Transactions, int Proof, string PreviousHash)
    {
        public string GetSha256Hash() => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(this.ToString())));
    }
}