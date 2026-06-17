using System.Reflection;
using NUnit.Framework;
using SixStringSyn.RPGToolkit2D.Runtime.Interactions;
using SixStringSyn.RPGToolkit2D.Runtime.World;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Tests.Runtime.Interactions
{
    public sealed class InteractionAndSceneTransitionTests
    {
        [Test]
        public void InteractionQuerySelectsHighestPriorityInteractable()
        {
            var low = new TestInteractable("Talk", 1, true);
            var high = new TestInteractable("Open", 10, true);
            var disabled = new TestInteractable("Ignore", 20, false);
            Assert.That(InteractionQuery.Best(new IInteractable[] { low, disabled, high }), Is.SameAs(high));
        }

        [Test]
        public void DoorInteractionCreatesTransitionRequest()
        {
            var go = new GameObject("Door");
            var door = go.AddComponent<DoorInteraction>();
            Set(door, "_targetSceneName", "Dungeon");
            Set(door, "_targetSpawnPointId", "entrance");
            var request = door.CreateRequest();
            Assert.That(request.IsValid, Is.True);
            Assert.That(request.TargetSceneName, Is.EqualTo("Dungeon"));
            Assert.That(request.SpawnPointId, Is.EqualTo("entrance"));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void SceneTransitionServiceRaisesValidRequests()
        {
            var service = new SceneTransitionService();
            SceneTransitionRequest observed = null;
            service.TransitionRequested += request => observed = request;
            service.RequestTransition(new SceneTransitionRequest("Town", "north"));
            Assert.That(observed.TargetSceneName, Is.EqualTo("Town"));
        }

        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        private sealed class TestInteractable : IInteractable
        {
            private readonly bool _canInteract;
            public TestInteractable(string label, int priority, bool canInteract) { InteractionLabel = label; InteractionPriority = priority; _canInteract = canInteract; }
            public string InteractionLabel { get; }
            public int InteractionPriority { get; }
            public bool CanInteract(GameObject interactor) => _canInteract;
            public void Interact(GameObject interactor) { }
        }
    }
}
