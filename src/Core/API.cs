using NiEngine.Expressions;
using NiEngine.Expressions.GameObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace NiEngine
{
    // Object that can be referenced to from save files
    // A IUidObject is unique within a NiBehaviour prefab or scene instance.
    // Finding a IUidObject from it's uid is done with NiBehaviour.TryFindUidObject(uid, out uidObject)
    public interface IUidObject
    {
        public Uid Uid { get; }

    }

    // Object that host nested IUidObject.
    public interface IUidObjectHost
    {
        public IEnumerable<IUidObject> Uids { get; }

    }


    [Save]
    public interface INiVariable : ICloneable
    {
        //public object GetValue(Owner owner, EventParameters parameters);
        //public bool TrySetValue(Owner owner, EventParameters parameters, object value);
        public object GetValue();
        //public T GetValue<T>();
        public bool TrySetValue(object value);
    }
    public interface INiVariableContainer
    {
        bool TryGetValue<T>(string name, out T value);
        bool TryGetValue(string name, out object value);

        bool TrySetValue<T>(string name, T value);
        bool TrySetValue(string name, object value);
    }





    /// <summary>
    /// Implement this interface to create your own custom conditions.
    /// You may also implement:
    ///     IInitialize : to do an initialization when the owner MonoBehaviour Start()
    ///     IUpdate : to do an update when the owner MonoBehaviour Update()
    ///     IStateObserver : to do observe the state owner of this condition
    /// </summary>
    [Save]
    public interface ICondition : ICloneable
    {
        bool Pass(Owner owner, EventParameters parameters, bool logFalseResults = false);
    }

    /// <summary>
    /// Implement this interface to create your own custom actions
    /// You may also implement:
    ///     IInitialize : to do an initialization when the owner MonoBehaviour Start()
    ///     IUpdate : to do an update when the owner MonoBehaviour Update()
    /// </summary>
    [Save]
    public interface IAction : ICloneable
    {
        void Act(Owner owner, EventParameters parameters);
    }

    /// <summary>
    /// Implement this interface to create your own custom action that begins and ends by its owner.
    /// For instance:
    ///     - A ReactOnKey StateAction on a key down (OnBegin) and the key down (OnEnd).
    ///     - A ReactOnFocus StateAction on getting focus (OnBegin) and losing focus (OnEnd)
    ///     - A ReactOnCollisionPair StateAction on collision enter (OnBegin) and collision exit (OnEnd)
    ///     - A Grabbable StateAction on grab (OnBegin) and on release (OnEnd)
    /// You may also implement:
    ///     IInitialize : to do an initialization when the owner MonoBehaviour Start()
    ///     IUpdate : to do an update when the owner MonoBehaviour Update()
    /// </summary>
    [Save]
    public interface IStateAction : ICloneable
    {
        void OnBegin(Owner owner, EventParameters parameters);
        void OnEnd(Owner owner, EventParameters parameters);
    }

    /// <summary>
    /// Derive this class to create your own custom conditions.
    /// You may also implement:
    ///     IInitialize : to do an initialization when the owner MonoBehaviour Start()
    ///     IUpdate : to do an update when the owner MonoBehaviour Update()
    ///     IStateObserver : to do observe the state owner of this condition
    /// </summary>
    [Serializable]
    public abstract class Condition : UidObject, ICondition
    {
        public abstract bool Pass(Owner owner, EventParameters parameters, bool logFalseResults = false);
    }
    /// <summary>
    /// Derive this class to create your own custom IAction that may be used as IStateAction as well
    /// You may also implement:
    ///     IInitialize : to do an initialization when the owner MonoBehaviour Start()
    ///     IUpdate : to do an update when the owner MonoBehaviour Update()
    /// </summary>
    [Serializable]
    public abstract class Action : UidObject, IAction, IStateAction
    {

        public abstract void Act(Owner owner, EventParameters parameters);
        void IStateAction.OnBegin(Owner owner, EventParameters parameters) => Act(owner, parameters);
        void IStateAction.OnEnd(Owner owner, EventParameters parameters) { }
    }
    public abstract class ActionBool : Action, IExpressionBool
    {
        [EditorField(addToEnd:true)]
        public bool IgnoreIfFails;
        public override void Act(Owner owner, EventParameters parameters)
        {
            if(!TryGetValue(owner, parameters, out var _))
                if(!IgnoreIfFails)
                    parameters.LogError(this, owner, $"ActionBool.{GetType().FullName}.Failed");
        }
        public abstract bool TryGetValue(Owner owner, EventParameters parameters, out bool value);

    }

    public abstract class ActionOnComponent<T> : Action
        where T : Component
    {

        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: true)] //, isPrefix:true, suffix:".")]
        public IExpressionGameObject On = new Self();
        public abstract void Act(T component, Owner owner, EventParameters parameters);
        public override void Act(Owner owner, EventParameters parameters)
        {
            var go = On.GetValue(owner, parameters);
            if(go == null)
            {
                parameters.LogError(this, owner, $"ActionOnComponent<{typeof(T).FullName}>.On.Null");
                return;
            }
            if(!go.TryGetComponent<T>(out var component))
            {
                parameters.LogError(this, owner, $"ActionOnComponent<{typeof(T).FullName}>.On.MissingComponent");
                return;
            }
            Act(component, owner, parameters);
        }
    }
    /// <summary>
    /// Derive this class to create your own custom IStateAction that can be used as an IAction as well
    /// For instance:
    ///     - A ReactOnKey StateAction on a key down (OnBegin) and the key down (OnEnd).
    ///     - A ReactOnFocus StateAction on getting focus (OnBegin) and losing focus (OnEnd)
    ///     - A ReactOnCollisionPair StateAction on collision enter (OnBegin) and collision exit (OnEnd)
    ///     - A Grabbable StateAction on grab (OnBegin) and on release (OnEnd)
    /// You may also implement:
    ///     IInitialize : to do an initialization when the owner MonoBehaviour Start()
    ///     IUpdate : to do an update when the owner MonoBehaviour Update()
    /// </summary>
    [Serializable]
    public abstract class StateAction : UidObject, IStateAction//, IAction
    {
        //void IAction.Act(Owner owner, EventParameters parameters)
        //{
        //    OnBegin(owner, parameters);
        //    OnEnd(owner, parameters);
        //}
        public abstract void OnBegin(Owner owner, EventParameters parameters);
        public abstract void OnEnd(Owner owner, EventParameters parameters);
    }

    /// <summary>
    /// Implement by Condition, Action and StateAction to do an 
    /// initialization when the owner MonoBehaviour Start()
    /// </summary>
    public interface IInitialize
    {
        void Initialize(Owner owner);
    }

    /// <summary>
    /// Implement by Condition, Action and StateAction to do an 
    /// update when the owner MonoBehaviour Update()
    /// </summary>
    public interface IUpdate : ICloneable
    {
        // return if should still be updated next frame
        bool Update(Owner owner, EventParameters parameters);
    }
    public interface ILateUpdate : ICloneable
    {
        // return if should still be updated next frame
        bool LateUpdate(Owner owner, EventParameters parameters);
    }
    public interface IFixedUpdate : ICloneable
    {
        // return if should still be updated next frame
        bool FixedUpdate(Owner owner, EventParameters parameters);
    }
    public interface IUpdatePhase : IUpdate, ILateUpdate, IFixedUpdate
    {
        UpdatePhase ActiveUpdatePhaseFlags { get; }
    }

    /// <summary>
    /// Implement by Condition, Action and StateAction to 
    /// observe when the owner state begins and ends
    /// </summary>
    public interface IStateObserver
    {
        void OnStateBegin(Owner owner, EventParameters parameters);
        void OnStateEnd(Owner owner, EventParameters parameters);
    }
    public struct StateObserver : IStateObserver
    {
        Action<Owner, EventParameters> OnBegin;
        Action<Owner, EventParameters> OnEnd;

        public StateObserver(Action<Owner, EventParameters> onBegin)
        {
            OnBegin = onBegin;
            OnEnd = null;
        }
        public StateObserver(Action<Owner, EventParameters> onBegin, Action<Owner, EventParameters> onEnd)
        {
            OnBegin = onBegin;
            OnEnd = onEnd;
        }
        void IStateObserver.OnStateBegin(Owner owner, EventParameters parameters)
        {
            OnBegin?.Invoke(owner, parameters);
        }
        void IStateObserver.OnStateEnd(Owner owner, EventParameters parameters)
        {
            OnEnd?.Invoke(owner, parameters);
        }
    }


    public class NiReference : ICloneable
    {
        public virtual object Clone() => this.DeepClone();
    }

    [Serializable]
    public struct NiTransform
    {
        [Flags]
        public enum SetBitMask
        {
            HasPosition = 1,
            HasScale = 2,
            HasRotation = 4,
        };
        public SetBitMask SetMask;
        public Vector3 Position;
        public Vector3 Scale;
        public Quaternion Rotation;
        public NiTransform(Transform trf, bool position, bool rotation, bool scale)
        {
            SetMask = default;
            if (position)
            {
                SetMask = SetMask | SetBitMask.HasPosition;
                Position = trf.position;
            }
            else
                Position = default;
            if (scale)
            {
                SetMask = SetMask | SetBitMask.HasScale;
                Scale = trf.lossyScale;
            }
            else
                Scale = default;
            if (rotation)
            {
                SetMask = SetMask | SetBitMask.HasRotation;
                Rotation = trf.rotation;
            }
            else
                Rotation = default;
        }
        public NiTransform(RigidTransform trf)
        {
            Position = trf.pos;
            Rotation = trf.rot;
            Scale = default;
            SetMask = SetBitMask.HasPosition | SetBitMask.HasRotation;
        }
        public bool HasPosition => (SetMask & SetBitMask.HasPosition) != 0;
        public bool HasScale => (SetMask & SetBitMask.HasScale) != 0;
        public bool HasRotation => (SetMask & SetBitMask.HasRotation) != 0;


        // Does not preserve scale
        public RigidTransform AsRigidTransform => math.RigidTransform((quaternion)Rotation, (float3)Position);

        public static NiTransform RigidTransformOf(Transform trf)
        {
            return new NiTransform(trf, true, true, false);
        }
        public static NiTransform RigidDifference(NiTransform from, NiTransform to)
        {
            var inversedFrom = math.inverse(from.AsRigidTransform);
            return new NiTransform(math.mul(inversedFrom, to.AsRigidTransform));
        }
        public static NiTransform RigidDifference(Transform from, Transform to)
            => RigidDifference(RigidTransformOf(from), RigidTransformOf(to));

        public static NiTransform AddTransform(NiTransform @base, NiTransform add)
        {
            if (add.SetMask == 0) return @base;
            return new NiTransform(math.mul(@base.AsRigidTransform, add.AsRigidTransform));
        }
    }

    public interface IReactionReceiver
    {
        public bool ReactEnabled { get; }
        public int React(string reactionName, EventParameters parameters);
        //public bool HasReaction(string reactionName, int maxLoop);
        public bool HasReaction(string reactionName, bool onlyEnabled, bool onlyActive, int maxLoop);
        //public bool HasActiveReaction(string reactionName, bool onlyEnabled, int maxLoop);
        //public ReactionActiveSum ActiveReactionSum(string reactionName, bool onlyEnabled, int maxLoop);
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class SaveAttribute : Attribute
    {
        public bool SaveInPlace = false;
        public SaveAttribute() { }
        public SaveAttribute(bool saveInPlace)
        {
            SaveInPlace = saveInPlace;
        }
    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface)]
    public class NotSavedAttribute : Attribute
    {
        public bool IsDebug = false;
        public NotSavedAttribute() { }
        public NotSavedAttribute(bool isDebug) 
        {
            IsDebug = isDebug;
        }

    }
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property)]
    public class SaveBreakpointAttribute : Attribute
    {
    }
    //public enum VariableType
    //{
    //    String,
    //    Int,
    //    Float,
    //    Vector3,
    //    GameObject,
    //    ManagedType,
    //}
    //public enum UpdatePhase
    //{
    //    Immediate = 0,
    //    FixedUpdate = 1,
    //    Update = 2,
    //    LateUpdate = 4,
    //}
    public enum UpdatePhase
    {
        Immediate = 0,
        FixedUpdate = 1,
        Update = 2,
        LateUpdate = 4,
    }

    public enum SnapMethod
    {
        None,
        Move,
        Parent,
        FixedJoint,
    }

    public static class Move
    {

        public enum Method
        {
            None,
            RigidbodyMove,
            RigidbodySet,
            TransformSet,
        }

        public static bool Object(GameObject go, Rigidbody rb, Method method, Vector3 position)
        {
            switch (method)
            {
                case Method.RigidbodyMove:
                    rb.MovePosition(position);
                    return true;
                case Method.RigidbodySet:
                    rb.position = position;
                    return true;
                case Method.TransformSet:
                    go.transform.position = position;
                    return true;
            }
            return false;
        }
        public static bool Object(GameObject go, Rigidbody rb, Method method, Vector3 position, Quaternion rotation)
        {
            switch (method)
            {
                case Method.RigidbodyMove:
                    rb.Move(position, rotation);
                    return true;
                case Method.RigidbodySet:
                    rb.position = position;
                    rb.rotation = rotation;
                    return true;
                case Method.TransformSet:
                    go.transform.position = position;
                    go.transform.rotation = rotation;
                    return true;
            }
            return false;
        }

        public static bool Object(GameObject go, Rigidbody rb, Method method, NiTransform transform)
        {
            switch (method)
            {
                case Method.RigidbodyMove:
                    if(transform.HasPosition)
                    {
                        if (transform.HasRotation)
                            rb.Move(transform.Position, transform.Rotation);
                        else
                            rb.MovePosition(transform.Position);
                    }
                    else if(transform.HasRotation)
                            rb.MoveRotation(transform.Rotation);
                    if (transform.HasScale)
                        Debug.LogWarning("Cannot set scale with a Rigidbody.");
                    return true;
                case Method.RigidbodySet:
                    if (transform.HasPosition)
                        rb.position = transform.Position;
                    if (transform.HasRotation)
                        rb.rotation = transform.Rotation;
                    if (transform.HasScale)
                        Debug.LogWarning("Cannot set scale with a Rigidbody.");
                    return true;
                case Method.TransformSet:
                    if (transform.HasPosition)
                        go.transform.position = transform.Position;
                    if (transform.HasRotation)
                        go.transform.rotation = transform.Rotation;
                    if(transform.HasScale)
                        if(go.transform.parent != null)
                            go.transform.localScale = transform.Scale - go.transform.parent.lossyScale;
                        else
                            go.transform.localScale = transform.Scale;
                    return true;
            }
            return false;
        }
        public static bool Object(GameObject go, Method method, NiTransform transform)
        {
            switch (method)
            {
                case Method.RigidbodyMove:
                    if (go.TryGetComponent<Rigidbody>(out var rb))
                    {
                        if (transform.HasPosition)
                        {
                            if (transform.HasRotation)
                                rb.Move(transform.Position, transform.Rotation);
                            else
                                rb.MovePosition(transform.Position);
                        }
                        else if (transform.HasRotation)
                            rb.MoveRotation(transform.Rotation);
                        if (transform.HasScale)
                            Debug.LogWarning("Cannot set scale with a Rigidbody.");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot move GameObject '{go.GetNameOrNull()}' with Method.RigidbodyMove, GameObject does not have a Rigidbody.");
                        return false;
                    }
                case Method.RigidbodySet:
                    if (go.TryGetComponent<Rigidbody>(out var rb2))
                    {
                        if (transform.HasPosition)
                            rb2.position = transform.Position;
                        if (transform.HasRotation)
                            rb2.rotation = transform.Rotation;
                        if (transform.HasScale)
                            Debug.LogWarning("Cannot set scale with a Rigidbody.");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot move GameObject '{go.GetNameOrNull()}' with Method.RigidbodySet, GameObject does not have a Rigidbody.");
                        return false;
                    }
                case Method.TransformSet:
                    if (transform.HasPosition)
                        go.transform.position = transform.Position;
                    if (transform.HasRotation)
                        go.transform.rotation = transform.Rotation;
                    if (transform.HasScale)
                        if (go.transform.parent != null)
                            go.transform.localScale = transform.Scale - go.transform.parent.lossyScale;
                        else
                            go.transform.localScale = transform.Scale;
                    return true;
            }
            return false;
        }

        public static bool Object(GameObject go, Method method, Vector3 position) =>
            Object(go,
                (method == Method.RigidbodyMove || method == Method.RigidbodySet) ? go.GetComponent<Rigidbody>() : null,
                method, position);
        public static bool Object(GameObject go, Method method, Vector3 position, Quaternion rotation) =>
            Object(go,
                (method == Method.RigidbodyMove || method == Method.RigidbodySet) ? go.GetComponent<Rigidbody>() : null,
                method, position, rotation);
        public static bool Object(GameObject go, Rigidbody rb, Method method, Transform transform) =>
            Object(go, rb, method, transform.position, transform.rotation);
        public static bool Object(GameObject go, Method method, Transform transform) =>
            Object(go, method, transform.position, transform.rotation);


    }


    public static class CloneableExt
    {
        static readonly BindingFlags k_BindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        public static object DeepClone(this object source)
        {
            if(source == null) return null;
            return DeepCopy(source);
            //var type = source.GetType();
            //var clone = Activator.CreateInstance(type);
            //foreach (var fi in type.GetFields(k_BindingFlags))
            //{
            //    var value = fi.GetValue(source);
            //    if (fi.FieldType.IsPrimitive)
            //    {

            //    }
            //    else if (fi.FieldType.IsValueType)
            //    {
            //        value = DeepClone(value);
            //    }
            //    else if (value != null)
            //    {
            //        if (value is ICloneable valueCloneable)
            //        {
            //            value = valueCloneable.Clone();
            //        }
            //    }
            //    fi.SetValue(clone, value);
            //}
            //return clone;
        }

        /// <summary>
        /// Gets all fields from an object and its hierarchy inheritance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>All fields of the type.</returns>
        public static List<FieldInfo> GetAllFields(this Type type, BindingFlags flags)
        {
            // Early exit if Object type
            if (type == typeof(System.Object))
            {
                return new List<FieldInfo>();
            }

            // Recursive call
            var fields = type.BaseType.GetAllFields(flags);
            fields.AddRange(type.GetFields(flags | BindingFlags.DeclaredOnly));
            return fields;
        }

        /// <summary>
        /// Perform a deep copy of the class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A deep copy of obj.</returns>
        /// <exception cref="System.ArgumentNullException">Object cannot be null</exception>
        public static T DeepCopy<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("Object cannot be null");
            }
            return (T)DoCopy(obj);
        }


        /// <summary>
        /// Does the copy.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Unknown type</exception>
        private static object DoCopy(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            // Value type
            var type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }

            // Array
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(DoCopy(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }

            // Unity Object
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return obj;
            }

            // Class -> Recursion
            else if (type.IsClass)
            {
                var copy = Activator.CreateInstance(obj.GetType());

                var fields = type.GetAllFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    var fieldValue = field.GetValue(obj);
                    if (fieldValue != null)
                    {
                        field.SetValue(copy, DoCopy(fieldValue));
                    }
                }

                return copy;
            }

            // Fallback
            else
            {
                throw new ArgumentException("Unknown type");
            }
        }
    }

