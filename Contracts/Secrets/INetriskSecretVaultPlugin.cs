namespace Contracts.Secrets;

/// <summary>
/// A "secret grab" plugin: something that holds credentials on NetRisk's behalf, so that NetRisk
/// stores a <em>reference</em> to a secret instead of the secret.
///
/// The shape of the contract follows from that goal. Three operations and no more:
///
///  * <see cref="TestConnectionAsync"/> — does this API key work, and what can it see. Run when an
///    administrator saves a vault connection, so a typo is found then rather than at 3am when a
///    notification tries to send.
///  * <see cref="ListSecretsAsync"/> — metadata for everything the key can reach, so a person can
///    pick one from a list. Deliberately returns no values: see <see cref="VaultSecretDescriptor"/>.
///  * <see cref="GetSecretAsync"/> — one value, one reference, at the moment of use.
///
/// What is *not* here matters as much. No "write secret" — NetRisk is a consumer, and a plugin that
/// could write would make a compromised NetRisk able to rewrite the estate's credentials. No
/// "list all values" — that is an exfiltration primitive with a friendly name. No caching: the host
/// caches, obfuscated, with a TTL it controls, because a per-plugin cache is a per-plugin bug.
///
/// Implementations must be stateless. The host creates an instance per lookup (that is how
/// <c>PluginsService</c> works) and passes everything through <see cref="SecretVaultContext"/>, so a
/// field set in one call is not reliably there in the next.
/// </summary>
public interface INetriskSecretVaultPlugin : INetriskPlugin
{
    /// <summary>
    /// The vault product this plugin speaks to, e.g. <c>bastionvault</c>. Stable, lower-case, and
    /// distinct from <see cref="INetriskPlugin.PluginName"/>: the plugin name identifies the
    /// assembly, this identifies the protocol, and a stored secret reference carries this so that
    /// re-packaging the plugin under a new name does not orphan every reference in the database.
    /// </summary>
    string VaultKind { get; }

    /// <summary>
    /// Whether this vault requires the machine identity in
    /// <see cref="SecretVaultCredentials.MachineId"/>. The host uses it to mark the field required in
    /// the UI rather than letting a save succeed and every later call 401.
    /// </summary>
    bool RequiresMachineId { get; }

    /// <summary>Verifies the credential and reports what it can reach. Must not throw for a bad credential — report it.</summary>
    Task<SecretVaultTestResult> TestConnectionAsync(SecretVaultContext context, CancellationToken ct = default);

    /// <summary>
    /// Metadata for every secret the credential can read. Metadata only.
    /// </summary>
    /// <exception cref="SecretVaultException">The vault refused or could not be reached.</exception>
    Task<IReadOnlyList<VaultSecretDescriptor>> ListSecretsAsync(SecretVaultContext context,
        CancellationToken ct = default);

    /// <summary>
    /// Reads one secret value.
    /// </summary>
    /// <exception cref="SecretVaultException">
    /// The reference does not resolve, the credential no longer has access, or the vault could not be
    /// reached. The host turns this into a failed integration call with the message attached — never
    /// into a silent empty credential, which produces a 401 from a third party and an operator
    /// hunting the wrong problem.
    /// </exception>
    Task<VaultSecretValue> GetSecretAsync(SecretVaultContext context, VaultSecretReference reference,
        CancellationToken ct = default);
}
