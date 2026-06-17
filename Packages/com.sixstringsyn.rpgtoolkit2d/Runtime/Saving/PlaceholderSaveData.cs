using System;
using System.Collections.Generic;

namespace SixStringSyn.RPGToolkit2D.Runtime.Saving
{
    [Serializable] public sealed class QuestSaveData { public List<string> activeQuestIds = new List<string>(); public List<string> completedQuestIds = new List<string>(); }
    [Serializable] public sealed class WorldStateSaveData { public List<string> flags = new List<string>(); public List<string> visitedSceneNames = new List<string>(); }
    [Serializable] public sealed class SceneStateSaveData { public string sceneName; public List<string> consumedObjectIds = new List<string>(); }
}
