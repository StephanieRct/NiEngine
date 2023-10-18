using NiEngine.Expressions.GameObjects;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NiEngine.Actions
{
    [Serializable, ClassPickerName("Grabbable/OnGrab")]
    public class GrabbableOnGrab : StateAction, IGrabbableObserver, IUidObjectHost
    {

        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObject GrabbableObject;

        [Tooltip("From is the grabbable GameObject. Trigger is GameObject that grabbed it."), Save(saveInPlace: true)]
        public ActionSet Actions;

        [Serializable]
        public struct InternalsState
        {
            public Grabbable Grabbable;
            public Owner Owner;
        }
        [SerializeField, EditorField(runtimeOnly: true)]
        public InternalsState Internals;

        public IEnumerable<IUidObject> Uids => Actions.Uids;

        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            var go = GrabbableObject.GetValue(owner, parameters);
            if (go == null)
            {
                parameters.LogError(this, owner, "Grabbable.GameObject.NotFound", $"Grabbable Object is null.");
                return;
            }
            if (go.TryGetComponent<Grabbable>(out var grabbable))
            {
                Internals.Grabbable = grabbable;
                Internals.Owner = owner;
                grabbable.AddObserver(this);
            }
            else
            {
                parameters.LogError(this, owner, "Grabbable.Component.NotFound", $"GameObject '{go.GetNameOrNull()}' must have a 'Grabbable' component.");
                return;
            }
        }

        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (Internals.Grabbable != null)
            {
                Internals.Grabbable.RemoveObserver(this);
                Internals.Grabbable = null;
                Internals.Owner = default;
            }
        }

        public void OnGrab(Grabbable grabbable)
        {
            Actions.Act(Internals.Owner, EventParameters.Trigger(Internals.Owner.GameObject, grabbable.gameObject, grabbable.GrabbedBy?.gameObject, grabbable.GrabbedPosition));
        }

        public void OnRelease(Grabbable grabbable)
        {
        }

    }
}