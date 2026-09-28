namespace Contracts.Ui;

/// <summary>
/// A host screen a plugin may contribute controls to.
///
/// A closed enumeration, and deliberately so: the host renders a screen it was written to render,
/// and a plugin that names a screen this host does not know gets its declaration ignored rather
/// than producing a half-drawn dialog. A new member is added when the host grows a renderer for it,
/// never before.
/// </summary>
public enum PluginScreen
{
    /// <summary>"Select a secret from the vault" — the picker opened from a credential field.</summary>
    VaultSecretSelector = 0,

    /// <summary>The vault connection editor, beside <c>BaseUrl</c>, <c>MachineId</c> and <c>AppId</c>.</summary>
    VaultConnectionEditor = 1
}

/// <summary>
/// The control one declared field is rendered as.
///
/// Four kinds and no more. The list is short because every member is a commitment the host has to
/// honour on every screen it renders and in every client it grows, and because the cases that
/// motivated this contract — an environment, a namespace, a mount, a tenant — are a choice or a
/// name. A kind the host does not know makes the whole field invalid; see
/// <see cref="PluginFieldSpec"/>.
/// </summary>
public enum PluginFieldKind
{
    /// <summary>A single-line text box.</summary>
    Text = 0,

    /// <summary>
    /// A combo whose options come from
    /// <c>INetriskSecretVaultPlugin.GetFieldOptionsAsync</c> — never from the declaration.
    /// </summary>
    Choice = 1,

    /// <summary>A check box. The value is the literal <c>true</c> or <c>false</c>.</summary>
    Toggle = 2,

    /// <summary>An integer spinner. The value is the number written out in invariant culture.</summary>
    Number = 3
}

/// <summary>
/// How far the host checks a <see cref="PluginFieldKind.Text"/> value before letting an operator
/// commit it.
///
/// A deliberately tiny vocabulary rather than a regular expression. A pattern supplied by a plugin
/// and evaluated by the host on every keystroke is a denial of service against the host's UI thread
/// and its API, and the expressiveness buys little: the cases this contract exists for are closed
/// lists, where validity is membership. Whatever this cannot express, the plugin still enforces on
/// the read path — a declaration is an affordance for the operator, not a security boundary.
/// </summary>
public enum PluginFieldFormat
{
    /// <summary>Any text within the length bound.</summary>
    Any = 0,

    /// <summary>No whitespace anywhere in the value.</summary>
    NoWhitespace = 1,

    /// <summary>Letters, digits, <c>-</c>, <c>_</c> and <c>.</c> — a name, not a path.</summary>
    Identifier = 2
}

/// <summary>
/// One control a plugin contributes to a host screen: what it is, what it is called, and what
/// counts as a usable value.
///
/// <para><b>Data, never behaviour.</b> There is no markup here, no view type, no callback the host
/// invokes to draw anything. A plugin is third-party code running with the host's authority, and a
/// screen that came from it must not become a place where its code runs. The host reads these
/// records and renders them with its own controls.</para>
///
/// <para><b>And no values.</b> The options of a <see cref="PluginFieldKind.Choice"/> are fetched by
/// <c>GetFieldOptionsAsync</c> with the connection's context, because the list depends on the
/// credential: two connections served by the same plugin reach two vaults that offer different
/// environments. A list embedded here would be right for one of them.</para>
///
/// <para>The host bounds everything it renders — key shape, label and help length, field count per
/// screen, option count per fetch. A declaration that breaks a bound is dropped whole, with a
/// warning in the log, and the screen renders as it did before the plugin declared anything. That
/// is the failure mode to design for: a screen missing a control is recoverable, a screen the host
/// cannot draw is not.</para>
/// </summary>
public sealed class PluginFieldSpec
{
    /// <summary>
    /// The stable key this field's value is stored under.
    ///
    /// Lower-case, starting with a letter or digit, thereafter letters, digits, <c>_</c>, <c>.</c>
    /// or <c>-</c>, at most 32 characters. It is part of the stored reference, so renaming it
    /// orphans every value already saved under the old name — treat it as a column name and not as
    /// a label.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// The label shown beside the control, in the plugin's own words.
    ///
    /// Plain text and not a resource key: the host's localization is three <c>.resx</c> files it
    /// owns, and a plugin cannot add a key to them — a lookup that missed would render as the key
    /// name. Supply <see cref="LabelTranslations"/> for the cultures the plugin knows; the host
    /// falls back to this.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>One sentence under the control. Optional, and the place to say where a value comes from.</summary>
    public string? Help { get; init; }

