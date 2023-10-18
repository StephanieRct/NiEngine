using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NiEngine.Expressions.GameObjects;

namespace NiEngine.Actions
{

    public abstract class StateActionComponentToggle<TComponent> : StateAction
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObjects Target;

        [EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public bool Enabled;

        [Serializable]
        public struct AtEndState
        {
            [EditorField(showPrefixLabel: true, inline: false)]
            public bool Reverse;

            [EditorField(showPrefixLabel: true, inline: false)]
            public bool ReevaluateTarget;
        }

        [EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public AtEndState AtEnd = new AtEndState
        {
            Reverse = true,
            ReevaluateTarget = true,
        };

        [Serializable]
        public struct InternalState
        {
            [EditorField(showPrefixLabel: true, inline: false)]
            public List<TComponent> Targets;
        }

        [SerializeField, EditorField(runtimeOnly: true)]
        public InternalState Internals;
        public abstract void SetValue(TComponent component, bool value);
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            if (AtEnd.Reverse && !AtEnd.ReevaluateTarget)
            {
                Internals.Targets = new List<TComponent>();
                foreach (var obj in Target.GetValues(owner, parameters))
                    if (obj.TryGetComponent<TComponent>(out var component))
                    {
                        Internals.Targets.Add(component);
                        SetValue(component, Enabled);
                    }
            }
            else
                foreach (var obj in Target.GetValues(owner, parameters))
                    if (obj.TryGetComponent<TComponent>(out var component))
                        SetValue(component, Enabled);
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            if (AtEnd.Reverse && !AtEnd.ReevaluateTarget)
            {
                Internals.Targets = new List<TComponent>();
                foreach (var component in Internals.Targets)
                    SetValue(component, !Enabled);
            }
            else
                foreach (var obj in Target.GetValues(owner, parameters))
                    if (obj.TryGetComponent<TComponent>(out var component))
                        SetValue(component, !Enabled);
        }
    }
}