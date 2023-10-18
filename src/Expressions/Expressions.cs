using NiEngine.Expressions.GameObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiEngine.Expressions
{

    [NotSaved]
    public interface IExpressionBase : ICloneable
    {
        public bool IsConst => false;
    }
    public interface IExpression : IExpressionBase
    {
        public object GetObjectValue(Owner owner, EventParameters parameters)
        {
            if (TryGetObjectValue(owner, parameters, out var value))
                return value;
            parameters.LogError((NiReference)this, owner, "IExpression.GetObjectValue.Failed");
            return default;
        }
        public bool TryGetObjectValue(Owner owner, EventParameters parameters, out object value);
    }
    public interface IExpression<T> : IExpression
    {
        public T GetValue(Owner owner, EventParameters parameters)
        {
            if (TryGetValue(owner, parameters, out var value))
                return value;
            parameters.LogError((NiReference)this, owner, $"IExpression<{typeof(T).FullName}>.GetValue.Failed");
            return default;
        }
        public bool TryGetValue(Owner owner, EventParameters parameters, out T value);
        object IExpression.GetObjectValue(Owner owner, EventParameters parameters) => GetValue(owner, parameters);

        bool IExpression.TryGetObjectValue(Owner owner, EventParameters parameters, out object value)
        {
            if (TryGetValue(owner, parameters, out var t))
            {
                value = t;
                return true;
            }
            value = default;
            return true;
        }
    }
    public interface IExpressionEnumerable : IExpressionBase
    {
        public IEnumerable GetObjectValues(Owner owner, EventParameters parameters);
    }
    public interface IExpressionEnumerable<T> : IExpressionEnumerable
    {
        public IEnumerable<T> GetValues(Owner owner, EventParameters parameters)
            => GetObjectValues(owner, parameters).Cast<T>();
    }

    public interface IExpressionBool : IExpression<bool>, ICondition
    {
        bool ICondition.Pass(Owner owner, EventParameters parameters, bool logFalseResults) => GetValue(owner, parameters);
    }
    public interface IExpressionBools : IExpressionEnumerable<bool> { }

    public interface IExpressionInt : IExpression<int> { }
    public interface IExpressionInts : IExpressionEnumerable<int> { }
    public interface IExpressionFloat : IExpression<float> { }
    public interface IExpressionFloats : IExpressionEnumerable<float> { }
    public interface IExpressionString : IExpression<string> { }
    public interface IExpressionStrings : IExpressionEnumerable<string> { }
    public interface IExpressionVector3 : IExpression<Vector3> { }
    public interface IExpressionVector3s : IExpressionEnumerable<Vector3> { }
    public interface IExpressionQuaternion : IExpression<Quaternion> { }
    public interface IExpressionQuaternions : IExpressionEnumerable<Quaternion> { }
    public interface IExpressionNiTransform : IExpression<NiTransform> { }
    public interface IExpressionNiTransforms : IExpressionEnumerable<NiTransform> { }


    public interface IExpressionAny : IExpression<object>
        , IExpressionInt
        , IExpressionFloat
        , IExpressionString
        , IExpressionVector3
        , IExpressionQuaternion
        , IExpressionNiTransform
        , IExpressionGameObject
        , IExpressionInts
        , IExpressionFloats
        , IExpressionStrings
        , IExpressionVector3s
        , IExpressionQuaternions
        , IExpressionNiTransforms
        , IExpressionGameObjects
    {
        bool IExpression<int>.TryGetValue(Owner owner, EventParameters parameters, out int value)
        {
            if (TryGetObjectValue(owner, parameters, out var t) && t is int v)
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }

        bool IExpression<float>.TryGetValue(Owner owner, EventParameters parameters, out float value)
        {
            if (TryGetObjectValue(owner, parameters, out var t) && t is float v)
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }

        bool IExpression<string>.TryGetValue(Owner owner, EventParameters parameters, out string value)
        {
            if (TryGetObjectValue(owner, parameters, out var t) && t is string v)
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }

        bool IExpression<Vector3>.TryGetValue(Owner owner, EventParameters parameters, out Vector3 value)
        {
            if (TryGetObjectValue(owner, parameters, out var t) && t is Vector3 v)
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }

        bool IExpression<Quaternion>.TryGetValue(Owner owner, EventParameters parameters, out Quaternion value)
        {
            if (TryGetObjectValue(owner, parameters, out var t) && t is Quaternion v)
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }

        bool IExpression<NiTransform>.TryGetValue(Owner owner, EventParameters parameters, out NiTransform value)
        {
            if (TryGetObjectValue(owner, parameters, out var t) && t is NiTransform v)
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }
        bool IExpression<GameObject>.TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            if (TryGetObjectValue(owner, parameters, out var t))
            {
                if(t == null)
                {
                    value = null;
                    return true;
                }
                if(t is GameObject go)
                {
                    value = go;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
    public interface IExpressionAnys : IExpressionEnumerable<object>
        , IExpressionInts
        , IExpressionFloats
        , IExpressionStrings
        , IExpressionVector3s
        , IExpressionQuaternions
        , IExpressionNiTransforms
        , IExpressionGameObjects
    {
        IEnumerable<object> IExpressionEnumerable<object>.GetValues(Owner owner, EventParameters parameters)
            => GetObjectValues(owner, parameters).Cast<object>();
        IEnumerable<int> IExpressionEnumerable<int>.GetValues(Owner owner, EventParameters parameters)
            => GetObjectValues(owner, parameters).Cast<int>();
        IEnumerable<float> IExpressionEnumerable<float>.GetValues(Owner owner, EventParameters parameters)
            => GetObjectValues(owner, parameters).Cast<float>();
        IEnumerable<string> IExpressionEnumerable<string>.GetValues(Owner owner, EventParameters parameters)
            => GetObjectValues(owner, parameters).Cast<string>();
        IEnumerable<Vector3> IExpressionEnumerable<Vector3>.GetValues(Owner owner, EventParameters parameters)
            => GetObjectValues(owner, parameters).Cast<Vector3>();
        IEnumerable<Quaternion> IExpressionEnumerable<Quaternion>.GetValues(Owner owner, EventParameters parameters)
            => GetObjectValues(owner, parameters).Cast<Quaternion>();
        IEnumerable<NiTransform> IExpressionEnumerable<NiTransform>.GetValues(Owner owner, EventParameters parameters)
            => GetObjectValues(owner, parameters).Cast<NiTransform>();
    }


    public abstract class Expression<T> : NiReference, IExpression<T>, IExpressionEnumerable<T>
    {
        public virtual bool IsConst => false;
        public virtual T GetValue(Owner owner, EventParameters parameters)
        {
            if (TryGetValue(owner, parameters, out var value))
                return value;
            parameters.LogError((NiReference)this, owner, "Expression.TryGetValue.Failed");
            return default;
        }
        public IEnumerable<T> GetValues(Owner owner, EventParameters parameters)
        {
            yield return GetValue(owner, parameters);
        }

        public IEnumerable GetObjectValues(Owner owner, EventParameters parameters)
        {
            yield return GetValue(owner, parameters);
        }

        public abstract bool TryGetValue(Owner owner, EventParameters parameters, out T value);

    }

    public abstract class Expression : Expression<object>
    {
    }

    public abstract class ExpressionEnumerable<T> : NiReference, IExpressionEnumerable<T>
    {
        public abstract IEnumerable<T> GetValues(Owner owner, EventParameters parameters);
        public virtual IEnumerable GetObjectValues(Owner owner, EventParameters parameters) => GetValues(owner, parameters);
    }


    public abstract class ExpressionList<TList, T> : ExpressionEnumerable<T>, IExpression<TList>
    {
        public virtual bool IsConst => false;
        public virtual TList GetValue(Owner owner, EventParameters parameters)
        {
            if (TryGetValue(owner, parameters, out var value))
                return value;
            parameters.LogError(this, owner, "ExpressionList.TryGetValue.Failed");
            return default;
        }
        public abstract bool TryGetValue(Owner owner, EventParameters parameters, out TList value);
    }

    public class ConstExpression<T> : Expression<T>, INiVariable
    {
        [EditorField(showPrefixLabel: false, inline: true)]
        public T Value;
        public override bool IsConst => true;
        public override T GetValue(Owner owner, EventParameters parameters) => Value;
        public object GetValue() => Value;

        public override bool TryGetValue(Owner owner, EventParameters parameters, out T value)
        {
            value = Value;
            return true;
        }
        public virtual bool TrySetValue(object value)
        {
            if(value == null)
            {
                Value = default;
                return true;
            }
            if (value is T t)
            {
                Value = t;
                return true;
            }
            Value = default;
            return false;
        }
    }
    public class ConstExpressionList<T> : ExpressionList<List<T>, T>, INiVariable
    {
        [EditorField(showPrefixLabel: false, inline: false, header: false)]
        public List<T> Value;
        public override IEnumerable<T> GetValues(Owner owner, EventParameters parameters) => Value;

        public override IEnumerable GetObjectValues(Owner owner, EventParameters parameters) => Value;
        public object GetValue() => Value;
        public virtual bool TrySetValue(object value)
        {
            if (value is List<T> t)
            {
                Value = new(t);
                return true;
            }
            Value = default;
            return false;
        }

        public override bool TryGetValue(Owner owner, EventParameters parameters, out List<T> value)
        {
            value = Value;
            return true;
        }
    }

    public interface IComponentExpression<T> : IExpression<T>
        where T : Component
    {
        T GetComponent(NiReference location, Owner owner, EventParameters parameters)
        {
            if (GetValue(owner, parameters)?.TryGetComponent<T>(out var component) ?? false)
                return component;
            parameters.LogError(location, owner, $"IComponentExpression<{typeof(T).FullName}>.GetComponent.Failed");
            return default;
        }

    }
    public interface IComponentsExpression<T> : IExpressionEnumerable<T>
        where T : Component
    {

    }
    public class ComponentExpression<T> : Expression<T>, INiVariable//, IExpressionGameObject, IExpressionGameObjects
        where T : Component
    {
        [EditorField(showPrefixLabel: false, inline: true)]
        public T Value;

        public override bool IsConst => true;
        public override T GetValue(Owner owner, EventParameters parameters) => Value;

        public object GetValue() => Value;

        public override bool TryGetValue(Owner owner, EventParameters parameters, out T value)
        {
            value = Value;
            return true;
        }
        public virtual bool TrySetValue(object value)
        {
            if (value is T t)
            {
                Value = t;
                return true;
            }
            Value = default;
            return false;
        }
        //public object GetObjectValue(Owner owner, EventParameters parameters) => Value;
        //public bool TryGetObjectValue(Owner owner, EventParameters parameters, out object value)
        //{
        //    value = Value;
        //    return true;
        //}

        //IEnumerable<GameObject> IExpressionEnumerable<GameObject>.GetValues(Owner owner, EventParameters parameters)
        //    => GetValues(owner, parameters).Select(x=>x.gameObject);
    }

    public class ComponentExpressionList<T> : ExpressionList<List<T>, T>, INiVariable, IExpressionGameObjects
        where T : Component
    {
        [EditorField(showPrefixLabel: false, inline: false, header: false)]
        public List<T> Value = new();
        public override IEnumerable<T> GetValues(Owner owner, EventParameters parameters) => Value;

        public override IEnumerable GetObjectValues(Owner owner, EventParameters parameters) => Value;
        IEnumerable<GameObject> IExpressionEnumerable<GameObject>.GetValues(Owner owner, EventParameters parameters) 
            => Value.Select(x=>x.gameObject);
        public object GetValue() => Value;
        public virtual bool TrySetValue(object value)
        {
            if (value is List<T> t)
            {
                Value = new(t);
                return true;
            }
            Value = default;
            return false;
        }

        public override bool TryGetValue(Owner owner, EventParameters parameters, out List<T> value)
        {
            value = Value;
            return true;
        }
    }

    public class ComponentAsGameObjectExpression<T, TExpression> : NiReference, IExpressionGameObject, IExpressionGameObjects
        where T : Component
        where TExpression : IComponentExpression<T>

    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public TExpression Value;

        public IEnumerable GetObjectValues(Owner owner, EventParameters parameters)
        {
            if (Value != null)
            {
                var v = Value.GetValue(owner, parameters);
                if(v!=null)
                    yield return v.gameObject;
            }
        }

        public IEnumerable<GameObject> GetValues(Owner owner, EventParameters parameters)
        {
            if (Value != null)
            {
                var v = Value.GetValue(owner, parameters);
                if (v != null)
                    yield return v.gameObject;
            }
        }

        public bool TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            value = Value?.GetValue(owner, parameters)?.gameObject;
            return true;
        }
    }

    public class ComponentsAsGameObjectListExpression<T> : ExpressionList<List<GameObject>, GameObject>, IExpressionGameObjects
        where T : Component
    {
        [EditorField(showPrefixLabel: false, inline: false, header: false)]
        public List<T> Value = new();
        public override IEnumerable<GameObject> GetValues(Owner owner, EventParameters parameters) => Value.Select(x=>x.gameObject);

        public override IEnumerable GetObjectValues(Owner owner, EventParameters parameters) => Value.Select(x => x.gameObject);
        IEnumerable<GameObject> IExpressionEnumerable<GameObject>.GetValues(Owner owner, EventParameters parameters)
            => Value.Select(x => x.gameObject);
        public object GetValue() => Value.Select(x => x.gameObject).ToList();
        public virtual bool TrySetValue(object value)
        {
            if (value is List<GameObject> t)
            {
                Value = t.Select(x=>x.GetComponent<T>()).ToList();
                return true;
            }
            Value = default;
            return false;
        }

        public override bool TryGetValue(Owner owner, EventParameters parameters, out List<GameObject> value)
        {
            value = Value.Select(x => x.gameObject).ToList(); ;
            return true;
        }
    }
    public abstract class ExpressionBool : Expression<bool>, IExpressionBool { }
    public abstract class ExpressionBools : ExpressionEnumerable<bool>, IExpressionBools { }
    public abstract class ExpressionInt : Expression<int>, IExpressionInt { }
    public abstract class ExpressionInts : ExpressionEnumerable<int>, IExpressionInts { }
    public abstract class ExpressionFloat : Expression<float>, IExpressionFloat { }
    public abstract class ExpressionFloats : ExpressionEnumerable<float>, IExpressionFloats { }
    public abstract class ExpressionString : Expression<string>, IExpressionString { }
    public abstract class ExpressionStrings : ExpressionEnumerable<string>, IExpressionStrings { }
    public abstract class ExpressionVector3 : Expression<Vector3>, IExpressionVector3 { }
    public abstract class ExpressionVector3s : ExpressionEnumerable<Vector3>, IExpressionVector3s { }
    public abstract class ExpressionQuaternion : Expression<Quaternion>, IExpressionQuaternion { }
    public abstract class ExpressionQuaternions : ExpressionEnumerable<Quaternion>, IExpressionQuaternions { }
    public abstract class ExpressionNiTransform : Expression<NiTransform>, IExpressionNiTransform { }
    public abstract class ExpressionNiTransforms : ExpressionEnumerable<NiTransform>, IExpressionNiTransforms { }



    public abstract class ComponentOnGameObject<TResult, TComponent> : Expression<TResult>
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObject On = new Self();

        public virtual TResult GetValue(TComponent component, Owner owner, EventParameters parameters)
        {
            if (TryGetValue(component, owner, parameters, out var result))
                return result;
            else
                parameters.LogError(this, owner, $"ComponentOnGameObject<{typeof(TResult).FullName}, {typeof(TComponent).FullName}>.GetValue.Failed");
            return default;

        }
        public abstract bool TryGetValue(TComponent component, Owner owner, EventParameters parameters, out TResult value);
        public override TResult GetValue(Owner owner, EventParameters parameters)
        {
            if (On == null)
            {
                parameters.LogError(this, owner, $"Expressions.ComponentOnGameObject.{nameof(TComponent)}.On.Null");
                return default;
            }
            if (!On.TryGetValue(owner, parameters, out var on))
            {
                parameters.LogError(this, owner, $"Expressions.ComponentOnGameObject.{nameof(TComponent)}.On.TryGetValue.Failed");
                return default;
            }
            if (on == null)
            {
                parameters.LogError(this, owner, $"Expressions.ComponentOnGameObject.{nameof(TComponent)}.On.Value.Null");
                return default;
            }
            if (!on.TryGetComponent<TComponent>(out var component))
            {
                parameters.LogError(this, owner, $"Expressions.ComponentOnGameObject.{nameof(TComponent)}.MissingComponent");
                return default;
            }
            return GetValue(component, owner, parameters);
        }
        public override bool TryGetValue(Owner owner, EventParameters parameters, out TResult value)
        {
            if(On != null
                && On.TryGetValue(owner, parameters, out var on)
                && on != null
                && on.TryGetComponent<TComponent>(out var component))
            {
                return TryGetValue(component, owner, parameters, out value);
            }
            value = default;
            return false;
        }
    }

    [Serializable, ClassPickerName("IsNull")]
    public class IsNull : ExpressionBool
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpression Value;

        public override bool TryGetValue(Owner owner, EventParameters parameters, out bool value)
        {
            if (Value.TryGetObjectValue(owner, parameters, out var obj))
            {
                value = obj == null || obj.Equals(null);
                return true;
            }
            value = default;
            return false;
        }
    }






    [Serializable, ClassPickerName("IsStateActive")]
    public class IsStateActive : ExpressionBool
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObjects Target = new Self();

        public enum OperationEnum
        {
            All,
            Any,
            None,
        }
        [EditorField(inline: true)]
        public OperationEnum Operation;

        [Tooltip("List of state name separated by commas. prefix a state name with ! to inverse the condition for that state only")]
        [EditorField(inline: true)]
        public string State;


        public override bool TryGetValue(Owner owner, EventParameters parameters, out bool value)
        {
            if(Target == null)
            {
                value = default;
                return false;
            }
            foreach (var obj in Target.GetValues(owner, parameters))
            {
                bool not = false;
                char[] delimiterChars = { ' ', ',', '\t' };
                string[] stateCommands = State.Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var sc in stateCommands)
                {
                    string state;
                    if (sc.StartsWith("!"))
                    {
                        state = sc.Substring(1).Trim();
                        not = true;
                    }
                    else
                        state = sc;
                    var result = ReactionReference.HasReaction(obj, sc, onlyEnabled: true, onlyActive: true, ReactionReference.k_MaxLoop);
                    if (not) result = !result;
                    switch (Operation)
                    {
                        case OperationEnum.All:
                            if (!result)
                            {
                                //if (logFalseResults)
                                //    parameters.Log(owner, "ConditionActiveState.logFalseResults", $"Condition False: state '{sc}' on [{obj.GetNameOrNull()}] is false and operation is 'All'");
                                value = false;
                                return true;
                            }

                            break;
                        case OperationEnum.Any:
                            if (result)
                            {
                                value = true;
                                return true;
                            }
                            break;
                        case OperationEnum.None:
                            if (result)
                            {
                                //if (logFalseResults)
                                //    parameters.Log(owner, "ConditionActiveState.logFalseResults", $"Condition False: state '{sc}' on [{obj.GetNameOrNull()}] is true and operation is 'None'");
                                value = false;
                                return true;
                            }

                            break;
                    }

                }
                value = true;
                return true;
            }
            value = false;
            return true;
        }

        //public override bool Pass(Owner owner, EventParameters parameters, bool logFalseResults = false)
        //{
        //    foreach (var obj in Target.GetValues(owner, parameters))
        //    {
        //        bool not = false;
        //        char[] delimiterChars = { ' ', ',', '\t' };
        //        string[] stateCommands = State.Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var sc in stateCommands)
        //        {
        //            string state;
        //            if (sc.StartsWith("!"))
        //            {
        //                state = sc.Substring(1).Trim();
        //                not = true;
        //            }
        //            else
        //                state = sc;
        //            var result = ReactionReference.HasReaction(obj, sc, onlyEnabled: true, onlyActive: true, ReactionReference.k_MaxLoop);
        //            if (not) result = !result;
        //            switch (Operation)
        //            {
        //                case OperationEnum.All:
        //                    if (!result)
        //                    {
        //                        if (logFalseResults)
        //                            parameters.Log(owner, "ConditionActiveState.logFalseResults", $"Condition False: state '{sc}' on [{obj.GetNameOrNull()}] is false and operation is 'All'");
        //                        return false;
        //                    }

        //                    break;
        //                case OperationEnum.Any:
        //                    if (result)
        //                        return true;
        //                    break;
        //                case OperationEnum.None:
        //                    if (result)
        //                    {
        //                        if (logFalseResults)
        //                            parameters.Log(owner, "ConditionActiveState.logFalseResults", $"Condition False: state '{sc}' on [{obj.GetNameOrNull()}] is true and operation is 'None'");
        //                        return false;
        //                    }

        //                    break;
        //            }

        //        }
        //        return true;
        //    }
        //    return false;
        //}
    }
}