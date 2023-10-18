using NiEngine.Expressions.GameObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine.Expressions.Variables
{
    public class NiVariableReference<T> : Expression<T>
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObject On = new Self();
        [Tooltip("Name of the variable to get")]
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32, prefix: ":")]
        public string Name;
        //public bool IgnoreNotFound = false;

        public override bool TryGetValue(Owner owner, EventParameters parameters, out T value)
        {
            if (On == null)
            {
                value = default;
                return false;
            }
            if (!On.TryGetValue(owner, parameters, out var on))
            {
                value = default;
                return false;
            }
            if (on == null)
            {
                value = default;
                return false;
            }
            if (!NiVariables.TryGetValue<T>(on, Name, out value))
            {
                value = default;
                return false;
            }
            return true;
        }
        public override T GetValue(Owner owner, EventParameters parameters)
        {
            if (On == null)
            {
                parameters.LogError(this, owner, $"Expressions.Variable.{nameof(T)}.On.Null");
                return default;
            }
            if (!On.TryGetValue(owner, parameters, out var on))
            {
                parameters.LogError(this, owner, $"Expressions.Variable.{nameof(T)}.On.TryGetValue.Failed");
                return default;
            }
            if (on == null)
            {
                parameters.LogError(this, owner, $"Expressions.Variable.{nameof(T)}.On.Value.Null");
                return default;
            }
            if (!NiVariables.TryGetValue<T>(on, Name, out var value))
            {
                parameters.LogError(this, owner, $"Expressions.Variable.{nameof(T)}.On.Value.Null");
                return default;
            }
            return value;
        }

        //bool IExpression<object>.TryGetValue(Owner owner, EventParameters parameters, out object value)
        //{
        //    if(TryGetValue(owner, parameters, out var t))
        //    {
        //        value = t;
        //        return true;
        //    }
        //    value = default;
        //    return false;
        //}
        //object IExpression.GetObjectValue(Owner owner, EventParameters parameters)
        //{
        //    return GetValue(owner, parameters);
        //}
        //bool IExpression.TryGetObjectValue(Owner owner, EventParameters parameters, out object value)
        //{
        //    if (TryGetValue(owner, parameters, out var t))
        //    {
        //        value = t;
        //        return true;
        //    }
        //    value = default;
        //    return false;
        //}

    }

    public class NiVariableReferenceList<T> : ExpressionEnumerable<T>
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObject On = new Self();
        [Tooltip("Name of the variable to get")]
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32, prefix:":")]
        public string Name;
        //public bool IgnoreNotFound = false;

        public override IEnumerable<T> GetValues(Owner owner, EventParameters parameters)
        {
            if (On == null)
            {
                parameters.LogError(this, owner, $"Expressions.Variable.List.{nameof(T)}.On.Null");
                yield break;
            }
            if (!On.TryGetValue(owner, parameters, out var on))
            {
                parameters.LogError(this, owner, $"Expressions.Variable.List.{nameof(T)}.On.TryGetValue.Failed");
                yield break;
            }
            if (on == null)
            {
                parameters.LogError(this, owner, $"Expressions.Variable.List.{nameof(T)}.On.Value.Null");
                yield break;
            }
            
            if (!NiVariables.TryGetValue<List<T>>(on, Name, out var value))
            {
                parameters.LogError(this, owner, $"Expressions.Variable.List.{nameof(T)}.On.Value.Null");
                yield break;
            }
            if (value == null)
                yield break;
            foreach(var v in value)
                yield return v;
        }

    }


    public class NiVariableComponentReference<T> : NiVariableReference<T>, IComponentExpression<T>
        where T : Component
    {
        public virtual object GetObjectValue(Owner owner, EventParameters parameters)
            => GetValue(owner, parameters);
        public bool TryGetObjectValue(Owner owner, EventParameters parameters, out object value)
        {
            if (TryGetValue(owner, parameters, out T t))
            {
                value = t;
                return true;
            }
            value = default;
            return false;
        }
    }

    public class NiVariableComponentReferenceList<T> : NiVariableReferenceList<T>, IComponentsExpression<T>
        where T : Component
    {
    }

    [Serializable, ClassPickerName("Variable")]
    public class NiVariableReferenceAny : NiVariableReference<object>, IExpressionAny
    {
        public virtual object GetObjectValue(Owner owner, EventParameters parameters)
            => GetValue(owner, parameters);


        public bool TryGetObjectValue(Owner owner, EventParameters parameters, out object value)
            => TryGetValue(owner, parameters, out value);

    }

    [Serializable, ClassPickerName("List/Variable")]
    public class NiVariableReferenceAnys : NiVariableReferenceList<object>, IExpressionAnys 
    {
    }



    [Serializable, ClassPickerName("Var/List Get At")]
    public class VarListGetAt : Expression, IExpressionGameObject, IExpressionGameObjects
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObject On = new Self();
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32)]
        public string List;


        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: true)]
        public IExpressionInt Index = new NiEngine.Variables.IntNiVariable();

        //public bool IgnoreIfFails;


        bool TryGetOn(Owner owner, EventParameters parameters, out GameObject result, bool ignoreIfFails)
        {
            result = On.GetValue(owner, parameters);
            if (result == null)
            {
                if (!ignoreIfFails)
                    parameters.LogError(this, owner, "Variable.List.GetAt", $"Could not get from list named '{List}'. 'On' parameter cannot be null");
                return false;
            }
            return true;
        }
        bool TryGetIndex(Owner owner, EventParameters parameters, out int result, bool ignoreIfFails)
        {
            result = Index.GetValue(owner, parameters);
            return true;
        }

        public override object GetValue(Owner owner, EventParameters parameters)
        {
            if (TryGetOn(owner, parameters, out var on, true)
                && TryGetIndex(owner, parameters, out var index, true))
            {
                if (!NiVariables.TryListGetAt(on, List, index, out var value))
                    parameters.LogError(this, owner, "Variable.List.TryListGetAt.Failed");
                return value;
            }
            return default;
        }
        public override bool TryGetValue(Owner owner, EventParameters parameters, out object value)
        {
            if (TryGetOn(owner, parameters, out var on, false)
                && TryGetIndex(owner, parameters, out var index, false)
                && NiVariables.TryListGetAt(on, List, index, out value))
                return true;
            value = default;
            return false;
        }

        bool IExpression<GameObject>.TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            if(TryGetValue(owner, parameters, out var o))
            {
                if(o == null)
                {
                    value = null;
                    return true;
                }
                if(o is GameObject go)
                {
                    value = go;
                    return true;
                }
            }
            value = default;
            return false;
        }
        public object GetObjectValue(Owner owner, EventParameters parameters)
            => GetValue(owner, parameters);
        public bool TryGetObjectValue(Owner owner, EventParameters parameters, out object value)
            => TryGetValue(owner, parameters, out value);

        IEnumerable<GameObject> IExpressionEnumerable<GameObject>.GetValues(Owner owner, EventParameters parameters)
        {
            if (TryGetValue(owner, parameters, out var o))
            {
                if (o == null)
                    yield return null;
                if (o is GameObject go)
                    yield return go;
            }
        }
    }

    [Serializable, ClassPickerName("Var/List Is Empty")]
    public class VarListIsEmpty : ExpressionBool
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObject On = new Self();
        [EditorField(inline: true, showPrefixLabel: true, minWidth: 32)]
        public string List;

        bool TryGetOn(Owner owner, EventParameters parameters, out GameObject result, bool logFailure)
        {
            result = On.GetValue(owner, parameters);
            if (result == null)
            {
                if (logFailure)
                    parameters.LogError(this, owner, "Variable.List.IsEmpty", $"Could not remove from list named '{List}'. 'On' parameter cannot be null");
                return false;
            }
            return true;
        }
        // TODO Add override GetValue() with proper error log
        public override bool TryGetValue(Owner owner, EventParameters parameters, out bool value)
        {
            if (!TryGetOn(owner, parameters, out var on, logFailure: false))
            {
                value = default;
                return false;
            }
            return NiVariables.TryListIsEmpty(on, List, out value);
        }
    }

}