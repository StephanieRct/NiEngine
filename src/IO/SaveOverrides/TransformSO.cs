using NiEngine.Actions.Flow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace NiEngine.IO.SaveOverrides
{
    // component that cannot be multiple on the same gameobject like transform or rigibody
    public abstract class ComponentSingleSO<T> : ISaveOverrideProxy
        where T : Component
    {
        GameObjectSO GameObjectSO;
        public ComponentSingleSO(GameObjectSO gameObjectSO)
        {
            GameObjectSO = gameObjectSO;
        }
        public SupportType IsSupportedType(Type type) => type == typeof(T) ? SupportType.Supported : SupportType.Unsupported;

        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            if (obj == null)
            {
                GameObjectSO.Save(context, type, null, io);
            }
            else if( obj is T objT)
            {
                if(objT != null)
                    GameObjectSO.Save(context, type, objT.gameObject, io);
                else
                    GameObjectSO.Save(context, type, null, io);
            }
            else
            {
                context.LogError($"Bad object type, expecting '{typeof(T).Name}' but was '{obj.GetType().Name}'");
            }
        }
        public object Load(StreamContext context, Type type, IInput io)
        {
            var obj = GameObjectSO.Load(context, type, io);
            if (obj == null)
                return null;
            var go = obj as GameObject;
            if(go != null && go.TryGetComponent<T>(out var component))
            {
                return component;
            }
            return null;
        }
        public abstract void SaveInPlace(StreamContext context, Type type, object obj, IOutput io);
        public abstract void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io);
    }
    public class TransformSO : ComponentSingleSO<Transform>
    {
        public TransformSO(GameObjectSO gameObjectSO)
            :base(gameObjectSO)
        {

        }
        public override void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var trf = obj as Transform;
            io.Save(context, "localPosition", trf.localPosition);
            io.Save(context, "localRotation", trf.localRotation);
            io.Save(context, "localScale", trf.localScale);
        }
        public override void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var trf = obj as Transform;
            trf.localPosition = io.Load<Vector3>(context, "localPosition");
            trf.localRotation = io.Load<Quaternion>(context, "localRotation");
            trf.localScale = io.Load<Vector3>(context, "localScale");
        }
    }
    public class RigidbodySO : ComponentSingleSO<Rigidbody>
    {
        public RigidbodySO(GameObjectSO gameObjectSO)
            : base(gameObjectSO)
        {

        }
        public override void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var rb = obj as Rigidbody;
            io.Save(context, "mass", rb.mass);
            io.Save(context, "position", rb.position);
            io.Save(context, "drag", rb.drag);
            io.Save(context, "angularDrag", rb.angularDrag);
            io.Save(context, "isKinematic", rb.isKinematic);
            if (!rb.isKinematic)
            {
                io.Save(context, "velocity", rb.velocity);
                io.Save(context, "angularVelocity", rb.angularVelocity);
            }
        }
        public override void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var rb = obj as Rigidbody;
            rb.mass = io.Load<float>(context, "mass");
            rb.position = io.Load<Vector3>(context, "position");
            rb.isKinematic = io.Load<bool>(context, "isKinematic");
            rb.drag = io.Load<float>(context, "drag");
            rb.angularDrag = io.Load<float>(context, "angularDrag");
            if (!rb.isKinematic)
            {
                rb.velocity = io.Load<Vector3>(context, "velocity");
                rb.angularVelocity = io.Load<Vector3>(context, "angularVelocity");
            }
        }
    }

    public class CharacterControllerSO : ComponentSingleSO<CharacterController>
    {
        public CharacterControllerSO(GameObjectSO gameObjectSO)
            : base(gameObjectSO)
        {

        }
        public override void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var cc = obj as CharacterController;
            io.Save(context, "localPosition", cc.transform.localPosition);
            io.Save(context, "localRotation", cc.transform.localRotation);
            io.Save(context, "localScale", cc.transform.localScale);
        }
        public override void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var cc = obj as CharacterController;
            cc.enabled = false;
            cc.transform.localPosition = io.Load<Vector3>(context, "localPosition");
            cc.transform.localRotation = io.Load<Quaternion>(context, "localRotation");
            cc.transform.localScale = io.Load<Vector3>(context, "localScale");
            cc.enabled = true;
            cc.Move(Vector3.zero);
        }
    }
    public class NavMeshAgentSO : ComponentSingleSO<NavMeshAgent>
    {
        public NavMeshAgentSO(GameObjectSO gameObjectSO)
            : base(gameObjectSO)
        {

        }
        public override void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            var a = obj as NavMeshAgent;
            io.Save(context, "destination", a.destination);
        }
        public override void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            var a = obj as NavMeshAgent;
            a.destination = io.Load<Vector3>(context, "destination");
        }
    }
}