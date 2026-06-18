using SixStringSyn.RPGToolkit2D.Runtime.Dialogue;

namespace SixStringSyn.RPGToolkit2D.Runtime.World
{
    public sealed class WorldStateDialogueContext : IDialogueContext
    {
        private readonly WorldState _state;
        public WorldStateDialogueContext(WorldState state) { _state = state; }
        public bool TryGetValue(string key, out string value) { value = null; return _state != null && _state.TryGet(key, out value); }
    }
}
