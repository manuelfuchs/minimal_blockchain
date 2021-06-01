using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MinimalBlockchain.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("default_client")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        // enables to communicate with other blockchain nodes without certificate validation
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        return handler;
    });
builder.Services.AddSingleton<Blockchain>();
await using var app = builder.Build();

var nodeIdentifier = Guid.NewGuid().ToString().Replace("-", string.Empty);
var blockchain = app.Services.GetRequiredService<Blockchain>();

app.MapGet("/chain", (Func<IEnumerable<Block>>)(() => blockchain.Chain));
app.MapGet("/mine", (Func<object>)MineBlock);
app.MapPost("/transactions/new", AddTransaction);
app.MapPost("/nodes/register", RegisterNode);
app.MapGet("/nodes/resolve", (Func<Task<object>>)FindConsensus);

await app.RunAsync();

object MineBlock()
{
    var lastBlock = blockchain.Chain.Last();
    var proof = BlockchainManager.DoWork(lastBlock.Proof);

    blockchain.AddTransaction(new Transaction(Sender: "0", Recipient: nodeIdentifier, Amount: 1));

    var block = blockchain.AddNewBlock(proof, lastBlock.GetSha256Hash());

    // debug:
    BlockchainManager.IsChainValid(blockchain.Chain.ToList());

    return new
    {
        Message = "New Block Forged",
        Index = block.Index,
        Transactions = block.Transactions,
        Proof = block.Proof,
        previousHash = block.PreviousHash
    };
}

async Task AddTransaction(HttpContext context)
{
    var transaction = await context.Request.ReadFromJsonAsync<Transaction>();

    if (string.IsNullOrWhiteSpace(transaction?.Sender) ||
        string.IsNullOrWhiteSpace(transaction.Recipient) ||
        transaction.Amount <= 0)
    {
        context.Response.StatusCode = ((int)HttpStatusCode.BadRequest);
        await context.Response.WriteAsync("Missing values");
        return;
    }

    var index = blockchain.AddTransaction(transaction);

    context.Response.StatusCode = ((int)HttpStatusCode.Created);
    await context.Response.WriteAsJsonAsync(new { Message = $"Transaction will be added to Block {index}" });
}

async Task RegisterNode(HttpContext context)
{
    var nodes = await context.Request.ReadFromJsonAsync<IList<Uri>>();
    if (nodes == null || nodes.Count == 0)
    {
        context.Response.StatusCode = ((int)HttpStatusCode.BadRequest);
        await context.Response.WriteAsync("Error: Invalid data");
        return;
    }

    foreach (var node in nodes)
    {
        blockchain.Nodes.Add(node);
    }

    context.Response.StatusCode = ((int)HttpStatusCode.Created);
    await context.Response.WriteAsJsonAsync(new { Message = "New nodes have been added", TotalNodes = blockchain.Nodes });
}

async Task<object> FindConsensus()
{
    var wasBlockchainReplaced = await blockchain.ResolveConflictsAsync();
    return wasBlockchainReplaced
        ? new
        {
            Message = "Our chain was replaced",
            NewChain = blockchain.Chain
        }
        : new
        {
            Message = "Chain is authoritative",
            Chain = blockchain.Chain
        };
}