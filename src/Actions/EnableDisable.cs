using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using NiEngine.Expressions;
using NiEngine.Expressions.GameObjects;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("State/GameObject./Enable", inline: true, showPrefixLabel: false)]
    public class GameObjectEnableStateAction : StateAction
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public IExpressionGameObjects Target;

        [EditorField(inline: true, showPrefixLabel: false), NotSaved, Tooltip("Enable GameObject")]
        public bool Enable = true;

        [NotSaved]
        public bool InverseAtEnd = true;

        [Serializable]
        struct InternalState
        {
            //public List<GameObject> TargetObjects;
            public EventParameters.ParameterSet ParametersAtBegin;
        }

        [SerializeField, EditorField(runtimeOnly: true)]
        private InternalState Internals;
        
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            //Internals.TargetObjects = new();
            Internals.ParametersAtBegin = parameters.Current;
            foreach (var obj in Target.GetValues(owner, parameters))
            {
                obj.SetActive(Enable);
                //Internals.TargetObjects.Add(obj);
            }
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if(InverseAtEnd)
            {
                foreach (var obj in Target.GetValues(owner, parameters.WithBegin(Internals.ParametersAtBegin)))
                {
                    obj.SetActive(!Enable);
                }
                //if (Internals.TargetObjects != null)
                //{
                //    foreach(var obj in Internals.TargetObjects)
                //        obj.SetActive(!Enable);
                //    Internals.TargetObjects = null;
                //}
                //else
                //{
                //    foreach (var obj in Target.GetValues(parameters))
                //    {
                //        obj.SetActive(!Enable);
                //    }
                //}
            }
        }
    }
    [Serializable, ClassPickerName("GameObject./Enable", inline: true, showPrefixLabel: false)]
    public class GameObjectEnableAction : Action
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObjects Target;

        [EditorField(inline: true, showPrefixLabel: false)]
        [Tooltip("Enable GameObject")]
        public bool Enable = true;
        

        public override void Act(Owner owner, EventParameters parameters)
        {
            foreach (var obj in Target.GetValues(owner, parameters))
            {
                obj.SetActive(Enable);
            }
        }
    }




    //[Serializable, ClassPickerName("Enable")]
    //public class EnableDisable : StateAction
    //{
    //    public GameObjectReference Target;
    //    public bool Enable;
    //    public bool RevertAtEnd;
    //    [Serializable]
    //    public struct InternalState
    //    {
    //        public GameObject TargetObject;
    //        public bool WasActive;

    //    }
    //    public InternalState Internals;
    //    public override void OnBegin(Owner owner, EventParameters parameters)
    //    {
    //        var target = Target.GetTargetGameObject(parameters);
    //        if (target != null)
    //        {
    //            Internals.TargetObject = target;
    //            if (RevertAtEnd)
    //                Internals.WasActive = target.activeSelf;
    //            target.SetActive(Enable);

    //        }
    //    }
    //    public override void OnEnd(Owner owner, EventParameters parameters)
    //    {
    //        if (RevertAtEnd && Internals.TargetObject != null)
    //            Internals.TargetObject.SetActive(Internals.WasActive);
    //    }
    //}
    //[Serializable, ClassPickerName("Enable Multiple")]
    //public class EnableDisableMultiple : StateAction
    //{
    //    [Serializable]
    //    public struct Instance
    //    {
    //        public GameObjectReference Target;
    //        public bool Enable;
    //        public bool RevertAtEnd;
    //        [Serializable]
    //        public struct InternalState
    //        {
    //            public GameObject TargetObject;
    //            public bool WasActive;
    //        }
    //        public InternalState Internals;

    //        public void OnBegin(Owner owner, EventParameters parameters)
    //        {
    //            var target = Target.GetTargetGameObject(parameters);
    //            if (target != null)
    //            {
    //                Internals.TargetObject = target;
    //                if (RevertAtEnd)
    //                    Internals.WasActive = target.activeSelf;
    //                target.SetActive(Enable);

    //            }
    //        }
    //        public void OnEnd(Owner owner, EventParameters parameters)
    //        {
    //            if (RevertAtEnd && Internals.TargetObject != null)
    //                Internals.TargetObject.SetActive(Internals.WasActive);
    //        }

    //    }
    //    public List<Instance> Instances = new();
    //    public override void OnBegin(Owner owner, EventParameters parameters)
    //    {
    //        foreach (var instance in Instances)
    //            instance.OnBegin(owner, parameters);
    //    }
    //    public override void OnEnd(Owner owner, EventParameters parameters)
    //    {
    //        foreach (var instance in Instances)
    //            instance.OnEnd(owner, parameters);
    //    }
    //}
}