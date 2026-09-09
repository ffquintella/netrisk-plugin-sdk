namespace Contracts.Secrets;

/// <summary>
/// The outbound HTTP seam a plugin is handed, instead of creating its own <c>HttpClient</c>.
///
/// A plugin runs with the host's full authority — nothing in .NET confines it — so this is not a
/// sandbox. What it is: the only HTTP path that is subject to the host's SSRF policy, its timeouts
/// and its redaction, and the one that a test can fake. A secret-vault plugin that news up an
/// <c>HttpClient</c> can be pointed at <c>169.254.169.254</c> by a base URL an operator pasted in;
/// one that goes through this cannot, because the host applies the same policy it applies to every
/// other integration.
///
/// Deliberately minimal — no serialization policy, no retries, no auth. The plugin builds its own
/// <c>Authorization</c> header, because only the plugin knows what its vault expects.
/// </summary>
public interface IPluginHttpClient
{
    Task<PluginHttpResponse> SendAsync(PluginHttpRequest request, CancellationToken ct = default);
}

/// <summary>One outbound request from a plugin.</summary>
public class PluginHttpRequest
{
    public required string Method { get; init; }

    public required string Url { get; init; }

    /// <summary>Header name → value. The plugin's credential goes here.</summary>
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>Already-serialized body. Null for GET and DELETE.</summary>
    public string? Body { get; init; }

    public string ContentType { get; init; } = "application/json";

    /// <summary>Per-request timeout. A hung vault must not hold a NetRisk request open forever.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// One outbound response, including transport failures.
///
/// A connection that never got an answer is <see cref="StatusCode"/> 0 with
/// <see cref="TransportError"/> set, rather than a thrown exception: every caller has to handle "the
/// vault said no" and "the vault was unreachable" the same way, and modelling one as a return value
/// and the other as a throw doubles every call site.
/// </summary>
public class PluginHttpResponse
{
    public int StatusCode { get; init; }

    public string? Body { get; init; }

    /// <summary>Response headers, lower-cased names.</summary>
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>Transport-level failure message when <see cref="StatusCode"/> is 0.</summary>
    public string? TransportError { get; init; }

    public bool IsSuccess => StatusCode is >= 200 and < 300;
}
