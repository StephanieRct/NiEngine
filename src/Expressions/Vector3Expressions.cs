using NiEngine.Expressions.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NiEngine.Expressions
{
    public interface IExpressionDirection : IExpressionVector3
    {

    }
    public enum AxisEnum
    {
        X,
        Y,
        Z,
        XN,
        YN,
        ZN
    }

    [Serializable, ClassPickerName("GameObject./Axis", inline: true, showPrefixLabel: false)]
    public class GameObjectAxis : ExpressionVector3 , IExpressionDirection
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObject On = new Self();

        [EditorField(showPrefixLabel: false, inline: true)]
        public AxisEnum Axis;

        public override bool TryGetValue(Owner owner, EventParameters parameters, out Vector3 value)
        {
            if (On.TryGetValue(owner, parameters, out var go))
            {
                value = Axis switch
                {
                    AxisEnum.X => go.transform.right,
                    AxisEnum.Y => go.transform.up,
                    AxisEnum.Z => go.transform.forward,
                    AxisEnum.XN => -go.transform.right,
                    AxisEnum.YN => -go.transform.up,
                    AxisEnum.ZN => -go.transform.forward,
                    _ => throw new NotImplementedException(),
                };
                return true;
            }
            value = default;
            return false;
        }
    }

    [Serializable, ClassPickerName("World Axis", inline: true, showPrefixLabel: false)]
    public class WorldAxis : ExpressionVector3, IExpressionDirection
    {
        [EditorField(showPrefixLabel: false, inline: true)]
        public AxisEnum Axis;
        public override bool TryGetValue(Owner owner, EventParameters parameters, out Vector3 value)
        {
            value = Axis switch
            {
                AxisEnum.X => Vector3.right,
                AxisEnum.Y => Vector3.up,
                AxisEnum.Z => Vector3.forward,
                AxisEnum.XN => -Vector3.right,
                AxisEnum.YN => -Vector3.up,
                AxisEnum.ZN => -Vector3.forward,
                _ => throw new NotImplementedException(),
            };
            value = default;
            return false;
        }
    }
    [Serializable, ClassPickerName("GameObject./Position", inline: true, showPrefixLabel: false)]
    public class GameObjectPosition : ExpressionVector3, IExpressionDirection
    {
        [SerializeReference, ObjectReferencePicker, EditorFieldThis]
        public IExpressionGameObject On = new Self();

        public override bool TryGetValue(Owner owner, EventParameters parameters, out Vector3 value)
        {
            if(On.TryGetValue(owner, parameters, out var go))
            {
                value = go.transform.position;
                return true;
            }
            value = default;
            return false;
        }
    }

    [Serializable, ClassPickerName("Trigger.Position", inline: true, showPrefixLabel: false)]
    public class CurrentTriggerPosition : ExpressionVector3
    {
        public override bool TryGetValue(Owner owner, EventParameters parameters, out Vector3 value)
        {
            value = parameters.Current.TriggerPosition;
            return true;
        }
    }
    [Serializable, ClassPickerName("OnBegin/Trigger.Position", inline: true, showPrefixLabel: false)]
    public class OnBeginTriggerPosition : ExpressionVector3
    {
        public override bool TryGetValue(Owner owner, EventParameters parameters, out Vector3 value)
        {
            value = parameters.OnBegin.TriggerPosition;
            return true;
        }
    }
}