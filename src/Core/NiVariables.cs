
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using NiEngine.IO;

namespace NiEngine
{

    [AddComponentMenu("NiEngine/NiVariables"), Save]
    public class NiVariables : MonoBehaviour, INiVariableContainer//, ISaveOverride
    {
        public static bool TryGetValue<T>(GameObject go, string variableName, out T value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
            {
                if (c.TryGetValue<T>(variableName, out value))
                {
                    return true;
                }
            }
            value = default;
            return false;
        }
        public static bool TryGetValue(GameObject go, string variableName, out object value)
        {
            foreach(var c in go.GetComponents<INiVariableContainer>())
            {
                if(c.TryGetValue(variableName, out value))
                {
                    return true;
                }
            }
            value = default;
            return false;
        }
        public static bool TrySetValue<T>(GameObject go, string variableName, T value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
            {
                if (c.TrySetValue<T>(variableName, value))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool TrySetValue(GameObject go, string variableName, object value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
            {
                if (c.TrySetValue(variableName, value))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ListAdd<T>(GameObject go, string variableName, T value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                        return l.Add(value) >= 0;
            return false;
        }
        public static bool ListAddUnique<T>(GameObject go, string variableName, T value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                        if(!l.Contains(value))
                            return l.Add(value) >= 0;
            return false;
        }
        public static bool ListClear(GameObject go, string variableName)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                        {
                            l.Clear();
                            return true;
                        }
            return false;
        }
        public static bool ListRemove<T>(GameObject go, string variableName, T value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                        if (l.Contains(value))
                        {
                            l.Remove(value);
                            return true;
                        }
                        else
                            return false;
            return false;
        }
        public static bool ListRemoveAt(GameObject go, string variableName, int index)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                    {
                        if (index < 0 || index >= l.Count)
                            return false;
                        l.RemoveAt(index);
                        return true;
                    }
            return false;
        }
        public static bool TryListGetAt(GameObject go, string variableName, int index, out object value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                    {
                        if (index < 0 || index >= l.Count)
                        {
                            value = default;
                            return false;
                        }
                        value = l[index];
                        return true;
                    }
            value = default;
            return false;
        }
        public static bool ListIsEmpty(GameObject go, string variableName)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                        return l.Count == 0;
            return true;
        }

        public static bool TryListIsEmpty(GameObject go, string variableName, out bool value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                    {
                        value = l.Count == 0;
                        return true;
                    }
            value = default;
            return false;
        }

        public static bool ListContains<T>(GameObject go, string variableName, T value)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                        return l.Contains(value);
            return false;
        }

        public static bool TryListContains<T>(GameObject go, string variableName, T value, bool result)
        {
            foreach (var c in go.GetComponents<INiVariableContainer>())
                if (c.TryGetValue(variableName, out var list))
                    if (list is IList l)
                    {
                        result = l.Contains(value);
                        return true;
                    }
            result = default;
            return false;
        }


        public bool AllowAddAtRunTime = false;
        [Serializable]
        public struct Variable
        {
            [EditorField(showPrefixLabel: true, inline:true, minWidth:100)]
            [NotSaved(isDebug:true)]
            public string Name;

            [EditorField(showPrefixLabel: true, inline: true)]
            [SerializeReference, ObjectReferencePicker(typeof(INiVariable))]
            [Save(SaveInPlace = true)]
            public INiVariable Value;
        }
        [Save(SaveInPlace = true)]
        public List<Variable> Variables;


        public bool TryGetValue<T>(string name, out T value)
        {
            var v = Variables.Find(x => x.Name == name);
            if(v.Value == null)
            {
                value = default;
                return true;
            }
            if(v.Value != null)
            {
                var vv = v.Value.GetValue();
                if(vv == null)
                {
                    value = default;
                    return true;
                }
                if (vv is T t)
                {
                    value = t;
                    return true;
                }
            }
            value = default;
            return false;
        }
        public bool TryGetValue(string name, out object value)
        {
            var v = Variables.Find(x => x.Name == name);
            if (v.Value == null)
            {
                value = default;
                return true;
            }
            if (v.Value != null)
            {
                value = v.Value.GetValue();
                return true;
            }
            value = default;
            return false;
        }

        public bool TrySetValue<T>(string name, T value)
        {
            var v = Variables.Find(x => x.Name == name);
            if (v.Value != null)
            {
                return v.Value.TrySetValue(value);
            }
            if (!AllowAddAtRunTime)
                return false;
            // add it
            Variables.Add(new Variable
            {
                Name = name,
                Value = new NiEngine.Variables.AnyNiVariable(value)
            });
            return true;
        }
        public bool TrySetValue(string name, object value)
        {

            var v = Variables.Find(x => x.Name == name);
            if (v.Value != null)
            {
                return v.Value.TrySetValue(value);
            }
            if (!AllowAddAtRunTime)
                return false;
            // add it
            Variables.Add(new Variable
            {
                Name = name,
                Value = new NiEngine.Variables.AnyNiVariable(value)
            });
            return true;
        }
        //public void Save(StreamContext context, IOutput io)
        //{
        //    io.SaveInPlace(context, "Variables", Variables);
        //}

        //public void Load(StreamContext context, IInput io)
        //{
        //    io.LoadInPlace(context, "Variables", ref Variables);
        //}
    }
}
