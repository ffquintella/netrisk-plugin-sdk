namespace Contracts.Importers;

/// <summary>
/// What an importer returns: the findings it parsed, plus an honest account of what it could not.
///
/// Note there are no counts of "created" or "updated" here — an importer does not persist anything,
/// so it cannot know. Those counts belong to the host's ingestion pipeline.
/// </summary>
public class ImportResult
{
    public List<NormalizedFinding> Findings { get; set; } = new();

    public List<ImportWarning> Warnings { get; set; } = new();

    /// <summary>The scanner and version the report identified itself as, when it did.</summary>
    public string? DetectedTool { get; set; }

    public string? DetectedToolVersion { get; set; }

    /// <summary>
    /// True when the report covers its target exhaustively — a full network scan, a complete
    /// dependency tree. Only a full scan may auto-close findings it no longer reports; treating a
    /// partial scan as full silently closes everything outside its slice, so this defaults to
    /// false and an importer must opt in.
    /// </summary>
    public bool IsFullScan { get; set; }

    /// <summary>When the scan itself ran, if the report says so (not when it was imported).</summary>
    public DateTime? ScanDate { get; set; }

    /// <summary>Records dropped for reasons that are not defects — informational findings filtered
    /// by <see cref="ImportContext.IgnoreNegligible"/>, for instance.</summary>
    public int FilteredCount { get; set; }

    public int SkippedCount => Warnings.Count(w => w.Skipped);

    public void AddWarning(string message, string? recordReference = null, bool skipped = false) =>
        Warnings.Add(new ImportWarning
        {
            Message = message,
            RecordReference = recordReference,
            Skipped = skipped
        });
}
