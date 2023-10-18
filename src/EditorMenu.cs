using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine
{
    public static class Formatting
    {
        public static string FormatShortName(MonoBehaviour mb) => FormatShortName(mb.GetNameOrNull());
        public static string FormatShortName(GameObject obj) => FormatShortName(obj.GetNameOrNull());
        public static string FormatShortName(string name)
        {
            if (name.Length > 32)
            {
                return $"{name.Substring(0, 15)}..{name.Substring(name.Length - 15),15}";
            }
            return name;// $"\"{name}\"{new string(' ', 32 - name.Length)}";
            //return $"\"{name}\"{new string(' ', 32 - name.Length)}";
        }
        public static string FormatHash(int hash)
        {
            return $"[{string.Format("0x{0:X8}", hash)}]";
        }
    }

    public static class DebugStates
    {

        public static bool DrawStatesGizmos;
        public static bool DrawStatesLabel;
        public static bool LogAllEvents;

        public static void LogReaction(string reactionName, EventParameters parameters)
        {
            if (LogAllEvents)
            {
                //Debug.Log($"[{Time.frameCount}] Reaction: \"{reactionName}\" : {parameters}");
                Debug.Log($"[{Time.frameCount}] [Obj:{Formatting.FormatShortName(parameters.Current.From)}] --------Reaction--> [Obj:{Formatting.FormatShortName(parameters.Self)}.{Formatting.FormatShortName(reactionName)}] Trigger: [{Formatting.FormatShortName(parameters.Current.TriggerObject)}]\r\n"
                    + $"Reaction: \"{reactionName}\" : {parameters}");
            }
        }
        public static void LogReactionOnBegin(string reactionName, EventParameters parameters)
        {
            if (LogAllEvents)
            {
                Debug.Log($"[{Time.frameCount}] [Obj:{Formatting.FormatShortName(parameters.Current.From)}] --OnBegin-Reaction--> [Obj:{Formatting.FormatShortName(parameters.Self)}.{Formatting.FormatShortName(reactionName)}] Trigger: [{Formatting.FormatShortName(parameters.Current.TriggerObject)}]\r\n"
                    + $"Reaction: \"{reactionName}\" : {parameters}");
            }
        }
        public static void LogReactionOnEnd(string reactionName, EventParameters parameters)
        {
            if (LogAllEvents)
            {
                Debug.Log($"[{Time.frameCount}] [Obj:{Formatting.FormatShortName(parameters.Current.From)}] --OnEnd---Reaction--> [Obj:{Formatting.FormatShortName(parameters.Self)}.{Formatting.FormatShortName(reactionName)}] Trigger: [{Formatting.FormatShortName(parameters.Current.TriggerObject)}]\r\n"
                    + $"Reaction: \"{reactionName}\" : {parameters}");
            }
        }
    }

    //[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Class)]
    public class DerivedClassPicker : PropertyAttribute
    {
        public System.Type BaseType;
        public bool ShowPrefixLabel;
        public DerivedClassPicker(System.Type baseType, bool showPrefixLabel = true)
        {
            BaseType = baseType;
            ShowPrefixLabel = showPrefixLabel;
        }
    }

    //[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Class)]
    public class ClassPickerName : System.Attribute 
    {
        public string Name;
        public bool ShowPrefixLabel;
        public bool Inline;
        public ClassPickerName(string name, bool showPrefixLabel = false, bool inline = false)
        {
            Name = name;
            ShowPrefixLabel = showPrefixLabel;
            Inline = inline;
        }
    }
}