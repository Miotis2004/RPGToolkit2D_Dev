namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    public interface ISaveContributor
    {
        string SystemId { get; }
        string CaptureJson();
        void RestoreJson(string json);
    }

    public interface ISaveMigration
    {
        bool CanMigrate(string fromVersion, string toVersion);
        GameSaveData Migrate(GameSaveData saveData, string toVersion);
    }
}
