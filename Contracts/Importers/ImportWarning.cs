namespace Contracts.Importers;

/// <summary>
/// Something the importer could not fully handle. Every skipped or partially-parsed record must
/// produce one of these: an importer that silently drops rows is the classic way a scan appears to
/// import cleanly while a third of its findings never arrive.
/// </summary>
public class ImportWarning
{
    /// <summary>Where in the report — a row number, an XML path, a JSON pointer.</summary>
    public string? RecordReference { get; set; }

    public string Message { get; set; } = string.Empty;

    /// <summary>True when the record was dropped entirely; false when it was imported degraded.</summary>
    public bool Skipped { get; set; }

    public override string ToString() =>
        (Skipped ? "[skipped] " : "[warning] ") +
        (string.IsNullOrWhiteSpace(RecordReference) ? Message : $"{RecordReference}: {Message}");
}
