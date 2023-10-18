using System.Collections;
using System.Collections.Generic;
using System;
using NiEngine.IO;
using UnityEngine;
using static NiEngine.ReactionStateMachine.StateGroup;

namespace NiEngine
{
    
    [Serializable,Save]
    public class ReactionStateMachine : NiBehaviour, IReactionReceiver
    {
        [Serializable]
        public struct GroupCall
        {
            public NameReference GroupName;
            public string ReturnState;
            public string FailedState;
            public string CancelState;
            public Owner Owner;
            public EventParameters Parameters;
            public YieldResult Result;
        }
        List<GroupCall> m_NewGroupCalls;
        List<GroupCall> m_YieldingGroupCalls;

        public bool ReactEnabled => enabled;
        [Serializable]
        public class State : ICloneable, ISerializationCallbackReceiver//: ISaveClassOverride
        {
            [NotSaved]
            public bool IsExpended;
            [NotSaved]
            public NameReference StateName;
            
            public bool IsActiveState;
            public bool Began;
            
            [NonSerialized]
            public ReactionStateMachine StateMachine;
            [NonSerialized]
            public StateGroup StateGroup;
            //[NotSaved]
            //public bool HasConditions = false;
            //public ConditionSet Conditions = new();

            public StateActionSet OnBegin = new();

            public ActionSet OnEnd = new();

            [HideInInspector, NonSerialized]
            public List<IStateObserver> StateObservers = new();


            [SerializeField, EditorField(runtimeOnly: true)]
            public EventStateProcessor Processor;

            public int Index => StateGroup.GetStateIndex(this);
            public object Clone()
            {
                var c = new State();
                c.IsExpended = this.IsExpended;
                c.StateName = this.StateName;
                c.IsActiveState = false;
                c.Began = false;
                //c.Conditions = (ConditionSet)Conditions.Clone();
                c.OnBegin = (StateActionSet)OnBegin.Clone();
                c.OnEnd = (ActionSet)OnEnd.Clone();
                return c;
            }

            public bool CanReact(EventParameters parameters)
            {
                if (IsActiveState && parameters.Current.IsSame(Processor.LastOnBeginParameters))
                    return false;
                //if (!Processor.Pass(parameters)) return false;
                return true;
            }

            public void Handshake(ReactionStateMachine owner, StateGroup group = null)
            {
                StateMachine = owner;
                StateGroup = group;
                //foreach (var action in Conditions.Conditions)
                //    Handshake(action);
                foreach (var action in OnBegin.Actions)
                    Handshake(action);
                foreach (var action in OnEnd.Actions)
                    Handshake(action);
                //Processor.Initialize(new(this), null, OnBegin, OnEnd);
            }

            void Handshake(object obj)
            {
                if (obj is IStateObserver observer)
                    StateObservers.Add(observer);
                if (obj is IInitialize initialize)
                    StateMachine.Initializers.Add(initialize);
            }


            public void Start(ReactionStateMachine owner, StateGroup group)
            {
                if (IsActiveState)
                {
                    if (Processor.LastOnBeginParameters.From == null)
                        Processor.LastOnBeginParameters.From = owner.gameObject;
                    var parameters = Processor.MakeBeginEvent(EventParameters.Parameters(owner.gameObject, Processor.LastOnBeginParameters));

                    if (Began)
                    {
                        group.HasActiveState = true;
                        group.CurrentStateIndex = StateGroup.States.IndexOf(this);
                        OnBegin.AddAllBeginIUpdate(new Owner(this), parameters);
                    }
                    else
                    {
                        group.SetActiveState(group.StateMachine, this, parameters);
                    }
                }
            }
            
            public void Begin(EventParameters parameters)
            {
                IsActiveState = true;
                if (!Began)
                {
                    Began = true;
                    Processor.Begin(new(this), OnBegin, parameters);

                    var owner = new Owner(this);
                    foreach (var observer in StateObservers)
                        observer.OnStateBegin(owner, parameters);
                }
            }

