using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MinimalBlockchain.Api
{
    internal static class BlockchainManager
    {
        public static int DoWork(int lastProof)
        {
            var proof = 0;
            while (IsProofInvalid(lastProof, proof))
                proof += 1;

            return proof;
        }

        public static bool IsChainValid(IList<Block> chain)
        {
            foreach (var (block, previousBlock) in chain.Skip(1).Select((b, i) => (b, chain[i - 1])))
            {
                Console.WriteLine(previousBlock);
                Console.WriteLine(block);
                Console.WriteLine("\n-------------\n");

                if (block.PreviousHash != block.GetSha256Hash() ||
                    IsProofInvalid(previousBlock.Proof, block.Proof))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsProofInvalid(int lastProof, int proof) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{lastProof}{proof}")))
                .Take(4)
                .All(digit => digit == '0');
    }
}