namespace Contracts.Importers;

/// <summary>
/// The asset a finding was observed on, as the scanner reported it.
///
/// Network scanners give an IP and often a FQDN; code and dependency scanners give neither, and
/// their findings carry a <see cref="NormalizedFinding.Location"/> instead. Every field here is
/// therefore optional and the host tolerates a finding with no host at all.
/// </summary>
public class NormalizedHost
{
    public string? Ip { get; set; }

    public string? HostName { get; set; }

    public string? Fqdn { get; set; }

    public string? MacAddress { get; set; }

    public string? OperatingSystem { get; set; }

    /// <summary>Free-form scanner metadata about the asset, one <c>name:value</c> per line.</summary>
    public string? Properties { get; set; }

    /// <summary>
    /// The network service the finding sits on, when the scanner reports one. Kept on the host
    /// rather than the finding because that is the shape NetRisk persists (host → service →
    /// vulnerability).
    /// </summary>
    public string? ServiceName { get; set; }

    public string? Port { get; set; }

    public string? Protocol { get; set; }

    /// <summary>
    /// True when the report identifies no asset at all — SARIF from a code scanner, for instance.
    /// The host then attributes the finding to the import's target rather than inventing an asset.
    /// </summary>
    public bool IsEmpty =>
        string.IsNullOrWhiteSpace(Ip) &&
        string.IsNullOrWhiteSpace(HostName) &&
        string.IsNullOrWhiteSpace(Fqdn);
}
