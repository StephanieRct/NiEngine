using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using NiEngine.Expressions.GameObjects;
using NiEngine.Expressions;

namespace NiEngine.Actions.Variables
{
    [Serializable, ClassPickerName("Var/Set")]
    public class SetNiVariable : Action
    {
        [SerializeReference, ObjectReferencePicker, EditorField(inline: true, showPrefixLabel: true, minWidth: 32)]
        public IExpressionGameObject On = new NiEngine.Expressions.GameObjects.Self();
        [Tooltip("Name of the variable to set")]
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32), NotSaved]
        public string Name;

        //[Tooltip("Set to ignore if variable not found")]
        //[EditorField(showPrefixLabel: true, inline: true, prefix: "/!\\")]
        //public bool IgnoreIfFails;


        [Tooltip("Value to set the variable to")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpression Value;

        public override void Act(Owner owner, EventParameters parameters)
        {
            var on = On.GetValue(owner, parameters);
            if (on == null)
            {
                //if (!IgnoreIfFails)
                {
                    parameters.LogError(this, owner, "Variable.Set.Fail", $"Could not set variable named '{Name}'. 'On' parameter cannot be null");
                }
                return;
            }
            var value = Value?.GetObjectValue(owner, parameters) ?? null;
            if (NiVariables.TrySetValue(on, Name, value))
                return;
            //if (!IgnoreIfFails)
            {
                parameters.LogError(this, owner, "Variable.Set.Fail", $"Could not set variable named '{Name}' on object '{on.GetNameOrNull()}'");
            }
        }
    }
    [Serializable, ClassPickerName("Var/List Add")]
    public class VarListAdd : ActionBool
    {
        [SerializeReference, ObjectReferencePicker, EditorField(inline: true, showPrefixLabel: true, minWidth: 32)]
        public IExpressionGameObject On = new NiEngine.Expressions.GameObjects.Self();
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32), NotSaved]
        public string List;


        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpression Value;
        [NotSaved]
        public bool Unique;

        // TODO Add override GetValue() with proper error log
        public override bool TryGetValue(Owner owner, EventParameters parameters, out bool value)
        {
            var on = On.GetValue(owner, parameters);
            if (on == null)
            {
                if (!IgnoreIfFails)
                    parameters.LogError(this, owner, "Variable.List.Add", $"Could not add to list named '{List}'. 'On' parameter cannot be null");
                value = default;
                return false;
            }
            var item = Value.GetObjectValue(owner, parameters);
            if (Unique)
                value = NiVariables.ListAddUnique(on, List, item);
            else
                value = NiVariables.ListAdd(on, List, item);
            return true;
        }
    }

    [Serializable, ClassPickerName("Var/List Remove")]
    public class VarListRemove : ActionBool
    {
        [SerializeReference, ObjectReferencePicker, EditorField(inline: true, showPrefixLabel: true, minWidth: 32)]
        public IExpressionGameObject On = new NiEngine.Expressions.GameObjects.Self();
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32), NotSaved]
        public string List;


        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpression Value;

        // TODO Add override GetValue() with proper error log
        public override bool TryGetValue(Owner owner, EventParameters parameters, out bool value)
        {
            var on = On.GetValue(owner, parameters);
            if (on == null)
            {
                if (!IgnoreIfFails)
                    parameters.LogError(this, owner, "Variable.List.Remove", $"Could not remove from list named '{List}'. 'On' parameter cannot be null");
                value = default;
                return false;
            }
            var item = Value.GetObjectValue(owner, parameters);
            value = NiVariables.ListRemove(on, List, item);
            return true;
        }
    }
    [Serializable, ClassPickerName("Var/List Remove At")]
    public class VarListRemoveAt : ActionBool
    {
        [SerializeReference, ObjectReferencePicker, EditorField(inline: true, showPrefixLabel: true, minWidth: 32)]
        public IExpressionGameObject On = new NiEngine.Expressions.GameObjects.Self();
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32), NotSaved]
        public string List;


        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: true)]
        public IExpressionInt Index = new NiEngine.Variables.IntNiVariable();

        bool TryGetOn(Owner owner, EventParameters parameters, out GameObject result)
        {
            result = On.GetValue(owner, parameters);
            if (result == null)
            {
                if (!IgnoreIfFails)
                    parameters.LogError(this, owner, "Variable.List.RemoveAt", $"Could not remove from list named '{List}'. 'On' parameter cannot be null");
                return false;
            }
            return true;
        }
        bool TryGetIndex(Owner owner, EventParameters parameters, out int result)
        {
            result = Index.GetValue(owner, parameters);
            return true;
        }
        // TODO Add override GetValue() with proper error log
        public override bool TryGetValue(Owner owner, EventParameters parameters, out bool value)
        {
            if (!TryGetOn(owner, parameters, out var on))
            {
                value = default;
                return false;
            }
            if (!TryGetIndex(owner, parameters, out var index))
            {
                value = default;
                return false;
            }
            value = NiVariables.ListRemoveAt(on, List, index);
            return true;
        }
    }


    [Serializable, ClassPickerName("Var/List Clear")]
    public class VarListClear : Action
    {
        [SerializeReference, ObjectReferencePicker, EditorField(inline: true, showPrefixLabel: true, minWidth: 32)]
        public IExpressionGameObject On = new NiEngine.Expressions.GameObjects.Self();
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32), NotSaved]
        public string List;
        [NotSaved]
        public bool IgnoreIfFails;
        public override void Act(Owner owner, EventParameters parameters)
        {
            var on = On.GetValue(owner, parameters);
            if (on == null)
            {
                if (!IgnoreIfFails)
                {
                    parameters.LogError(this, owner, "Variable.List.Clear", $"Could not clear list named '{List}'. 'On' parameter cannot be null");
                }
                return;
            }
            if (NiVariables.ListClear(on, List))
                return;
            if (!IgnoreIfFails)
            {
                parameters.LogError(this, owner, "Variable.List.Clear", $"Could not clear list named '{List}' on object '{on.GetNameOrNull()}'");
            }
        }
    }
}