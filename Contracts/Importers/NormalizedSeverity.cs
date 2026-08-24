namespace Contracts.Importers;

/// <summary>
/// NetRisk's severity scale. Every scanner has its own — CVSS bands, "Warning/Error/Note",
/// numeric 0-4, colour names — and each importer maps onto this one.
///
/// The tool's own value is preserved verbatim in <see cref="NormalizedFinding.RawSeverity"/>: a
/// mapping is a judgement call, and losing the input makes a wrong mapping impossible to audit or
/// re-derive later.
/// </summary>
public enum NormalizedSeverity
{
    /// <summary>Informational; not a defect. Nessus severity 0, SARIF "none".</summary>
    None = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
