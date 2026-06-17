using System;
using UnityEngine.SceneManagement;

namespace SixStringSyn.RPGToolkit2D.Runtime.World
{
    public sealed class SceneTransitionService : ISceneTransitionService
    {
        public static SceneTransitionService Default { get; } = new SceneTransitionService();
        public event Action<SceneTransitionRequest> TransitionRequested;
        public void RequestTransition(SceneTransitionRequest request) { if (request == null || !request.IsValid) return; TransitionRequested?.Invoke(request); }
        public void LoadRequestedScene(SceneTransitionRequest request) { if (request != null && request.IsValid) SceneManager.LoadScene(request.TargetSceneName); }
    }
}
