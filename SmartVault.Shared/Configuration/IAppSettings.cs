namespace SmartVault.Shared.Configuration
{
    public interface IAppSettings
    {
        string DefaultConnection { get; }
        string DatabaseFileName { get; }
        string OutputFilePath { get; }
    }
}
