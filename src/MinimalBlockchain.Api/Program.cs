using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using MinimalBlockchain.Api;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

var nodeIdentifier = Guid.NewGuid().ToString().Replace("-", string.Empty);
var blockchain = new Blockchain();

app.MapPost("/transactions/new", async http =>
{
    var transaction = await http.Request.ReadFromJsonAsync<Transaction>();

    if (string.IsNullOrWhiteSpace(transaction?.Sender) ||
        string.IsNullOrWhiteSpace(transaction.Recipient) ||
        transaction.Amount <= 0)
    {
        http.Response.StatusCode = 400;
        await http.Response.WriteAsync("Missing values");
    }
    else
    {
        var index = blockchain.NewTransaction(transaction);

        http.Response.StatusCode = 201;
        await http.Response.WriteAsJsonAsync(new
        {
            Message = $"Transaction will be added to Block {index}"
        });
    }
});

app.MapGet("/mine", (Func<object>)(() =>
{
    var lastBlock = blockchain.LastBlock;
    var proof = blockchain.ProofOfWork(lastBlock.Proof);

    blockchain.NewTransaction(new Transaction(Sender: "0", Recipient: nodeIdentifier, Amount: 1));

    var previousHash = lastBlock.GetSha256Hash();
    var block = blockchain.NewBlock(proof, previousHash);

    return new
    {
        Message = "New Block Forged",
        Index = block.Index,
        Transactions = block.Transactions,
        Proof = block.Proof,
        previousHash = block.PreviousHash
    };
}));

app.MapGet("/chain", (Func<IEnumerable<Block>>)(() => blockchain.Chain));

await app.RunAsync();
