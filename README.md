# ABOUT

This SDK contains a set of tools to enable the use of plugins in NetRisk.

The main project are the contracts that are used to define the interfaces the plugins must implement.

# INSTALLATION
Just clone the repository and use it as submodule to your project.


# CONTRACTS

Every plugin implements `INetriskPlugin`. Beyond that it implements one *capability* interface per
thing it does, and the host discovers it by that interface:

| Capability | Interface | What the host uses it for |
|---|---|---|
| Model extension | `INetriskModelPlugin` | Domain model extension points |
| Face recognition | `INetriskFaceIDPlugin` | Biometric enrolment and verification |
| Vulnerability classification | `INetriskVulnerabilityClassificationPlugin` | Severity/classification overrides |
| Report import | `Contracts.Importers.INetriskVulnerabilityImporterPlugin` | Ingesting scanner reports |
| Deduplication | `Contracts.Importers.IDeduplicationStrategyPlugin` | Finding identity strategies |
| **Secret vault ("secret grab")** | **`Contracts.Secrets.INetriskSecretVaultPlugin`** | **Resolving a stored secret *reference* to a live credential** |

## Secret vault plugins

A secret vault plugin lets NetRisk store a *reference* to a credential — "secret `db-prod`, field
`password`, in the vault" — instead of the credential itself. The host resolves the reference at the
moment of use and caches the value in obfuscated memory for a short, host-controlled TTL.

Three rules an implementation has to follow:

1. **Be stateless.** The host instantiates the plugin per lookup and passes everything through
   `SecretVaultContext`. A field set in one call is not reliably there in the next.
2. **Use `context.Http`, never your own `HttpClient`.** That is the only path subject to the host's
   SSRF policy and timeouts, and the only one a test can fake. An operator can paste any base URL
   into a vault connection; going through the host is what stops that from becoming a request to the
   cloud metadata endpoint.
3. **Never log or return a credential.** `SecretVaultTestResult.Message` and
   `SecretVaultException.Message` are both shown to an operator.

Minimal implementation:

```csharp
public class MyVaultPlugin : INetriskSecretVaultPlugin
{
    public string PluginName => "MyVaultPlugin";
    public string PluginVersion => "1.0.0";
    public string PluginDescription => "Reads secrets from MyVault";

    public string VaultKind => "myvault";
    public bool RequiresMachineId => false;

    public void Initialize(ILogger? logger) { }
    public void Dispose() { }

    public async Task<SecretVaultTestResult> TestConnectionAsync(SecretVaultContext ctx, CancellationToken ct = default)
    {
        var secrets = await ListSecretsAsync(ctx, ct);
        return SecretVaultTestResult.Ok($"Reached MyVault.", secrets.Count);
    }

    public Task<IReadOnlyList<VaultSecretDescriptor>> ListSecretsAsync(SecretVaultContext ctx, CancellationToken ct = default) => /* ... */;

    public Task<VaultSecretValue> GetSecretAsync(SecretVaultContext ctx, VaultSecretReference reference, CancellationToken ct = default) => /* ... */;
}
```

The assembly name must end in `Plugin.dll` and the file goes in a subdirectory of the host's
`Plugins` folder — `Plugins/Secrets/` by convention for this capability.