    /// <summary>
    /// <see cref="Label"/> per culture, keyed by culture name (<c>pt-BR</c>, <c>en</c>). The host
    /// matches the current UI culture exactly, then its neutral parent, then falls back to
    /// <see cref="Label"/>.
    /// </summary>
    public IReadOnlyDictionary<string, string> LabelTranslations { get; init; } =
        new Dictionary<string, string>();

    /// <summary><see cref="Help"/> per culture, resolved the same way as <see cref="LabelTranslations"/>.</summary>
    public IReadOnlyDictionary<string, string> HelpTranslations { get; init; } =
        new Dictionary<string, string>();

    public PluginFieldKind Kind { get; init; } = PluginFieldKind.Text;

    /// <summary>
    /// Whether the screen refuses to commit without a value.
    ///
    /// This is what an environment-scoped credential needs: the vault refuses every read that names
    /// no environment, and the operator should learn that while choosing rather than at 3am from a
    /// permission-denied in a sync job.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// The value the control starts with. For a <see cref="PluginFieldKind.Choice"/> it must be one
    /// of the fetched options, or the host ignores it rather than pre-filling something the operator
    /// cannot see in the list.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>Applies to <see cref="PluginFieldKind.Text"/> only. Ignored for the other kinds.</summary>
    public PluginFieldFormat Format { get; init; } = PluginFieldFormat.Any;

    /// <summary>
    /// Upper bound on the value's length. The host clamps it to its own maximum, so a declaration
    /// cannot ask for an unbounded string to render.
    /// </summary>
    public int MaxLength { get; init; } = 128;

    /// <summary>
    /// For <see cref="PluginFieldKind.Choice"/>: whether the operator may also type a value that was
    /// not offered.
    ///
    /// False makes the list closed, which is the honest shape when the plugin can enumerate
    /// everything the vault will accept. True makes it a suggestion list, for a vault that cannot —
    /// and the plugin then has to reject a bad value on the read path, because nothing else can.
    /// </summary>
    public bool AllowCustomValue { get; init; }

    /// <summary>
    /// For <see cref="PluginFieldKind.Choice"/>: whether the option list depends on which secret is
    /// selected.
    ///
    /// True makes the host re-fetch whenever the selection changes, and false makes it fetch once
    /// per connection. Say false when it is true of the vault: a fetch per selection is a round trip
    /// per click in a picker somebody is scrolling through.
    /// </summary>
    public bool OptionsDependOnSecret { get; init; }
}

/// <summary>
/// One option of a <see cref="PluginFieldKind.Choice"/> field.
///
/// Metadata only, exactly as a listing is. The prohibition that governs
/// <c>VaultSecretDescriptor</c> governs this too: neither the value nor the label may be a secret
/// or be derived from one, and a plugin must not read a secret in order to populate a list. A
/// picker that reads is a picker that writes an access record in the vault's audit log for every row
/// an administrator scrolls past, and an option list that carries values is the exfiltration
/// primitive the listing contract was shaped to exclude.
/// </summary>
public sealed class PluginFieldOption
{
    /// <summary>What is stored when this option is chosen.</summary>
    public required string Value { get; init; }

    /// <summary>What the operator reads in the list. May equal <see cref="Value"/>.</summary>
    public required string Label { get; init; }

    /// <summary>A short clarification, when the label is not self-explanatory.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// What the host knows when it asks a plugin for one field's options: which screen, which field,
/// and what the operator has chosen so far.
///
/// The partial state is here so one choice can narrow another — and so the environments offered for
/// a secret can come from the mount that secret lives in, rather than from the union of every mount
/// the credential can see.
/// </summary>
public sealed class PluginFieldQuery
{
    public required PluginScreen Screen { get; init; }

    /// <summary>The <see cref="PluginFieldSpec.Key"/> whose options are being asked for.</summary>
    public required string FieldKey { get; init; }

    /// <summary>
    /// The secret currently selected, when the screen has a selection and something is selected.
    /// Null on the connection editor, and null on the picker before the operator picks a row.
    /// </summary>
    public string? SecretId { get; init; }

    /// <summary>
    /// The plugin's other fields as filled so far, keyed by <see cref="PluginFieldSpec.Key"/>.
    /// Partial by nature — a field the operator has not reached yet is simply absent.
    /// </summary>
    public IReadOnlyDictionary<string, string> Values { get; init; } =
        new Dictionary<string, string>();
}
