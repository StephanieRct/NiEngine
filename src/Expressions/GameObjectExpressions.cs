using NiEngine.Expressions.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiEngine.Expressions.GameObjects
{
    public interface IExpressionGameObject : IExpression<GameObject> { }
    public interface IExpressionGameObjects : IExpressionEnumerable<GameObject> { }
    [Serializable, ClassPickerName("GameObject")] public class ConstExpressionGameObject : ConstExpression<GameObject>, IExpressionGameObject, IExpressionGameObjects
    {
        public override bool TrySetValue(object value)
        {
            if (value == null)
            {
                Value = null;
                return true;
            }
            switch (value)
            {
                case GameObject go:
                    Value = go;
                    return true;
                case Component c:
                    Value = c.gameObject;
                    return true;
            }
            Value = default;
            return false;
        }
    }
    [Serializable, ClassPickerName("List/GameObject")] public class ConstExpressionListGameObject : ConstExpressionList<GameObject>, IExpressionGameObjects 
    {
        public override bool TrySetValue(object value)
        {
            switch (value)
            {
                case List<GameObject> gos:
                    Value = new(gos);
                    return true;
                case IList l:
                    var eT = value.GetType().GenericTypeArguments?.FirstOrDefault();
                    if(eT != null && typeof(Component).IsAssignableFrom(eT))
                    {
                        Value = new(l.Count);
                        foreach(var e in l)
                            Value.Add((e as Component).gameObject);

                        return true;
                    }
                    break;
            }
            Value = default;
            return false;
        }

    }
    public abstract class ExpressionGameObject : Expression<GameObject>, IExpressionGameObject, IExpressionGameObjects { }
    public abstract class ExpressionGameObjects : ExpressionEnumerable<GameObject>, IExpressionGameObjects { }
    [Serializable, ClassPickerName("Var/GameObject")] public class NiVariableReferenceGameObject : NiVariableReference<GameObject>, IExpressionGameObject, IExpressionGameObjects { }
    [Serializable, ClassPickerName("Var/List/GameObject")] public class NiVariableReferenceGameObjects : NiVariableReferenceList<GameObject>, IExpressionGameObjects { }



    [Serializable, ClassPickerName("Self")]
    public class Self : ExpressionGameObject
    {
        public override bool IsConst => true;
        public override bool TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            value = parameters.Self;
            return true;
        }
        public override GameObject GetValue(Owner owner, EventParameters parameters) => parameters.Self;
    }

    [Serializable, ClassPickerName("Trigger")]
    public class Trigger : ExpressionGameObject
    {
        public override bool IsConst => true;
        public override bool TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            value = parameters.Current.TriggerObject;
            return true;
        }
        public override GameObject GetValue(Owner owner, EventParameters parameters) => parameters.Current.TriggerObject;
    }

    [Serializable, ClassPickerName("OnBegin/Trigger")]
    public class OnBeginTrigger : ExpressionGameObject
    {
        public override bool IsConst => true;
        public override bool TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            value = parameters.OnBegin.TriggerObject;
            return true;
        }
        public override GameObject GetValue(Owner owner, EventParameters parameters) => parameters.OnBegin.TriggerObject;
    }

    [Serializable, ClassPickerName("From")]
    public class From : ExpressionGameObject
    {
        public override bool IsConst => true;
        public override bool TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            value = parameters.Current.From;
            return true;
        }
        public override GameObject GetValue(Owner owner, EventParameters parameters) => parameters.Current.From;
    }

    [Serializable, ClassPickerName("OnBegin/From")]
    public class OnBeginFrom : ExpressionGameObject
    {
        public override bool IsConst => true;
        public override bool TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            value = parameters.OnBegin.From;
            return true;
        }
        public override GameObject GetValue(Owner owner, EventParameters parameters) => parameters.OnBegin.From;
    }

    //[Serializable, ClassPickerName("Object")]
    //public class Object : ExpressionGameObject
    //{
    //    [EditorField(inline: true, showPrefixLabel: false, minWidth: 100)]
    //    public GameObject GameObject;
    //    public override bool IsConst => true;
    //    public override bool TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
    //    {
    //        value = GameObject;
    //        return true;
    //    }
    //    public override GameObject GetValue(Owner owner, EventParameters parameters) => GameObject;
    //}

    //[Serializable, ClassPickerName("Objects", showPrefixLabel: false)]
    //public class Objects : ExpressionGameObjects
    //{
    //    [EditorField(inline: false, showPrefixLabel: false)]
    //    public List<GameObject> GameObjects;
    //    public override IEnumerable<GameObject> GetValues(Owner owner, EventParameters parameters) => GameObjects;
    //}

    [Serializable, ClassPickerName("Tag.Single", inline: true, showPrefixLabel: false)]
    public class TagSingle : ExpressionGameObject
    {
        [EditorField(showPrefixLabel: false, inline: true)]
        public string Tag;
        public bool IgnoreNotFound;
        public override bool IsConst => true;
        public override bool TryGetValue(Owner owner, EventParameters parameters, out GameObject value)
        {
            value = GameObject.FindGameObjectWithTag(Tag);
            if (value == null)
            {
                if (!IgnoreNotFound)
                    parameters.LogError(this, owner, "TagSingle.NotFound", $"Could not find GameObject with tag '{Tag}'");
                return false;
            }
            return true;
        }
        public override GameObject GetValue(Owner owner, EventParameters parameters)
        {
            TryGetValue(owner, parameters, out var value);
            return value;
        }
    }

}