            public void End(EventParameters parameters)
            {
                Debug.Assert(Began);

                parameters = Processor.MakeEndEvent(parameters);

                var owner = new Owner(this);
                foreach (var observer in StateObservers)
                    observer.OnStateEnd(owner, parameters);

                Processor.End(new(this), OnBegin, OnEnd, parameters);

                IsActiveState = false;
                Began = false;
            }

            public void OnBeforeSerialize()
            {
            }

            public void OnAfterDeserialize()
            {
                //HasConditions = Conditions.Conditions.Count > 0;
            }


            //#region ISaveOverride
            //public void Save(SaveContext context, IDataOutput io)
            //{
            //    throw new NotImplementedException();
            //    //SavingState result = SavingState.Successful;
            //    //result.Merge(Saving.SerializeKeyValue(context, "name", StateName.Name, io));
            //    //result.Merge(Saving.SerializeKeyValue(context, "IsActiveState", IsActiveState, io));
            //    //return result;
            //}

            //public void Load(SaveContext context, IDataInput io)
            //{
            //    throw new NotImplementedException();
            //}
            //#endregion
        }

        [Serializable]
        public class StateGroup : ISaveOverride
        {
            public void Save(StreamContext context, IOutput io)
            {
                if (HasActiveState)
                {
                    io.Save(context, "HasActiveState", HasActiveState);
                    io.Save(context, "CurrentStateIndex", CurrentStateIndex);
                    if (!CurrentGroupCall.Owner.IsNull)
                    {
                        //io.Save(context, "CancelState", CancelState);
                        io.Save(context, "CurrentGroupCall", CurrentGroupCall);
                    }
                    //io.Save(context, "CancelState", CancelState);
                    
                    using (var scopeStates = context.ScopeKey("Sates", io))
                    {
                        for(int i = 0; i != States.Count; ++i)
                        {
                            var s = States[i];
                            if (s.IsActiveState)
                            {
                                io.SaveInPlace(context, i, s);
                            }
                        }
                    }

                }
            }

            public void Load(StreamContext context, IInput io)
            {
                if(!io.TryLoad(context, "HasActiveState", out HasActiveState))
                {
                    HasActiveState = false;
                    return;
                }
                io.LoadInPlace(context, "CurrentStateIndex", ref CurrentStateIndex);
                io.TryLoadInPlace(context, "CurrentGroupCall", ref CurrentGroupCall);
                //io.LoadInPlace(context, "CancelState", ref CancelState);
                HasYielded = false;
                for (int i = 0; i != States.Count; ++i)
                {
                    var s = States[i];
                    s.IsActiveState = false;
                    s.Began = false;

                }
                using (var scopeStates = context.ScopeKey("Sates", io))
                {
                    foreach(var k in io.Keys)
                    {
                        if(k is int i)
                        {
                            var s = States[i];
                            io.LoadInPlace(context, k, ref s);
                        }
                    }
                }
            }


            public int Index => StateMachine.GetGroupIndex(this);

            [NonSerialized]
            public ReactionStateMachine StateMachine;
            [NotSaved]
            public NameReference GroupName;
            public bool HasActiveState;
            [Save(saveInPlace: true)]
            public List<State> States = new();
            
            public State CurrentState => CurrentStateIndex < 0 ? null : States[CurrentStateIndex];

            [NonSerialized, Save]
            public int CurrentStateIndex = -1;

            [NonSerialized]
            private bool m_InTransition = false;
            //[NonSerialized]
            //State m_StateAfterTransition;
            [NonSerialized]
            Queue<(State, EventParameters)> m_StateAfterTransition = new();

            [NonSerialized]
            EventParameters m_ParametersAfterTransition;

            public GroupCall CurrentGroupCall;
            public string CancelState;

