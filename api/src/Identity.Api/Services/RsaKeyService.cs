namespace Identity.Api.Services;

public class RsaKeyService : IHostedService
{
    public string? PrivateKey { get; private set; }
    public RSAParameters PrivateKeyParam { get; private set; }
    public string? PublicKey { get; private set; }
    public RSAParameters PublicKeyParam { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentDir = Directory.GetCurrentDirectory();
        var privateKeyPath = Path.Combine(currentDir, "private.pem");
        var publicKeyPath = Path.Combine(currentDir, "public.pem");
        if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath))
        {
            File.Delete(privateKeyPath);
            File.Delete(publicKeyPath);
            using var rsa = RSA.Create(2048);
            var privateKey = rsa.ExportRSAPrivateKeyPem();
            var publicKey = rsa.ExportRSAPublicKeyPem();
            cancellationToken.ThrowIfCancellationRequested();

            await File.WriteAllTextAsync(privateKeyPath, privateKey);
            await File.WriteAllTextAsync(publicKeyPath, publicKey);
            PrivateKey = privateKey;
            PrivateKeyParam = rsa.ExportParameters(true);
            PublicKey = publicKey;
            PublicKeyParam = rsa.ExportParameters(false);
        }
        else
        {
            PrivateKey = File.ReadAllText(privateKeyPath);
            PublicKey = File.ReadAllText(publicKeyPath);
            using var rsa = RSA.Create();
            rsa.ImportFromPem(PrivateKey);
            PrivateKeyParam = rsa.ExportParameters(true);
            PublicKeyParam = rsa.ExportParameters(false);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
