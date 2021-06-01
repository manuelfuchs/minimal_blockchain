namespace MinimalBlockchain.Api {
    internal sealed record Transaction(string Sender, string Recipient, int Amount);
}