            public int GetStateIndex(State state)
            {
                return States.IndexOf(state);
            }
            public bool HasYielded { get; private set; }
            public enum YieldResult
            {
                Done,
                Failed,
                Canceled,
            }
            public void Call(GroupCall gc)
            {
                if (!CurrentGroupCall.Owner.IsNull)
                {
                    gc.Parameters.LogError(null, gc.Owner, "CallGroup", $"Cannot call state group '{gc.GroupName}'. The group has not yielded a previous call yet");
                    return;
                }
                //HasYielded = false;
                CurrentGroupCall = gc;
                SetActiveState(StateMachine, States[0], CurrentGroupCall.Parameters.WithSelf(StateMachine.gameObject));
            }
            public void Yield(YieldResult result)
            {
                var gc = CurrentGroupCall;
                CurrentGroupCall = default;
                if (!gc.Owner.IsNull)
                {
                    string resultState = result switch
                    {
                        YieldResult.Done => gc.ReturnState,
                        YieldResult.Failed => string.IsNullOrEmpty(gc.FailedState) ? gc.ReturnState : gc.FailedState,
                        YieldResult.Canceled => gc.CancelState,
                        _ => throw new NotImplementedException(),
                    };
                    if (!string.IsNullOrEmpty(resultState))
                        gc.Owner.GameObject.React(gc.Owner, resultState, gc.Parameters);
                }
                HasYielded = true;
            }
            public void BreakCall(Owner owner, EventParameters parameters)
            {
                if (CurrentGroupCall.Owner.IsNull)
                    return;
                HasYielded = true;
                var gc = CurrentGroupCall;
                CurrentGroupCall = default;
                DeactivateCurrentState(parameters);
                if (!string.IsNullOrEmpty(CancelState))
                    React(CancelState, gc.Parameters);
                if (!string.IsNullOrEmpty(gc.CancelState))
                    gc.Owner.GameObject.React(gc.Owner, gc.CancelState, gc.Parameters);
            }
            public StateGroup()
            {
            }
            public StateGroup(NameReference name)
            {
                GroupName = name;
            }

            public void InsertStateAt(int index, State state)
            {
                States.Insert(index, state);
            }
            public void SetStateAt(int index, State state)
            {
                States[index] = state;
            }


            public State AddState(NameReference name)
            {
                var state = new State();
                state.StateName = name;
                state.StateMachine = StateMachine;
                state.StateGroup = this;
                States.Add(state);
                return state;
            }

            public bool HasState(NameReference name)
            {
                foreach (var s in States)
                    if (s.StateName == name)
                        return true;
                return false;
            }
            public bool IsStateActive(NameReference name)
            {
                foreach (var s in States)
                    if (s.StateName == name)
                        return s.IsActiveState;
                return false;
            }

            public IEnumerable<State> AllStatesNamed(NameReference name)
            {
                foreach (var s in States)
                    if (s.StateName == name)
                        yield return s;
            }
            public int React(string reactionOrStateName, EventParameters parameters) 
            {
                int count = 0;
                foreach (var state in AllStatesNamed(reactionOrStateName))
                {
                    if (state.CanReact(parameters))
                    {
                        SetActiveState(StateMachine, state, parameters);
                        ++count;
                    }
                    else if (state.IsActiveState)
                    {
                        ++count;
                    }
                }
                return count;
            }
            public bool NextState(Owner owner, EventParameters parameters)
            {
                int newIndex;
                if (CurrentStateIndex < 0)
                {
                    newIndex = 1;
                }
                else
                {
                    newIndex = CurrentStateIndex + 1;
                    DeactivateCurrentState(parameters);
                }
                if(newIndex < States.Count)
                {
                    SetActiveState(StateMachine, States[newIndex], parameters);
                    return true;
                }
                else
                {
                    Yield(YieldResult.Done);
                    return false;
                }
            }
            public void DeactivateCurrentState(EventParameters parameters)
            {
                Debug.Assert(parameters.Self == StateMachine.gameObject);
                if(CurrentStateIndex >= 0)
                {
                    States[CurrentStateIndex].IsActiveState = false;
                    States[CurrentStateIndex].End(parameters);
                    HasActiveState = false;
                    CurrentStateIndex = -1;
                }
            }
            public void DeactivateAllState(EventParameters parameters)
            {
                Debug.Assert(parameters.Self == StateMachine.gameObject);
                foreach (var s in States)
                {
                    if (s.IsActiveState)
                    {
                        s.IsActiveState = false;
                        s.End(parameters);
                    }
                }
                HasActiveState = false;
                CurrentStateIndex = -1;
            }
            public void SetActiveState(ReactionStateMachine component, State state, EventParameters parameters)
            {

                if (StateMachine == null)
                {
                    parameters.LogError(null, new(component), "RSM.MissingHandshake", $"Cannot set active state to '{state?.StateName}' on state machine '{component.GetNameOrNull()}'. Missing handshake", component);
                    return;
                }
                Debug.Assert(parameters.Self == StateMachine.gameObject);
                Debug.Assert(parameters.Current.From != null);


                if (m_InTransition)
                {
                    if (m_StateAfterTransition.Count >= 250)
                    {
                        parameters.LogError(null, new(component), "RSM.RecursiveTransition",
                            $"Recursive transition reach its limit of 250",
                            component.gameObject);
                        return;
                    }
                    m_StateAfterTransition.Enqueue((state, parameters));
                    //m_StateAfterTransition = state;
                    //m_ParametersAfterTransition = parameters;
                    return;
                }
                m_InTransition = true;

#if UNITY_EDITOR
                if (state?.StateGroup != this || state.StateMachine != component)
                    Debug.LogError($"ReactionStateMachine '{component.name}' switches to an unknown state '{state?.StateName.Name}'", component);
#endif

                HasYielded = false;
                CurrentState?.End(parameters);
                CurrentStateIndex = States.IndexOf(state);
                HasActiveState = state != null;
                state?.Begin(parameters);
                m_InTransition = false;
                if(m_StateAfterTransition.TryDequeue(out var nn))
                {
                    SetActiveState(component, nn.Item1, nn.Item2);
                }
            }

