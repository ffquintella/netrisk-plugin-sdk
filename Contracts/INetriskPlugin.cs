using Serilog;

namespace Contracts;

public interface INetriskPlugin
{
    public string PluginName { get; } 
    public string PluginVersion { get; }
    public string PluginDescription { get; }
    
    public void Initialize(ILogger? logger);
    public void Dispose();
}