#if UNITY_EDITOR
    public static class SerializedPropertyExt
    {
        public static SerializedProperty FindParentProperty(this SerializedProperty serializedProperty)
        {
            var propertyPaths = serializedProperty.propertyPath.Split('.');
            if (propertyPaths.Length <= 1)
            {
                return default;
            }

            var parentSerializedProperty = serializedProperty.serializedObject.FindProperty(propertyPaths.First());
            for (var index = 1; index < propertyPaths.Length - 1; index++)
            {
                if (propertyPaths[index] == "Array")
                {
                    if (index + 1 == propertyPaths.Length - 1)
                    {
                        // reached the end
                        break;
                    }
                    if (propertyPaths.Length > index + 1 && Regex.IsMatch(propertyPaths[index + 1], "^data\\[\\d+\\]$"))
                    {
                        var match = Regex.Match(propertyPaths[index + 1], "^data\\[(\\d+)\\]$");
                        var arrayIndex = int.Parse(match.Groups[1].Value);
                        parentSerializedProperty = parentSerializedProperty.GetArrayElementAtIndex(arrayIndex);
                        index++;
                    }
                }
                else
                {
                    parentSerializedProperty = parentSerializedProperty.FindPropertyRelative(propertyPaths[index]);
                }
            }

            return parentSerializedProperty;
        }
    }
#endif
}