            public bool SetInitialState(string stateName)
            {
                foreach (var s in States)
                {
                    if (s.StateName == stateName)
                    {
                        foreach (var s2 in States)
                        {
                            if (s.Began)
                                s.End(EventParameters.WithoutTrigger(StateMachine?.gameObject, StateMachine?.gameObject));
                            s2.IsActiveState = false;
                            s.Began = false;
                        }
                        s.IsActiveState = true;
                        HasActiveState = false;
                        CurrentStateIndex = -1;
                        return true;
                    }
                }

                return false;
            }

            public void Handshake(ReactionStateMachine owner)
            {
                StateMachine = owner;
                foreach (var state in States)
                    state.Handshake(owner, this);
            }
            public void Start(ReactionStateMachine owner)
            {
                StateMachine = owner;
                foreach (var state in States)
                    state.Start(owner, this);
            }

        }


        public int GetGroupIndex(StateGroup group)
        {
            return Groups.IndexOf(group);
        }

        [Save(saveInPlace: true)]
        public List<StateGroup> Groups = new();

        [NonSerialized]
        List<IInitialize> Initializers = new();

        [NonSerialized]
        private bool m_Booted = false;
        private bool m_Started = false;
        public static bool TryGetGroup(GameObject obj, NameReference name, out StateGroup group)
        {
            foreach(var sm in obj.GetComponents<ReactionStateMachine>())
            {
                
                if (sm.TryGetGroup(name, out group))
                    return true;
            }
            group = default;
            return false;
        }
        public static int AddStateObserve(GameObject obj, NameReference name, IStateObserver observer)
        {
            int count = 0;
            foreach (var sm in obj.GetComponents<ReactionStateMachine>())
                foreach (var g in sm.Groups)
                    foreach(var s in g.AllStatesNamed(name))
                    {
                        s.StateObservers.Add(observer);
                        ++count;
                    }
            return count;
        }
        public static int RemoveStateObserve(GameObject obj, NameReference name, IStateObserver observer)
        {
            int count = 0;
            foreach (var sm in obj.GetComponents<ReactionStateMachine>())
                foreach (var g in sm.Groups)
                    foreach (var state in g.AllStatesNamed(name))
                    {
                        if(state.StateObservers.Remove(observer))
                            ++count;
                    }
            return count;
        }

