using Contracts.Ui;

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

    /// <summary>
    /// Whether this vault requires the application identity in
    /// <see cref="SecretVaultCredentials.AppId"/> — BastionVault's <c>app_id</c>, the identity the
    /// vault's policies are written against.
    ///
    /// Default-implemented as <c>false</c> rather than declared abstract: this member was added after
    /// the contract shipped, and a plugin already compiled against the earlier SDK must keep loading.
    /// A vault that needs an app id overrides it, and the host then marks the field required in the
    /// connection editor instead of letting the save succeed and every later call 401.
    /// </summary>
    bool RequiresAppId => false;

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

    /// <summary>
    /// The extra controls this plugin contributes to one of the host's screens, or empty for
    /// "render the screen as it already is".
    ///
    /// <para><b>Why the contract has this at all.</b> Every screen in NetRisk is the host's, and a
    /// plugin whose vault has a concept the contract did not foresee had nowhere to put it — so it
    /// encoded the concept inside a field the host treats as opaque, which is how a secret id
    /// acquires a grammar that the host stores, indexes and displays without knowing it exists. The
    /// rule this replaces it with: a screen specific to a plugin is declared by the plugin and
    /// rendered by the host.</para>
    ///
    /// <para>Pure, and called to build a form. No I/O, no credential — it may be called before any
    /// connection exists, which is the case on the connection editor. The options of a
    /// <see cref="PluginFieldKind.Choice"/> are not here for that reason; they come from
    /// <see cref="GetFieldOptionsAsync"/>, with a context.</para>
    ///
    /// <para>Default-implemented as empty rather than declared abstract, the precedent being
    /// <see cref="RequiresAppId"/>: a plugin compiled against an earlier SDK must keep loading, and
    /// its screens then render exactly as they do today.</para>
    /// </summary>
    IReadOnlyList<PluginFieldSpec> DescribeScreen(PluginScreen screen) => [];

    /// <summary>
    /// The options for one <see cref="PluginFieldKind.Choice"/> field this plugin declared.
    ///
    /// <para>By call and never in the declaration, because the answer depends on the credential: an
    /// environment-scoped role may read the environments its scope names and no others, and two
    /// connections served by this same plugin reach two vaults that answer differently.</para>
    ///
    /// <para>Metadata only, and the listing rule applies unchanged: an option must not be a secret
    /// or be derived from reading one. If a list can only be built by reading secrets, return
    /// nothing and declare the field with <see cref="PluginFieldSpec.AllowCustomValue"/> — an empty
    /// list an operator can type into is better than an audit-log entry per row they scroll past.
    /// </para>
    /// </summary>
    /// <exception cref="SecretVaultException">The vault refused or could not be reached.</exception>
    Task<IReadOnlyList<PluginFieldOption>> GetFieldOptionsAsync(SecretVaultContext context,
        PluginFieldQuery query, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PluginFieldOption>>([]);

    /// <summary>
    /// A stored reference written in a form this plugin no longer produces, rewritten into the one
    /// it does — or the same reference, unchanged, when there is nothing to rewrite.
    ///
    /// <para>The migration hook for exactly the situation <see cref="DescribeScreen"/> ends. A
    /// plugin that once had to encode a concept inside <see cref="VaultSecretReference.SecretId"/>
    /// is the only thing that knows that encoding; the host must not learn it. So the host hands the
    /// reference back and stores whatever comes out, on the read path for display and through an
    /// explicit console command for persistence.</para>
    ///
    /// <para>Pure and offline: no I/O, no vault call, no exception. A reference it does not
    /// recognise comes back as it went in. Returning the same instance is the correct answer for
    /// every plugin that never invented a grammar, which is why that is the default.</para>
    /// </summary>
    VaultSecretReference NormalizeReference(VaultSecretReference reference) => reference;
}
