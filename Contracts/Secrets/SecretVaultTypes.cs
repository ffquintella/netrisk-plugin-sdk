namespace Contracts.Secrets;

/// <summary>
/// What a secret-vault plugin needs to authenticate, supplied per call rather than held by the
/// plugin.
///
/// Per call, and not captured in <c>Initialize</c>, for two reasons. A plugin instance is created by
/// reflection on every lookup, so state on it is not reliably there next time; and an installation
/// may have more than one vault connection (a production vault and a staging one), which a plugin
/// that remembered "the" API key could not serve.
/// </summary>
public class SecretVaultCredentials
{
    /// <summary>The vault's API root, e.g. <c>https://vault.example.com</c>.</summary>
    public required string BaseUrl { get; init; }

    /// <summary>The API key the vault issued. The one mandatory credential.</summary>
    public required string ApiKey { get; init; }

    /// <summary>
    /// The machine identity the vault issued for the host NetRisk runs on, when the vault binds keys
    /// to a machine.
    ///
    /// Optional by design: a vault that does not do machine binding has nothing to put here, and
    /// requiring it would make this contract unimplementable for those. A plugin whose vault *does*
    /// require it should fail the connection test with a clear message rather than sending a request
    /// it knows will 401.
    /// </summary>
    public string? MachineId { get; init; }

    /// <summary>
    /// The application identity the vault knows this installation by, when the vault authorizes by
    /// application rather than (or as well as) by key — BastionVault's <c>app_id</c>, and the same
    /// idea as CyberArk's AppID.
    ///
    /// Optional for the same reason as <see cref="MachineId"/>: a vault that authorizes on the key
    /// alone has nothing to put here, and a plugin whose vault does need it declares
    /// <see cref="INetriskSecretVaultPlugin.RequiresAppId"/> so the host can refuse the connection at
    /// save time instead of shipping a 401 into a sync job.
    /// </summary>
    public string? AppId { get; init; }
}

/// <summary>
/// Everything a plugin is given for one operation: who to talk to, how to make the call, and where
/// to log.
/// </summary>
public class SecretVaultContext
{
    public required SecretVaultCredentials Credentials { get; init; }

    /// <summary>The host's HTTP seam. See <see cref="IPluginHttpClient"/> for why it is not the plugin's own.</summary>
    public required IPluginHttpClient Http { get; init; }
}

/// <summary>
/// One secret the API key can see, as the picker in the UI shows it — metadata only.
///
/// No value field, and that is the whole point of the type. Listing is what an administrator does
/// while choosing which secret a field should use; it must not be a way to exfiltrate every
/// credential in the vault through NetRisk's API. The value is fetched one reference at a time by
/// <see cref="INetriskSecretVaultPlugin.GetSecretAsync"/>, on the server, at the moment it is needed.
/// </summary>
public class VaultSecretDescriptor
{
    /// <summary>The vault's stable identifier for this secret. What NetRisk stores in place of the secret.</summary>
    public required string Id { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Folder, path or collection the secret lives in, for grouping in the picker. Null when the vault is flat.</summary>
    public string? Path { get; init; }

    public string? Description { get; init; }

    /// <summary>
    /// The named fields this secret carries, when the vault stores structured secrets — a database
    /// entry with <c>username</c> and <c>password</c>, say. Empty for a single-value secret, which is
    /// what the picker treats as "no field to choose".
    /// </summary>
    public IReadOnlyList<string> Fields { get; init; } = Array.Empty<string>();

    /// <summary>Vault-side version, when the vault versions secrets. Recorded so a rotation is visible in the log.</summary>
    public string? Version { get; init; }

    public DateTime? UpdatedAt { get; init; }
}

/// <summary>Points at one secret — and optionally one field of it — inside a vault.</summary>
public class VaultSecretReference
{
    public required string SecretId { get; init; }

    /// <summary>The field to read, for a structured secret. Null means the secret's single value.</summary>
    public string? Field { get; init; }
}

/// <summary>
/// A retrieved secret. Short-lived: the host copies the value into its obfuscated cache and drops
/// this object.
/// </summary>
public class VaultSecretValue
{
    public required string Value { get; init; }

    /// <summary>The version this value came from, when the vault reports one.</summary>
    public string? Version { get; init; }

    /// <summary>
    /// The vault's own opinion on how long this value may be cached, when it has one. The host takes
    /// the shorter of this and its configured TTL — a vault that says "30 seconds" knows something
    /// the host does not.
    /// </summary>
    public TimeSpan? MaxCacheAge { get; init; }
}

/// <summary>The outcome of a connection test, as the administrator sees it.</summary>
public class SecretVaultTestResult
{
    public required bool Success { get; init; }

    /// <summary>Human-readable outcome. Must never contain the API key or a secret value.</summary>
    public required string Message { get; init; }

    /// <summary>How many secrets the key can see, when the test enumerated them. Null when it did not.</summary>
    public int? VisibleSecretCount { get; init; }

    public static SecretVaultTestResult Ok(string message, int? count = null) =>
        new() { Success = true, Message = message, VisibleSecretCount = count };

    public static SecretVaultTestResult Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>
/// A vault operation failed in a way the plugin understood — bad credential, unknown secret,
/// rate limit.
///
/// Its message reaches the operator, so a plugin must not put the credential or the secret value in
/// it. Anything the plugin did *not* understand should be allowed to propagate as its own exception;
/// the host logs and wraps it.
/// </summary>
public class SecretVaultException : Exception
{
    public SecretVaultException(string message) : base(message) { }

    public SecretVaultException(string message, Exception inner) : base(message, inner) { }
}
