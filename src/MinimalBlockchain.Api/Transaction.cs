namespace MinimalBlockchain.Api {
    internal record Transaction(string Sender, string Recipient, int Amount);
}