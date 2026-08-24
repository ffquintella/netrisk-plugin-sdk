namespace Contracts.Importers;

/// <summary>
/// One finding, normalized out of a scanner report and not yet persisted.
///
/// The field set is deliberately wider than what NetRisk's <c>vulnerabilities</c> table stores
/// today: deduplication needs a stable identity (<see cref="ToolUniqueId"/>, <see cref="RuleId"/>,
/// <see cref="Location"/>) and SLA tracking needs <see cref="FirstSeen"/>, so an importer that
/// drops them leaves those features with nothing to work from.
/// </summary>
public class NormalizedFinding
{
    /// <summary>The scanner that produced the finding — "nessus", "trivy", "semgrep".</summary>
    public string Tool { get; set; } = string.Empty;

    /// <summary>Version of the scanner, when the report states it. Diagnostic only.</summary>
    public string? ToolVersion { get; set; }

    /// <summary>
    /// The scanner's own stable identifier for this finding instance (Snyk issue id, Dependabot
    /// alert number, SARIF <c>guid</c>). When present it is the strongest dedup key there is,
    /// because the tool itself promises it is stable across runs.
    /// </summary>
    public string? ToolUniqueId { get; set; }

    /// <summary>
    /// The rule, plugin or check that fired — Nessus plugin id, Semgrep rule id, SARIF ruleId.
    /// Stable across runs for the same defect class, unlike the title, which vendors reword.
    /// </summary>
    public string? RuleId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    /// <summary>Remediation guidance as the tool phrased it.</summary>
    public string? Solution { get; set; }

    public NormalizedSeverity Severity { get; set; } = NormalizedSeverity.None;

    /// <summary>
    /// The severity exactly as the tool wrote it. Preserved so a mapping decision stays auditable
    /// and re-derivable — see <see cref="NormalizedSeverity"/>.
    /// </summary>
    public string? RawSeverity { get; set; }

    public List<string> Cves { get; set; } = new();

    public List<string> Cwes { get; set; } = new();

    public string? CvssVector { get; set; }

    public double? CvssBaseScore { get; set; }

    /// <summary>CVSS v3 vector, when the tool distinguishes it from an older v2 vector.</summary>
    public string? Cvss3Vector { get; set; }

    public double? Cvss3BaseScore { get; set; }

    public double? Cvss3TemporalScore { get; set; }

    public double? Cvss3ImpactScore { get; set; }

    /// <summary>The asset. Null for findings that have none (see <see cref="NormalizedHost"/>).</summary>
    public NormalizedHost? Host { get; set; }

    /// <summary>
    /// Where in the target the finding is — a file path with line, a URL path, a package
    /// coordinate. Carries the dedup identity for scanners that report no asset, and is the
    /// component GitLab calls the location fingerprint.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>The package/component and version for dependency scanners.</summary>
    public string? Component { get; set; }

    public string? ComponentVersion { get; set; }

    /// <summary>The version that fixes it, when the tool knows one.</summary>
    public string? FixedInVersion { get; set; }

    /// <summary>
    /// When the scanner first saw it, if the report says. Left null when it does not, and the host
    /// then uses the import time — but a report that carries a real first-seen date must not have
    /// it discarded, because the SLA clock starts there.
    /// </summary>
    public DateTime? FirstSeen { get; set; }

    public DateTime? LastSeen { get; set; }

    public DateTime? VulnerabilityPublicationDate { get; set; }

    public DateTime? PatchPublicationDate { get; set; }

    public bool? ExploitAvailable { get; set; }

    public string? ExploitCodeMaturity { get; set; }

    public string? ExploitabilityEasy { get; set; }

    public bool? ExploitedByScanner { get; set; }

    public string? ThreatIntensity { get; set; }

    public string? ThreatRecency { get; set; }

    public string? ThreatSources { get; set; }

    public double? VprScore { get; set; }

    /// <summary>External references, one per entry (CVE links, vendor advisories, xrefs).</summary>
    public List<string> References { get; set; } = new();

    /// <summary>
    /// The tool's own output for this finding — the matched line, the HTTP exchange, the plugin
    /// output. This is the evidence a triager reads to decide false-positive, so it is worth
    /// carrying even though it is bulky.
    /// </summary>
    public string? Evidence { get; set; }

    /// <summary>
    /// The tool's native status string, when the report carries one ("open", "fixed",
    /// "dismissed"). The host uses it to detect findings a scanner considers resolved.
    /// </summary>
    public string? RawStatus { get; set; }

    /// <summary>
    /// Tool-specific values that have no home among the normalized fields but that a
    /// deduplication strategy may need. Nessus's <c>risk_factor</c> is the motivating case: it is
    /// part of the hash NetRisk has always computed for Nessus findings, so a strategy that has to
    /// reproduce that hash byte-for-byte needs it, while nothing else in the model does.
    ///
    /// Not a general-purpose extension point — anything a second importer would also want belongs
    /// in a real property.
    /// </summary>
    public Dictionary<string, string> ToolFields { get; set; } = new();
}
