using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using NiEngine.Expressions;
using NiEngine.Expressions.GameObjects;

namespace NiEngine.Actions.Flow
{

    [Serializable, ClassPickerName("Flow/If")]
    public class If : Action, IUidObjectHost
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionBool Condition;

        [EditorField(showPrefixLabel: true, inline: false, header:false), Save(saveInPlace: true)]
        public ActionSet True;

        [EditorField(showPrefixLabel: true, inline: false, prefix:"Else"), Save(saveInPlace: true)]
        public ActionSet False;

        public IEnumerable<IUidObject> Uids => True.Uids.Concat(False.Uids);

        public override void Act(Owner owner, EventParameters parameters)
        {
            var eventProcessor = new EventProcessor();
            var b = Condition.GetValue(owner, parameters);
            if (b) 
                eventProcessor.Act(owner, True, parameters);
            else
                eventProcessor.Act(owner, False, parameters);
        }
    }
    [Serializable, ClassPickerName("Flow/If")]
    public class StateIf : StateAction, IUidObjectHost
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionBool Condition;

        [EditorField(showPrefixLabel: true, inline: false, header: false), Save(saveInPlace: true)]
        public StateActionSet True;

        [EditorField(showPrefixLabel: true, inline: false, prefix: "Else"), Save(saveInPlace: true)]
        public StateActionSet False;
        public IEnumerable<IUidObject> Uids => True.Uids.Concat(False.Uids);

        [EditorField(runtimeOnly: true), NotSaved]
        public bool Result = false;

        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            var eventProcessor = new EventStateProcessor();
            Result = Condition.GetValue(owner, parameters);
            if (Result)
                eventProcessor.Begin(owner, True, parameters);
            else
                eventProcessor.Begin(owner, False, parameters);
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            var eventProcessor = new EventStateProcessor();
            if (Result)
                eventProcessor.End(owner, True, null, parameters);
            else
                eventProcessor.End(owner, False, null, parameters);
        }
    }
    [Serializable, ClassPickerName("Flow/ReactionSwitch")]
    public class ReactionSwitch : Action
    {

        [Tooltip("The source object to test state with.")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionGameObject Source = new NiEngine.Expressions.GameObjects.Self();

        [Tooltip("The destination object to send reaction to. If left null, same as source")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionGameObject Destination = new NiEngine.Expressions.GameObjects.Self();

        [Serializable]
        public struct Entry
        {
            [EditorField(showPrefixLabel: true, inline: true)]
            public string If;
            [EditorField(showPrefixLabel: true, inline: true)]
            public string Then;
        }

        [EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public List<Entry> Switches;

        [Serializable]
        public struct OptionsStruct
        {
            public bool IgnoreIfNull;
            public bool IgnoreMissingReaction;
            public bool UseOverrides;
            [EditorField(unfold: true)]
            public EventParameters.Overrides Overrides;
        }

        [EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public OptionsStruct Options;

        public override void Act(Owner owner, EventParameters parameters)
        {
            var objSource = Source.GetValue(owner, parameters);
            var objDestination = Destination?.GetValue(owner, parameters) ?? objSource;
            if (objSource == null || objDestination == null)
            {
                if (!Options.IgnoreIfNull)
                {
                    if (objSource == null)
                        parameters.LogError(this, owner, "ReactionSwitch.Source.Null", "Source is null");
                    if (objDestination == null)
                        parameters.LogError(this, owner, "ReactionSwitch.Destination.Null", "Destination is null");
                }
                return;
            }
            foreach (var s in Switches)
            {
                if (ReactionReference.HasReaction(objSource, s.If, onlyEnabled: true, onlyActive: true, maxLoop: ReactionReference.k_MaxLoop))
                {
                    if (Options.UseOverrides)
                        parameters = parameters.WithOverride(owner, Options.Overrides);
                    int count = ReactionReference.React(owner, objDestination, s.Then, parameters.WithSelf(objDestination));

                    if (!Options.IgnoreMissingReaction && count == 0)
                    {
                        parameters.LogError(this, owner, "ReactionSwitch.ReactionNotFound",
                            $"Reaction '{s.Then}' not found on '{objDestination.GetNameOrNull()}'",
                            owner.GameObject);
                    }

                    break;
                }
            }
        }
    }
    [Serializable, ClassPickerName("Flow/Group Call")]
    public class CallGroup : StateAction
    {
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: false, inline: true)]
        public IExpressionGameObject On = new NiEngine.Expressions.GameObjects.Self();

        [Tooltip("Group name")]
        [EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public string Name;
        
        [EditorField(unfold:true), NotSaved]
        public EventParameters.Overrides Overrides;
        
        [EditorField(showPrefixLabel: true, inline: true), NotSaved]
        public string StateAfter;

        [Tooltip("If empty, will use 'State After' when fail.")]
        [EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public string StateIfFails;
        
        [Tooltip("Set only if you need to do something special when canceled.")]
        [EditorField(showPrefixLabel: true, inline: false), NotSaved]
        public string StateIfCancel;
        
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            var on = On.GetValue(owner, parameters);
            if (ReactionStateMachine.TryGetGroup(on, Name, out var group))
            {
                group.StateMachine.CallGroup(group, owner, parameters.WithOverride(owner, Overrides), StateAfter, StateIfFails, StateIfCancel);
            }
            else
            {
                parameters.LogError(this, owner, "CallGroup.GroupNotFound", $"Could not call group named '{Name}', group not found.");
            }

        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
            var on = On.GetValue(owner, parameters);
            if (ReactionStateMachine.TryGetGroup(on, Name, out var group))
            {
                group.StateMachine.BreakCall(group, owner, parameters);
            }
            else
            {
                parameters.LogError(this, owner, "CallGroup.GroupNotFound", $"Could not break call group named '{Name}', group not found.");
            }
        }
    }
    [Serializable, ClassPickerName("Flow/Group Yield")]
    public class YieldGroup : StateAction
    {
        [EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public ReactionStateMachine.StateGroup.YieldResult Result = ReactionStateMachine.StateGroup.YieldResult.Done;
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            if (owner.State != null)
                owner.StateMachine.YieldGroup(owner.State.StateGroup, Result);
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
        }
    }

    [Serializable, ClassPickerName("Flow/Group OnCancel")]
    public class GroupCancel : StateAction
    {
        [Tooltip("Name of the state in this group that will activate if the group call is canceled.")]
        [EditorField(showPrefixLabel: false, inline: true), NotSaved]
        public string StateOnCancel;
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            owner.State.StateGroup.CancelState = StateOnCancel;
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
        }
    }
    [Serializable, ClassPickerName("Flow/Group Next State")]
    public class GroupNextState : StateAction
    {
        public override void OnBegin(Owner owner, EventParameters parameters)
        {
            owner.State.StateGroup.NextState(owner, parameters);
        }
        public override void OnEnd(Owner owner, EventParameters parameters)
        {
        }
    }
}
