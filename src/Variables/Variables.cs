using NiEngine.Expressions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine.Variables
{
    [Serializable, ClassPickerName("Prim./Any")] public class AnyNiVariable : ConstExpression<object>, IExpressionAny, INiVariable
    {
        public AnyNiVariable() { }
        public AnyNiVariable(object value) 
        {
            Value = value;
        }
        object IExpression.GetObjectValue(Owner owner, EventParameters parameters)
            => GetValue(owner, parameters);
        bool IExpression.TryGetObjectValue(Owner owner, EventParameters parameters, out object value)
            => TryGetValue(owner, parameters, out value);
    }
    [Serializable, ClassPickerName("Prim./Bool")] public class BoolNiVariable : ConstExpression<bool>, IExpressionBool, INiVariable { }
    [Serializable, ClassPickerName("Prim./Int")] public class IntNiVariable : ConstExpression<int>, IExpressionInt, INiVariable { }
    [Serializable, ClassPickerName("Prim./Float")] public class FloatNiVariable : ConstExpression<float>, IExpressionFloat, INiVariable { }
    [Serializable, ClassPickerName("Prim./String")] public class StringNiVariable : ConstExpression<string>, IExpressionString, INiVariable { }
    [Serializable, ClassPickerName("Prim./Vector3")] public class Vector3NiVariable : ConstExpression<Vector3>, IExpressionVector3, INiVariable { }
    [Serializable, ClassPickerName("Prim./Quaternion")] public class QuaternionNiVariable : ConstExpression<Quaternion>, IExpressionQuaternion, INiVariable { }
    [Serializable, ClassPickerName("Prim./NiTransform")] public class NiTransformNiVariable : ConstExpression<NiTransform>, IExpressionNiTransform, INiVariable { }


    [Serializable, ClassPickerName("List/Any")] public class ListAnyNiVariable : ConstExpressionList<object>, IExpressionAnys, INiVariable { }
    [Serializable, ClassPickerName("List/Bool")] public class ListBoolNiVariable : ConstExpressionList<bool>, INiVariable { }
    [Serializable, ClassPickerName("List/Int")] public class ListIntNiVariable : ConstExpressionList<int>, INiVariable { }
    [Serializable, ClassPickerName("List/Float")] public class ListFloatNiVariable : ConstExpressionList<float>, INiVariable { }
    [Serializable, ClassPickerName("List/String")] public class ListStringNiVariable : ConstExpressionList<string>, INiVariable { }
    [Serializable, ClassPickerName("List/Vector3")] public class ListVector3NiVariable : ConstExpressionList<Vector3>, INiVariable { }
    [Serializable, ClassPickerName("List/Quaternion")] public class ListQuaternionNiVariable : ConstExpressionList<Quaternion>, INiVariable { }
    [Serializable, ClassPickerName("List/NiTransform")] public class ListNiTransformNiVariable : ConstExpressionList<NiTransform>, INiVariable { }
}