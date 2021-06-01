using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MinimalBlockchain.Api
{
    internal sealed class Blockchain
    {
        private List<Block> chain;
        private List<Transaction> currentTransactions;
        private List<Uri> nodes;

        private IHttpClientFactory clientFactory;

        public IReadOnlyCollection<Block> Chain => this.chain;

        public Blockchain(IHttpClientFactory clientFactory)
        {
            (this.chain, this.currentTransactions, this.nodes) = (new List<Block>(), new List<Transaction>(), new List<Uri>());
            this.clientFactory = clientFactory;

            this.AddNewBlock(previousHash: "1", proof: 100);
        }

        public Block AddNewBlock(int proof, string? previousHash = null)
        {
            this.chain.Add(new Block(
                Index: this.Chain.Count + 1,
                Timestamp: DateTime.Now,
                Transactions: this.currentTransactions.ToList(),
                Proof: proof,
                PreviousHash: previousHash ?? this.Chain.Last().GetSha256Hash()));

            this.currentTransactions.Clear();

            return this.Chain.Last();
        }

        public int AddTransaction(Transaction transaction)
        {
            this.currentTransactions.Add(transaction);
            return this.Chain.Last().Index + 1;
        }

        public async Task<bool> ResolveConflictsAsync()
        {
            using var client = this.clientFactory.CreateClient();

            List<Block>? longerChain = null;
            var longestChainLength = this.Chain.Count;

            foreach (var node in this.nodes)
            {
                using var response = await client.GetAsync($"{node}/chain");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var otherChain = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Block>>(await response.Content.ReadAsStreamAsync());
                    if (otherChain?.Count > longestChainLength && BlockchainManager.IsChainValid(otherChain))
                    {
                        longerChain = otherChain;
                        longestChainLength = otherChain.Count;
                    }
                }
            }

            var isResolved = longerChain != null;
            if (isResolved) {
                this.chain = longerChain;
            }

            return isResolved;
        }
    }
}