        public StateGroup AddGroup(NameReference name)
        {
            var group = new StateGroup();
            group.GroupName = name;
            group.StateMachine = this;
            Groups.Add(group);
            return group;
        }
        public bool TryGetGroup(NameReference name, out StateGroup group)
        {
            foreach (var g in Groups)
            {
                if (g.GroupName == name)
                {
                    group = g;
                    return true;
                }
            }

            group = default;
            return false;
        }
        public IEnumerable<(StateGroup, State)> AllStatesNamed(NameReference name)
        {
            foreach (var g in Groups)
                foreach (var s in g.AllStatesNamed(name))
                    yield return (g,s);
        }

        public bool HasReaction(string reactionName, bool onlyEnabled, bool onlyActive, int maxLoop)
        {
            if (onlyEnabled && !ReactEnabled) return false;

            foreach (var g in Groups)
                foreach (var s in g.AllStatesNamed(reactionName))
                {
                    if (onlyActive)
                    {
                        if (s.IsActiveState) 
                            return true;
                    }
                    else
                        return true;
                }

            return false;
        }
        public bool HasState(NameReference name)
        {
            foreach (var group in Groups)
                if (group.HasState(name))
                    return true;
            return false;
        }

        public bool IsStateActive(NameReference GroupName, NameReference stateName)
        {
            foreach (var g in Groups)
                if(g.GroupName == GroupName)
                    return g.IsStateActive(stateName);
            return false;
        }
        public bool IsStateActive(NameReference stateName)
        {
            foreach (var g in Groups)
                if (g.IsStateActive(stateName))
                    return true;
            return false;
        }
        public bool CanReact(string reactionOrStateName, EventParameters parameters)
        {
            Debug.Assert(parameters.Self == gameObject);
            parameters = parameters.WithSelf(gameObject);
            foreach (var group in Groups)
                foreach (var state in group.AllStatesNamed(name))
                    if (state.CanReact(parameters))
                        return true;
            return false;
        }

        public void ForceActivateState(string stateName)
        {
            ReactionReference.React(new Owner(this), stateName, EventParameters.Default.WithSelf(gameObject, gameObject));
        }

        public bool SetInitialState(string groupName, string stateName)
        {
            int count = 0;
            foreach (var group in Groups)
                if (group.GroupName == groupName)
                    if (group.SetInitialState(stateName))
                        ++count;
            return count > 0;
        }

        public int React(string reactionOrStateName, EventParameters parameters)
        {
            Debug.Assert(parameters.Self == gameObject);
            Debug.Assert(parameters.Current.From != null);
            int count = 0;
            foreach (var group in Groups)
            {
                count += group.React(reactionOrStateName, parameters);
            }
            return count;
        }

        public void DeactivateAllStateOfGroup(string groupName, EventParameters parameters)
        {
            Debug.Assert(parameters.Self == gameObject);
            foreach (var group in Groups)
                if (group.GroupName == new NameReference(groupName))
                    group.DeactivateAllState(parameters);
        }


        public void CallGroup(StateGroup group, Owner owner, EventParameters parameters, string returnState, string failedState, string cancelState)
        {
            if (group.States.Count == 0)
            {
                owner.GameObject.React(owner, returnState, parameters);
                return;
            }
            if (!group.CurrentGroupCall.Owner.IsNull)
            {
                parameters.LogError(null, owner, "CallGroup", $"Cannot call state group '{group.GroupName}'. The group has not yielded a previous call yet");
                return;
            }
            if (m_NewGroupCalls == null)
                m_NewGroupCalls = new();

            m_NewGroupCalls.Add(new GroupCall
            {
                GroupName = group.GroupName,
                ReturnState = returnState,
                FailedState = failedState,
                CancelState = cancelState,
                Owner = owner,
                Parameters = parameters,
                Result = YieldResult.Done,
            });
            ProccessPending();
        }
        public void YieldGroup(StateGroup group, YieldResult result)
        {
            if (group.CurrentGroupCall.Owner.IsNull)
                return;
            group.CurrentGroupCall.Result = result;
            if (m_YieldingGroupCalls == null)
                m_YieldingGroupCalls = new();
            m_YieldingGroupCalls.Add(group.CurrentGroupCall);
            ProccessPending();
        }

