namespace Contracts.Importers;

/// <summary>
/// What the host tells an importer about the run. Deliberately narrow: an importer parses a report
/// and returns records, so it needs no database handle, no services, and no way to reach anything
/// else in the host.
/// </summary>
public class ImportContext
{
    /// <summary>Original file name, when the report came from a file. Used in warning messages.</summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Whether informational (severity <see cref="NormalizedSeverity.None"/>) findings should be
    /// skipped. A full Nessus report is mostly informational plugin output, and importing it
    /// swamps the register.
    /// </summary>
    public bool IgnoreNegligible { get; set; } = true;

    /// <summary>The entity (tenant) the import is scoped to, when the caller is scoped.</summary>
    public int? EntityId { get; set; }

    /// <summary>The user who started the import.</summary>
    public int? UserId { get; set; }

    /// <summary>
    /// When the importer runs, in UTC. Passed in rather than read from the clock so a parse is
    /// deterministic and testable, and so every record from one import shares a timestamp.
    /// </summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Importer-specific options from the scanner's configuration, e.g. a severity-mapping
    /// override. Unknown keys are ignored.
    /// </summary>
    public Dictionary<string, string> Options { get; set; } = new();
}
