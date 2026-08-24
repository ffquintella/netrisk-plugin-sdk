namespace Contracts.Importers;

/// <summary>
/// A plugin-supplied deduplication key function — the <c>Custom</c> strategy of the dedup engine.
///
/// The contract is a pure function on purpose. The host persists whatever key it returns and
/// compares keys for equality; a strategy that consulted the database or the clock would produce
/// keys that stop matching, which is the one failure mode a dedup engine cannot tolerate.
/// </summary>
public interface IDeduplicationStrategyPlugin : INetriskPlugin
{
    /// <summary>Stable identifier used in the per-scanner strategy chain configuration.</summary>
    string StrategyName { get; }

    /// <summary>
    /// The key two findings must share to be the same finding. Return null when this strategy has
    /// no opinion about <paramref name="finding"/> — the host then falls through to the next
    /// strategy in the chain.
    /// </summary>
    string? ComputeKey(NormalizedFinding finding);
}
