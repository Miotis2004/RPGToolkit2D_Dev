namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    public interface IDialogueContext
    {
        bool TryGetValue(string key, out string value);
    }
}
