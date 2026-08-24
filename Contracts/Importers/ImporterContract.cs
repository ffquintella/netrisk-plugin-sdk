namespace Contracts.Importers;

/// <summary>
/// The version of the importer contract an implementation was written against.
///
/// Third-party importers live in separately-compiled plugin assemblies, so a breaking change to
/// the shapes in this namespace cannot be caught at compile time — the host loads the old DLL and
/// discovers the mismatch at run time, usually as a confusing <c>MissingMethodException</c>.
/// Reporting the version explicitly lets the host refuse an incompatible importer with a clear
/// message instead.
/// </summary>
public static class ImporterContract
{
    /// <summary>
    /// Bumped only for breaking changes. Adding an optional property to
    /// <see cref="NormalizedFinding"/> is not one; removing or renaming a member is.
    /// </summary>
    public const int Version = 1;
}
