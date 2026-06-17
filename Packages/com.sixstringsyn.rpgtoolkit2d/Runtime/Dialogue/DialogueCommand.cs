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
    }
}