        public void BreakCall(StateGroup group, Owner owner, EventParameters parameters)
        {
            if (group.CurrentGroupCall.Owner.IsNull)
                return;
            group.CurrentGroupCall.Result = YieldResult.Canceled;
            if (m_YieldingGroupCalls == null)
                m_YieldingGroupCalls = new();
            m_YieldingGroupCalls.Add(group.CurrentGroupCall);
            ProccessPending();
        }
        void ProccessPending()
        {
            if (m_NewGroupCalls != null)
            {
                var calls = m_NewGroupCalls;
                m_NewGroupCalls = null;
                foreach (var gc in calls)
                {
                    if (TryGetGroup(gc.GroupName, out var group))
                    {
                        group.Call(gc);
                    }
                }
            }
            if (m_YieldingGroupCalls != null)
            {
                var calls = m_YieldingGroupCalls;
                m_YieldingGroupCalls = null;
                foreach (var gc in calls)
                {
                    if (TryGetGroup(gc.GroupName, out var group))
                    {
                        group.Yield(gc.Result);
                    }
                }
            }
        }
        public void Boot()
        {
            Awake();
            Start();
            m_Booted = true;
        }
        #region Unity Callback
        private void Start()
        {
            //Debug.Log($"Start: {gameObject.GetPathNameOrNull()}");
            if (m_Booted)
                return;
            if(!m_Started)
                foreach (var group in Groups)
                    group.Start(this);
            m_Started = true;
        }
        private void Awake()
        {
            //Debug.Log($"Awake: {gameObject.GetPathNameOrNull()}");
            if (m_Booted)
                return;
            foreach (var group in Groups)
                group.Handshake(this);

            var owner = new Owner(this);
            foreach (var i in Initializers)
                i.Initialize(owner);
        }
#if UNITY_EDITOR
        [NonSerialized]
        public string StateToToggleOnUpdate;
#endif
        void Update()
        {
            ProccessPending();
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(StateToToggleOnUpdate))
            {
                var stateToToggle = StateToToggleOnUpdate;
                StateToToggleOnUpdate = null;
                foreach (var (group, state) in AllStatesNamed(stateToToggle))
                {
                    var parameters = EventParameters.WithoutTrigger(gameObject, gameObject);
                    if (state.IsActiveState)
                    {
                        Debug.Log($"Deactivating State '{state.StateName.Name}' on gameObject '{gameObject.name}' without trigger object.");
                        group.DeactivateAllState(parameters);
                    }
                    else
                    {
                        Debug.Log($"Activating State '{state.StateName.Name}' on gameObject '{gameObject.name}' without trigger object.");
                        group.DeactivateAllState(parameters);
                        group.SetActiveState(this, state, parameters);
                    }
                }
            }
#endif

        }
        #endregion

        public override bool TryFindUidObject(Uid uid, out IUidObject uidObject)
        {
            foreach(var g in Groups)
            {
                foreach(var s in g.States)
                {
                    if (TryFindUidObjectInIUidObject(s.OnBegin, uid, out uidObject))
                        return true;
                    if (TryFindUidObjectInIUidObject(s.OnEnd, uid, out uidObject))
                        return true;
                }
            }
            uidObject = default;
            return false;
        }
        //#region ISaveOverride
        //public void Save(SaveContext context, IDataOutput io)
        //{
        //    throw new NotImplementedException();
        //    //SavingState result = SavingState.Successful;
        //    //result.Merge(Saving.SerializeKeyListInPlace(context, "groups", Groups, io));
        //    //return result;
        //}

        //public void Load(SaveContext context, IDataInput io)
        //{
        //    throw new NotImplementedException();
        //    //LoadingState result = LoadingState.Successful;
        //    //result.Merge(Saving.DeserializeKeyListInPlace(context, "groups", Groups, io));
        //    //return result;
        //    //LoadingState result = LoadingState.Successful;
        //    //foreach (var g in Groups)
        //    //{
        //    //    using var scope = SaveScope.KeyValue(context, "group", g.GroupName, io);
        //    //    result.Merge(g.Load(context, io));
        //    //}
        //    //
        //    //return result;
        //}
        //#endregion
    }
}