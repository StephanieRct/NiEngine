using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using NiEngine.Recording;
using NiEngine.IO;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine
{

    [Serializable]
    [Save(saveInPlace: true)]
    public class ConditionSet : ICloneable, IUidObjectHost
    {
        [SerializeField, NotSaved]
        bool Expanded = false;
        [SerializeReference, ObjectReferencePicker(typeof(ICondition)), Save(saveInPlace: true), EditorField(showPrefixLabel: false, inline: false)]
        public List<ICondition> Conditions = new();
        public IEnumerable<IUidObject> Uids => Conditions.OfType<IUidObject>();
        public bool Pass(Owner owner, EventParameters parameters, bool logFalseResults = false)
        {
            foreach (var c in Conditions)
            {
                if (owner.State?.StateGroup?.HasYielded ?? false)
                    break;
                if (c == null)
                {
                    parameters.LogWarning(owner, "Condition.Null", "Null condition");
                    continue;
                }
                parameters.RecordEventSource?.BeginRecordCondition(owner, c, parameters);
                var pass = c.Pass(owner, parameters, logFalseResults);
                parameters.RecordEventSource?.EndRecordCondition(pass);
                if (!pass)
                    return false;
            }
            return true;
        }

        public void AddAllUpdates(UpdateSet updateSet, Owner owner, EventParameters parameters)
        {
            foreach (var c in Conditions)
            {
                parameters.RecordEventSource?.BeginRecordCondition(owner, c, parameters);
                if (c is IUpdate update)
                    updateSet.AddUpdate(owner, parameters, update);
                parameters.RecordEventSource?.EndRecordCondition(result:true);
            }
        }

        public object Clone()
        {
            var copy = new ConditionSet();
            copy.Conditions = new List<ICondition>(Conditions.Count);
            foreach(var o in Conditions)
                copy.Conditions.Add((ICondition)o.Clone());
            return copy;
        }
    }

    [Serializable]
    public class ActionSetBase 
    {
        //[SerializeReference, ObjectReferencePicker(typeof(IAction)), EditorField(showPrefixLabel: false, inline: false, header:false)]
        //public List<IAction> Actions = new();
        protected void Act(Owner owner, EventParameters parameters, List<IAction> actions)
        {
            foreach (var a in actions)
            {
                if (owner.State?.StateGroup?.HasYielded ?? false)
                    break;
                if (a == null)
                {
                    parameters.LogWarning(owner, "Action.Null", "Null action");
                    continue;
                }
                parameters.RecordEventSource?.BeginRecordAction(owner, a, parameters);
                try
                {
                    a.Act(owner, parameters);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, owner.GameObject);
                }
                if (a is IUpdatePhase updatePhase)
                {
                    switch (updatePhase.ActiveUpdatePhaseFlags)
                    {
                        case UpdatePhase.FixedUpdate:
                            owner.AddFixedUpdate((IFixedUpdate)updatePhase, parameters);
                            break;
                        case UpdatePhase.Update:
                            owner.AddUpdate((IUpdate)updatePhase, parameters);
                            break;
                        case UpdatePhase.LateUpdate:
                            owner.AddLateUpdate((ILateUpdate)updatePhase, parameters);
                            break;
                    }
                }
                else
                {
                    if (a is IUpdate update)
                        owner.AddUpdate(update, parameters);
                    if (a is ILateUpdate lateUpdate)
                        owner.AddLateUpdate(lateUpdate, parameters);
                    if (a is IFixedUpdate fixedUpdate)
                        owner.AddFixedUpdate(fixedUpdate, parameters);
                }

                parameters.RecordEventSource?.EndRecordAction();
            }
        }

        //protected void Act(MonoBehaviour from, List<IAction> actions)
        //    => Act(new(from), EventParameters.WithoutTrigger(from.gameObject, from.gameObject), actions);
    }
    [Serializable]
    [Save(saveInPlace: true)]
    public class ActionSetNoHead : ActionSetBase, ICloneable, IUidObjectHost
    {
        [SerializeReference, ObjectReferencePicker(typeof(IAction)), Save(saveInPlace: true), EditorField(showPrefixLabel: false, inline: false, header:false)]
        public List<IAction> Actions = new();
        public IEnumerable<IUidObject> Uids => Actions.OfType<IUidObject>();
        public void Act(Owner owner, EventParameters parameters)
            => Act(owner, parameters, Actions);
        //public void Act(MonoBehaviour from)
        //    => Act(new(from), EventParameters.WithoutTrigger(from.gameObject, from.gameObject));
        public object Clone()
        {
            var copy = new ActionSet();
            copy.Actions = new List<IAction>(Actions.Count);
            foreach (var o in Actions)
                copy.Actions.Add((IAction)o.Clone());
            return copy;
        }
    }

    [Serializable]
    [Save(saveInPlace: true)]
    public class ActionSet : ActionSetBase, ICloneable, IUidObjectHost
    {
        [SerializeField, NotSaved]
        bool Expanded = false;
        [SerializeReference, ObjectReferencePicker(typeof(IAction)), Save(saveInPlace: true), EditorField(showPrefixLabel: false, inline: false)]
        public List<IAction> Actions = new();
        public IEnumerable<IUidObject> Uids => Actions.OfType<IUidObject>();


        public void Act(Owner owner, EventParameters parameters)
            => Act(owner, parameters, Actions);
        //public void Act(MonoBehaviour from)
        //    => Act(new(from), EventParameters.WithoutTrigger(from.gameObject, from.gameObject));
        public object Clone()
        {
            var copy = new ActionSet();
            copy.Actions = new List<IAction>(Actions.Count);
            foreach (var o in Actions)
                copy.Actions.Add((IAction)o.Clone());
            return copy;
        }
    }


    [Serializable]
    public class StateActionSetBase
    {
        protected void OnBegin(Owner owner, EventParameters parameters, List<IStateAction> actions)
        {
            foreach (var a in actions)
            {
                if (owner.State?.StateGroup?.HasYielded ?? false)
                    break;
                if (a == null)
                {
                    parameters.LogWarning(owner, "StateAction.Null", "Null state action");
                    continue;
                }
                parameters.RecordEventSource?.BeginRecordStateAction(owner, a, EventRecord.PhaseEnum.OnBegin, parameters);
                try
                {
                    a.OnBegin(owner, parameters);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, owner.GameObject);
                }

                if (a is IUpdatePhase updatePhase)
                {
                    switch (updatePhase.ActiveUpdatePhaseFlags)
                    {
                        case UpdatePhase.FixedUpdate:
                            owner.AddFixedUpdate((IFixedUpdate)updatePhase, parameters);
                            break;
                        case UpdatePhase.Update:
                            owner.AddUpdate((IUpdate)updatePhase, parameters);
                            break;
                        case UpdatePhase.LateUpdate:
                            owner.AddLateUpdate((ILateUpdate)updatePhase, parameters);
                            break;
                    }
                }
                else
                {
                    if (a is IUpdate update)
                        owner.AddUpdate(update, parameters);
                    if (a is ILateUpdate lateUpdate)
                        owner.AddLateUpdate(lateUpdate, parameters);
                    if (a is IFixedUpdate fixedUpdate)
                        owner.AddFixedUpdate(fixedUpdate, parameters);
                }

                parameters.RecordEventSource?.EndRecordStateAction();
            }
        }
        protected void OnEnd(Owner owner, EventParameters parameters, List<IStateAction> reversedActions)
        {
            foreach (var a in reversedActions)
            {
                if (a is IUpdatePhase updatePhase)
                {
                    switch (updatePhase.ActiveUpdatePhaseFlags)
                    {
                        case UpdatePhase.FixedUpdate:
                            owner.RemoveFixedUpdate((IFixedUpdate)updatePhase);
                            break;
                        case UpdatePhase.Update:
                            owner.RemoveUpdate((IUpdate)updatePhase);
                            break;
                        case UpdatePhase.LateUpdate:
                            owner.RemoveLateUpdate((ILateUpdate)updatePhase);
                            break;
                    }
                }
                else
                {
                    if (a is IUpdate update)
                        owner.RemoveUpdate(update);
                    if (a is ILateUpdate lateUpdate)
                        owner.RemoveLateUpdate(lateUpdate);
                    if (a is IFixedUpdate fixedUpdate)
                        owner.RemoveFixedUpdate(fixedUpdate);
                }
            }

            foreach (var a in reversedActions)
            {
                parameters.RecordEventSource?.BeginRecordStateAction(owner, a, EventRecord.PhaseEnum.OnEnd, parameters);
                try
                {
                    a.OnEnd(owner, parameters);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, owner.GameObject);
                }
                parameters.RecordEventSource?.EndRecordStateAction();
            }
        }

        protected void AddAllBeginIUpdate(Owner owner, EventParameters parameters, List<IStateAction> actions)
        {
            foreach (var a in actions)
            {
                parameters.RecordEventSource?.BeginRecordStateAction(owner, a, EventRecord.PhaseEnum.OnBegin, parameters);
                if (a is IUpdate update)
                    owner.AddUpdate(update, parameters);
                if (a is ILateUpdate lateUpdate)
                    owner.AddLateUpdate(lateUpdate, parameters);
                if (a is IFixedUpdate fixedUpdate)
                    owner.AddFixedUpdate(fixedUpdate, parameters);
                parameters.RecordEventSource?.EndRecordStateAction();
            }
        }

    }

    [Serializable]
    [Save(saveInPlace: true)]
    public class StateActionSetNoHead : StateActionSetBase, ICloneable, IUidObjectHost//, ISaveOverride
    {
        [SerializeReference, ObjectReferencePicker(typeof(IStateAction)), Save(saveInPlace: true), EditorField(showPrefixLabel: false, inline: false, header:false)]//, EditorField(inline: true, showPrefixLabel: false)]//, DerivedClassPicker(typeof(IStateAction), showPrefixLabel: false)]
        public List<IStateAction> Actions = new();
        [NotSaved]
        public List<IStateAction> ReversedActions;
        public IEnumerable<IUidObject> Uids => Actions.OfType<IUidObject>();

        public void OnBegin(Owner owner, EventParameters parameters)
            => OnBegin(owner, parameters, Actions);
        public void OnEnd(Owner owner, EventParameters parameters)
        {
            if (ReversedActions == null)
            {
                ReversedActions = new(Actions);
                ReversedActions.Reverse();
            }
            OnEnd(owner, parameters, ReversedActions);
        }

        public void AddAllBeginIUpdate(Owner owner, EventParameters parameters)
            => AddAllBeginIUpdate(owner, parameters, Actions);

        public object Clone()
        {
            var copy = new StateActionSet();
            copy.Actions = new List<IStateAction>(Actions.Count);
            copy.ReversedActions = null;
            foreach (var o in Actions)
                copy.Actions.Add((IStateAction)o.Clone());
            return copy;
        }

        //public void Save(StreamContext context, IOutput io)
        //{
        //    io.SaveInPlace(context, "Actions", Actions);
        //}

        //public void Load(StreamContext context, IInput io)
        //{
        //    io.LoadInPlace(context, "Actions", ref Actions);
        //}
    }

    [Serializable]
    [Save(saveInPlace: true)]
    public class StateActionSet : StateActionSetBase, ICloneable, IUidObjectHost//, ISaveOverride
    {
        
        [SerializeField, NotSaved]
        bool Expanded = false;
        [SerializeReference, ObjectReferencePicker(typeof(IStateAction)), Save(saveInPlace: true), EditorField(showPrefixLabel: false, inline: false)]//, EditorField(inline: true, showPrefixLabel: false)]//, DerivedClassPicker(typeof(IStateAction), showPrefixLabel: false)]
        public List<IStateAction> Actions = new();
        [NotSaved]
        public List<IStateAction> ReversedActions;
        public IEnumerable<IUidObject> Uids => Actions.OfType<IUidObject>();

        public void OnBegin(Owner owner, EventParameters parameters)
            => OnBegin(owner, parameters, Actions);
        public void OnEnd(Owner owner, EventParameters parameters)
        {
            if (ReversedActions == null)
            {
                ReversedActions = new(Actions);
                ReversedActions.Reverse();
            }
            OnEnd(owner, parameters, ReversedActions);
        }

        public void AddAllBeginIUpdate(Owner owner, EventParameters parameters)
            => AddAllBeginIUpdate(owner, parameters, Actions);

        public object Clone()
        {
            var copy = new StateActionSet();
            copy.Actions = new List<IStateAction>(Actions.Count);
            copy.ReversedActions = null;
            foreach (var o in Actions)
                copy.Actions.Add((IStateAction)o.Clone());
            return copy;
        }

        //public void Save(StreamContext context, IOutput io)
        //{
        //    io.SaveInPlace(context, "Actions", Actions);
        //}

        //public void Load(StreamContext context, IInput io)
        //{
        //    io.LoadInPlace(context, "Actions", ref Actions);
        //}
    }

    [Serializable]
    public class UpdateSet<TUpdate> : ISaveOverride
        where TUpdate : class
    {
        [Serializable]
        public struct UpdateState
        {
            [SerializeReference]
            public TUpdate IUpdate;
            public Owner Owner;
            public EventParameters Parameters;
            public Recording.EventRecord EventRecord;
        }
        public List<UpdateState> Updates = new();

        [NonSerialized]
        protected bool m_IsIterating = false;
        [NonSerialized]
        List<(UpdateState updateState, bool addOrRemove)> m_UpdateDelta;
        void AddDelta(UpdateState updateState, bool addOrRemove)
        {
            if(m_UpdateDelta == null)
            {
                m_UpdateDelta = new();
            }
            m_UpdateDelta.Add((updateState, addOrRemove));
        }
        protected void ProcessDelta() 
        {
            if (m_UpdateDelta == null) return;
            foreach(var (update, addOrRemove) in m_UpdateDelta)
            {
                if (addOrRemove)
                    Updates.Add(update);
                else
                    Updates.RemoveAll(x => x.IUpdate == update.IUpdate);
            }
            m_UpdateDelta.Clear();
        }
        public void AddUpdate(Owner owner, EventParameters parameters, TUpdate update)
        {
            if (Updates == null)
                Updates = new();

            var us = new UpdateState
            {
                IUpdate = update,
                Owner = owner,
                Parameters = parameters,
                EventRecord = parameters.RecordEventSource?.CurrentRecord,
            };
            if (m_IsIterating)
            {
                AddDelta(us, true);
                return;
            }

            Updates.Add(us);
        }
        public void RemoveUpdate(TUpdate update)
        {
            if (m_IsIterating)
            {
                AddDelta(new UpdateState { IUpdate = update }, false);
                return;
            }
            Updates?.RemoveAll(x => x.IUpdate == update);
        }
        public void Save(StreamContext context, IOutput io)
        {
            int length = Updates.Count;
            io.Save(context, "length", length);
            for (int i = 0; i != length; i++)
            {
                using var _ = context.ScopeKey(i, io);
                io.Save(context, "Owner", Updates[i].Owner);
                io.Save(context, "Parameters", Updates[i].Parameters);

                // TODO use io.SaveReference
                var u = Updates[i].IUpdate;
                if (u is IUidObject uidObj)
                {
                    io.Save(context, "IUpdateRef", uidObj.Uid);
                }
            }
        }

        public void Load(StreamContext context, IInput io)
        {
            var length = io.Load<int>(context, "length");
            Updates.Clear();

            for (int i = 0; i != length; i++)
            {
                using var _ = context.ScopeKey(i, io);
                var us = new UpdateState();
                us.Owner = io.Load<Owner>(context, "Owner");
                us.Parameters = io.Load<EventParameters>(context, "Parameters");
                if (io.TryLoad(context, "IUpdateRef", out Uid uid))
                {
                    if(us.Owner.NiBehaviour.TryFindUidObject(uid, out var uidObject))
                    {
                        us.IUpdate = uidObject as TUpdate;
                    }    
                    //if (UidObject.UidToObject.TryGetValue(uid, out var obj))
                    //{
                    //    us.IUpdate = obj as TUpdate;
                    //}
                }
                if (us.IUpdate != null)
                {
                    Updates.Add(us);
                }
                else
                {
                    context.LogError($"{nameof(UpdateSet)}.{nameof(Load)}: Could not find {nameof(TUpdate)} object with Uid {uid}");
                    if (context.MustStop)
                        return;
                }
            }
        }
    }
    [Serializable]
    [Save(saveInPlace: true)]
    public class UpdateSet : UpdateSet<IUpdate>// ISaveOverride
    {
        public bool Update()
        {
            if (Updates == null) return false;
            m_IsIterating = true;
            Updates.RemoveAll(x =>
            {

                var source = x.Parameters.RecordEventSource;
                var previousRecord = source?.SwapCurrentRecord(x.EventRecord);
                source?.BeginRecordUpdate(x.Owner, x.IUpdate, x.Parameters);
                bool result = false;
                try
                {
                    result = x.IUpdate.Update(x.Owner, x.Parameters);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, x.Owner.GameObject);
                }
                source?.EndRecordUpdate();
                source?.SwapCurrentRecord(previousRecord);
                return !result;
            });
            m_IsIterating = false;
            ProcessDelta();
            return Updates.Count > 0;
        }

    }
    [Serializable]
    [Save(saveInPlace: true)]
    public class LateUpdateSet : UpdateSet<ILateUpdate>
    {
        public bool Update()
        {
            if (Updates == null) return false;
            m_IsIterating = true;

            Updates.RemoveAll(x =>
            {
                var source = x.Parameters.RecordEventSource;
                var previousRecord = source?.SwapCurrentRecord(x.EventRecord);
                source?.BeginRecordUpdate(x.Owner, null, x.Parameters);
                bool result = false;
                try
                {
                    result = x.IUpdate.LateUpdate(x.Owner, x.Parameters);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, x.Owner.GameObject);
                }
                source?.EndRecordUpdate();
                source?.SwapCurrentRecord(previousRecord);
                return !result;
            });
            m_IsIterating = false;
            ProcessDelta();
            return Updates.Count > 0;
        }
    }
    [Serializable]
    [Save(saveInPlace: true)]
    public class FixedUpdateSet : UpdateSet<IFixedUpdate>
    {
        public bool Update()
        {
            if (Updates == null) return false;
            m_IsIterating = true;

            Updates.RemoveAll(x =>
            {
                var source = x.Parameters.RecordEventSource;
                var previousRecord = source?.SwapCurrentRecord(x.EventRecord);
                source?.BeginRecordUpdate(x.Owner, null, x.Parameters);

                bool result = false;
                try
                {
                    result = x.IUpdate.FixedUpdate(x.Owner, x.Parameters);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex, x.Owner.GameObject);
                }

                source?.EndRecordUpdate();
                source?.SwapCurrentRecord(previousRecord);
                return !result;
            });
            m_IsIterating = false;
            ProcessDelta();
            return Updates.Count > 0;
        }
    }

}