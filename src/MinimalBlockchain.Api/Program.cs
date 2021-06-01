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
var blockChain = new Blockchain();

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
        var index = blockChain.NewTransaction(transaction);

        http.Response.StatusCode = 201;
        await http.Response.WriteAsJsonAsync(new
        {
            Message = $"Transaction will be added to Block {index}"
        });
    }
});

app.MapGet("/mine", (Func<string>)(() => "Hello World!2"));

app.MapGet("/chain", (Func<IEnumerable<Transaction>>)(() => new[]{
    new Transaction("my", "you", 10),
    new Transaction("you", "my", 20)
}));

await app.RunAsync();
