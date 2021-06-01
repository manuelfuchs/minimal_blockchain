using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MinimalBlockchain.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<Blockchain>();
await using var app = builder.Build();

var nodeIdentifier = Guid.NewGuid().ToString().Replace("-", string.Empty);
var blockchain = app.Services.GetRequiredService<Blockchain>();

app.MapPost("/transactions/new", AddTransaction);
app.MapGet("/mine", (Func<object>)MineBlock);
app.MapGet("/chain", (Func<IEnumerable<Block>>)(() => blockchain.Chain));

await app.RunAsync();

async Task AddTransaction(HttpContext http) {
    var transaction = await http.Request.ReadFromJsonAsync<Transaction>();

    if (string.IsNullOrWhiteSpace(transaction?.Sender) ||
        string.IsNullOrWhiteSpace(transaction.Recipient) ||
        transaction.Amount <= 0)
    {
        http.Response.StatusCode = ((int)HttpStatusCode.BadRequest);
        await http.Response.WriteAsync("Missing values");
    }
    else
    {
        var index = blockchain.AddTransaction(transaction);

        http.Response.StatusCode = ((int)HttpStatusCode.Created);
        await http.Response.WriteAsJsonAsync(new
        {
            Message = $"Transaction will be added to Block {index}"
        });
    }
}

object MineBlock() {
{
    var lastBlock = blockchain.Chain.Last();
    var proof = BlockchainManager.DoWork(lastBlock.Proof);

    blockchain.AddTransaction(new Transaction(Sender: "0", Recipient: nodeIdentifier, Amount: 1));

    var previousHash = lastBlock.GetSha256Hash();
    var block = blockchain.AddNewBlock(proof, previousHash);

    return new
    {
        Message = "New Block Forged",
        Index = block.Index,
        Transactions = block.Transactions,
        Proof = block.Proof,
        previousHash = block.PreviousHash
    };
}