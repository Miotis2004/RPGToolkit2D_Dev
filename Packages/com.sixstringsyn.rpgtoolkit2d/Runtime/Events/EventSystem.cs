using System;
using System.Collections.Generic;
using SixStringSyn.RPGToolkit2D.Runtime.Core;
using SixStringSyn.RPGToolkit2D.Runtime.Foundation;
using SixStringSyn.RPGToolkit2D.Runtime.Inventory;
using SixStringSyn.RPGToolkit2D.Runtime.Items;
using SixStringSyn.RPGToolkit2D.Runtime.Quests;
using SixStringSyn.RPGToolkit2D.Runtime.World;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Events
{
    public enum RPGEventCommandType { ShowDialogue, MoveNPC, PlaySound, GiveItem, StartQuest, OpenShop, ChangeVariable, TeleportPlayer, Custom }
    public enum RPGEventVariableOperation { SetString, SetFlag, SetCounter, AddCounter, SetFloat, Clear }

    [Serializable]
    public sealed class RPGEventCommand
    {
        public RPGEventCommandType type = RPGEventCommandType.Custom;
        public string targetId;
        public string argument;
        public int amount = 1;
        public Vector3 vectorValue;
        public ItemDefinition item;
        public QuestDefinition quest;
        public AudioClip audioClip;
        public RPGEventVariableOperation variableOperation = RPGEventVariableOperation.SetString;
        public List<WorldStateCondition> conditions = new List<WorldStateCondition>();

        public bool ConditionsMet(WorldState state)
        {
            foreach (var condition in conditions)
            {
                if (condition != null && !condition.Evaluate(state)) return false;
            }
            return true;
        }
    }

    [CreateAssetMenu(fileName = "RPGEvent", menuName = "RPG Toolkit/Events/Event")]
    public sealed class RPGEventDefinition : RPGObject
    {
        [SerializeField] private bool _runOnce;
        [SerializeField] private List<WorldStateCondition> _conditions = new List<WorldStateCondition>();
        [SerializeField] private List<RPGEventCommand> _commands = new List<RPGEventCommand>();

        public bool RunOnce => _runOnce;
        public IReadOnlyList<WorldStateCondition> Conditions => _conditions;
        public IReadOnlyList<RPGEventCommand> Commands => _commands;

        public bool CanRun(WorldState state)
        {
            foreach (var condition in _conditions)
            {
                if (condition != null && !condition.Evaluate(state)) return false;
            }
            return true;
        }
    }

    public interface IEventDialogueService { void ShowDialogue(string dialogueId); }
    public interface IEventAudioService { void Play(AudioClip clip, string soundId = null); }
    public interface IEventShopService { void OpenShop(string shopId); }
    public interface IEventNpcMover { void MoveNpc(string npcId, Vector3 destinationOrDelta); }
    public interface IEventSceneTeleporter { void Teleport(string sceneName, string spawnPointId, Vector3 fallbackPosition); }
    public interface IEventCommandHandler { bool TryExecute(RPGEventCommand command, RPGEventContext context); }

    public sealed class RPGEventContext
    {
        public WorldState WorldState { get; set; }
        public InventoryContainer Inventory { get; set; }
        public QuestTracker Quests { get; set; }
        public IEventDialogueService Dialogue { get; set; }
        public IEventAudioService Audio { get; set; }
        public IEventShopService Shops { get; set; }
        public IEventNpcMover NpcMover { get; set; }
        public IEventSceneTeleporter Teleporter { get; set; }
        public List<IEventCommandHandler> CustomHandlers { get; } = new List<IEventCommandHandler>();
    }

    public sealed class RPGEventRunResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public int ExecutedCommands { get; private set; }
        public static RPGEventRunResult Ok(int count) => new RPGEventRunResult { Success = true, ExecutedCommands = count };
        public static RPGEventRunResult Fail(string message, int count = 0) => new RPGEventRunResult { Success = false, Message = message, ExecutedCommands = count };
    }

    public sealed class RPGEventRunner
    {
        private readonly RPGEventContext _context;
        public event Action<RPGEventDefinition, RPGEventCommand> CommandExecuted;

        public RPGEventRunner(RPGEventContext context) { _context = context ?? new RPGEventContext(); }

        public RPGEventRunResult Run(RPGEventDefinition definition)
        {
            if (definition == null) return RPGEventRunResult.Fail("Event definition is missing.");
            var state = _context.WorldState;
            var completionKey = $"event.{definition.Id.Value}.completed";
            if (definition.RunOnce && state != null && state.GetFlag(completionKey)) return RPGEventRunResult.Ok(0);
            if (!definition.CanRun(state)) return RPGEventRunResult.Fail("Event conditions were not met.");

            var executed = 0;
            foreach (var command in definition.Commands)
            {
                if (command == null || !command.ConditionsMet(state)) continue;
                if (!Execute(command)) return RPGEventRunResult.Fail($"Event command {command.type} could not be handled.", executed);
                executed++;
                CommandExecuted?.Invoke(definition, command);
            }
            if (definition.RunOnce && state != null) state.SetFlag(completionKey);
            return RPGEventRunResult.Ok(executed);
        }

        private bool Execute(RPGEventCommand command)
        {
            switch (command.type)
            {
                case RPGEventCommandType.ShowDialogue: _context.Dialogue?.ShowDialogue(command.targetId); return _context.Dialogue != null;
                case RPGEventCommandType.MoveNPC: _context.NpcMover?.MoveNpc(command.targetId, command.vectorValue); return _context.NpcMover != null;
                case RPGEventCommandType.PlaySound: _context.Audio?.Play(command.audioClip, command.targetId); return _context.Audio != null;
                case RPGEventCommandType.GiveItem: return command.item != null && _context.Inventory != null && _context.Inventory.Add(command.item, Math.Max(1, command.amount)) == Math.Max(1, command.amount);
                case RPGEventCommandType.StartQuest: return command.quest != null && _context.Quests != null && _context.Quests.StartQuest(command.quest) != null;
                case RPGEventCommandType.OpenShop: _context.Shops?.OpenShop(command.targetId); return _context.Shops != null;
                case RPGEventCommandType.ChangeVariable: return ApplyVariable(command);
                case RPGEventCommandType.TeleportPlayer: _context.Teleporter?.Teleport(command.argument, command.targetId, command.vectorValue); return _context.Teleporter != null;
                case RPGEventCommandType.Custom:
                    foreach (var handler in _context.CustomHandlers) if (handler != null && handler.TryExecute(command, _context)) return true;
                    return false;
                default: return false;
            }
        }

        private bool ApplyVariable(RPGEventCommand command)
        {
            var state = _context.WorldState;
            if (state == null || string.IsNullOrWhiteSpace(command.targetId)) return false;
            switch (command.variableOperation)
            {
                case RPGEventVariableOperation.SetString: state.SetString(command.targetId, command.argument); return true;
                case RPGEventVariableOperation.SetFlag: state.SetFlag(command.targetId, string.Equals(command.argument, "true", StringComparison.OrdinalIgnoreCase) || command.amount != 0); return true;
                case RPGEventVariableOperation.SetCounter: state.SetCounter(command.targetId, command.amount); return true;
                case RPGEventVariableOperation.AddCounter: state.AddCounter(command.targetId, command.amount); return true;
                case RPGEventVariableOperation.SetFloat: state.SetVariable(command.targetId, command.vectorValue.x); return true;
                case RPGEventVariableOperation.Clear: state.Clear(command.targetId); return true;
                default: return false;
            }
        }
    }
}
