namespace Contracts;

public interface INetriskPlugin
{
    public string PluginName { get; } 
    public string PluginVersion { get; }
    public string PluginDescription { get; }
}