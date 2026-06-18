using System;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Runtime.Dialogue
{
    [Serializable]
    public sealed class DialogueCommand
    {
        [SerializeField] private string _name;
        [SerializeField] private string _argument;

        public DialogueCommand() { }

        public DialogueCommand(string name, string argument = null)
        {
            _name = name;
            _argument = argument;
        }

        public string Name => _name ?? string.Empty;
        public string Argument => _argument ?? string.Empty;

        public static DialogueCommand QuestEvent(string eventId) => new DialogueCommand("quest_event", eventId);
        public static DialogueCommand SetVariable(string key, string value) => new DialogueCommand("set_variable", $"{key}={value}");
        public static DialogueCommand GiveReward(string rewardId) => new DialogueCommand("reward", rewardId);
    }
}
