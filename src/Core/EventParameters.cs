using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using NiEngine.Recording;
using UnityEngine;
using UnityEngine.Events;
using NiEngine.Expressions;
using NiEngine.Expressions.GameObjects;
using NiEngine.IO;
using System.Security.Cryptography;

namespace NiEngine
{

    [Serializable]
    public struct EventParameters
    {
        [Serializable]
        public struct ParameterSet
        {
            /// <summary>
            /// Object sending the event
            /// </summary>
            public GameObject From;
            /// <summary>
            /// Object that triggered the event.
            /// may be different if an event is passed around between objects
            /// </summary>
            public GameObject TriggerObject;
            public Vector3 TriggerPosition;

            public bool IsSame(ParameterSet other) => From == other.From && TriggerObject == other.TriggerObject && TriggerPosition == other.TriggerPosition;
            public override string ToString()
            {
                return $"(From: '{From.GetNameOrNull()}', TriggerObject: '{TriggerObject.GetNameOrNull()}', TriggerPosition: {TriggerPosition}";
            }
            public string ToStringMultiline(string indent)
            {
                return $"{indent}From: [{From.GetNameOrNull()}]\n{indent}TriggerObject: [{TriggerObject.GetNameOrNull()}]\n{indent}TriggerPosition: {TriggerPosition}";
            }
            public static ParameterSet Default => new ParameterSet
            {
                From = null,
                TriggerObject = null,
                TriggerPosition = Vector3.zero,
            };
            public static ParameterSet Trigger(GameObject from, GameObject triggerObject, Vector3 position) => new ParameterSet
            {
                From = from,
                TriggerObject = triggerObject,
                TriggerPosition = position,
            };
            public static ParameterSet Trigger(GameObject from, GameObject triggerObject) => new ParameterSet
            {
                From = from,
                TriggerObject = triggerObject,
                TriggerPosition = Vector3.zero,
            };
            public static ParameterSet WithoutTrigger(GameObject from) => new ParameterSet
            {
                From = from,
                TriggerObject = null,
                TriggerPosition = Vector3.zero,
            };
        }


        [Serializable]
        public struct Overrides
        {
            [Tooltip("Override 'Trigger' parameter. Leave to 'null' to not override")]
            [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
            public IExpressionGameObject Trigger;
            [Tooltip("Override 'From' parameter. Leave to 'null' to not override")]
            [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
            public IExpressionGameObject From;

        }
        /// <summary>
        /// The object on which the event happens
        /// </summary>
        public GameObject Self;


        public ParameterSet Current;
        public ParameterSet OnBegin;

        public Recording.EventSource RecordEventSource;
        NiReference m_Location;
        public Recording.EventSource MakeNewSource()
        {
            if (Recording.EventRecorder.Instance != null)
            {
                RecordEventSource = new Recording.EventSource(Recording.EventRecorder.Instance);
            }
            return RecordEventSource;
        }

        public Recording.EventSource AttachOrNewSource()
        {
            if (Recording.EventRecorder.Instance != null && RecordEventSource == null)
            {
                RecordEventSource = new Recording.EventSource(Recording.EventRecorder.Instance);
            }
            return RecordEventSource;
        }

        ///// <summary>
        ///// Log something that must be fixed by the developer
        ///// </summary>
        //public void LogError(Owner owner, string id, string message, UnityEngine.Object reference)
        //{
        //    if (Recording.EventRecorder.Instance != null && RecordEventSource != null)
        //        RecordEventSource.RecordLog(owner, this, ErrorRecord.LogTypeEnum.Error, id, message, reference);
        //    Debug.LogError($"{message}\n(Owner:{owner})\n{ToStringMultiline("")}", reference);
        //}



        public void LogError(NiReference location, Owner owner, string id, string message, UnityEngine.Object reference)
        {
            if (Recording.EventRecorder.Instance != null && RecordEventSource != null)
                RecordEventSource.RecordLog(owner, this, ErrorRecord.LogTypeEnum.Error, id, message, reference);
            Debug.LogError($"{message}\n(Owner:{owner})\n{ToStringMultiline("")} loc:{location?.ToString()}", reference);
        }
        public void LogError(NiReference location, Owner owner, string id)
            => LogError(location, owner, id, id, owner.GameObject);
        public void LogError(NiReference location, Owner owner, string id, string message)
            => LogError(location, owner, id, message, owner.GameObject);



        /// <summary>
        /// Log something that might require developer attention
        /// </summary>
        public void LogWarning(Owner owner, string id, string message, UnityEngine.Object reference)
        {
            if (Recording.EventRecorder.Instance != null && RecordEventSource != null)
                RecordEventSource.RecordLog(owner, this, ErrorRecord.LogTypeEnum.Warning, id, message, reference);
            Debug.LogWarning($"{message}\n(Owner:{owner})\n{ToStringMultiline("")}", reference);
        }
        public void LogWarning(Owner owner, string id, string message)
            => LogWarning(owner, id, message, owner.GameObject);

        /// <summary>
        /// Log any information about usage of the API
        /// </summary>
        public void LogInfo(Owner owner, string id, string message, UnityEngine.Object reference)
        {
            if (Recording.EventRecorder.Instance != null && RecordEventSource != null)
                RecordEventSource.RecordLog(owner, this, ErrorRecord.LogTypeEnum.Info, id, message, reference);
            Debug.Log($"{message}\n(Owner:{owner})\n{ToStringMultiline("")}", reference);
        }
        public void LogInfo(Owner owner, string id, string message)
            => LogInfo(owner, id, message, owner.GameObject);

        /// <summary>
        /// Log custom message
        /// </summary>
        public void Log(Owner owner, string id, string message, UnityEngine.Object reference)
        {
            if (Recording.EventRecorder.Instance != null && RecordEventSource != null)
                RecordEventSource.RecordLog(owner, this, ErrorRecord.LogTypeEnum.Log, id, message, reference);
            Debug.Log($"{message}\n(Owner:{owner})\n{ToStringMultiline("")}", reference);
        }

        public void Log(Owner owner, string id, string message)
            => Log(owner, id, message, owner.GameObject);

        public EventParameters WithSelf(GameObject self)
            => WithSelf(self: self, from: Self);

        public EventParameters WithSelf(GameObject self, GameObject from)
        {
            var copy = this;
            copy.Current.From = from;
            copy.Self = self;
            return copy;
        }
        public EventParameters WithBegin(EventParameters.ParameterSet begin)
        {
            var copy = this;
            copy.OnBegin = begin;
            return copy;
        }

        public EventParameters WithOverrideTrigger(GameObject trigger)
        {
            var copy = this;
            copy.Current.TriggerObject = trigger;
            return copy;
        }
        public EventParameters WithOverride(GameObject from, GameObject trigger)
        {
            var copy = this;
            copy.Current.From = from;
            copy.Current.TriggerObject = trigger;
            return copy;
        }
        public EventParameters WithOverride(Owner owner, Overrides overrides)
        {
            var copy = this;
            copy.Current.From = overrides.From?.GetValue(owner, this) ?? copy.Current.From;
            copy.Current.TriggerObject = overrides.Trigger?.GetValue(owner, this) ?? copy.Current.TriggerObject;
            return copy;
        }


        /// <summary>
        /// Use the OnBegin trigger as the Current trigger
        /// </summary>
        /// <returns></returns>
        public EventParameters WithOnBeginTrigger()
        {
            var copy = this;
            copy.Current.From = OnBegin.From;
            copy.Current.TriggerPosition = OnBegin.TriggerPosition;
            copy.Current.TriggerObject = OnBegin.TriggerObject;
            return copy;
        }
        public override string ToString()
        {
            return $"(Self: '{Self.GetNameOrNull()}', Current:{Current}, OnBegin:{OnBegin}";
        }
        public string ToStringMultiline(string indent)
        {
            return $"{indent}Self: [{Self.GetNameOrNull()}]\n{indent}Current:\n{Current.ToStringMultiline(indent + "  ")}\n{indent}OnBegin:\n{OnBegin.ToStringMultiline(indent + "  ")}";
        }
        public void SetLocation(NiReference location)
        {
            m_Location = location;
        }
        public static EventParameters Default => new EventParameters
        {
            Self = null,
            Current = ParameterSet.Default,
            OnBegin = ParameterSet.Default,
        };
        public static EventParameters Parameters(GameObject self, ParameterSet parameters) => new EventParameters
        {
            Self = self,
            Current = parameters,
            OnBegin = parameters,
        };
        public static EventParameters Parameters(GameObject self, ParameterSet parametersCurrent, ParameterSet parametersOnBegin) => new EventParameters
        {
            Self = self,
            Current = parametersCurrent,
            OnBegin = parametersOnBegin,
        };
        public static EventParameters Trigger(GameObject selfAndfrom, GameObject triggerObject) => new EventParameters
        {
            Self = selfAndfrom,
            Current = ParameterSet.Trigger(selfAndfrom, triggerObject, Vector3.zero),
            OnBegin = ParameterSet.Trigger(selfAndfrom, triggerObject, Vector3.zero),
        };
        public static EventParameters Trigger(GameObject selfAndfrom, GameObject triggerObject, Vector3 position) => new EventParameters
        {
            Self = selfAndfrom,
            Current = ParameterSet.Trigger(selfAndfrom, triggerObject, position),
            OnBegin = ParameterSet.Trigger(selfAndfrom, triggerObject, position),
        };
        public static EventParameters Trigger(GameObject self, GameObject from, GameObject triggerObject) => new EventParameters
        {
            Self = self,
            Current = ParameterSet.Trigger(from, triggerObject, Vector3.zero),
            OnBegin = ParameterSet.Trigger(from, triggerObject, Vector3.zero),
        };
        public static EventParameters Trigger(GameObject self, GameObject from, GameObject triggerObject, Vector3 position) => new EventParameters
        {
            Self = self,
            Current = ParameterSet.Trigger(from, triggerObject, position),
            OnBegin = ParameterSet.Trigger(from, triggerObject, position),
        };
        public static EventParameters WithoutTrigger(GameObject selfAndfrom) => new EventParameters
        {
            Self = selfAndfrom,
            Current = ParameterSet.WithoutTrigger(selfAndfrom),
            OnBegin = ParameterSet.WithoutTrigger(selfAndfrom),
        };
        public static EventParameters WithoutTrigger(GameObject self, GameObject from) => new EventParameters
        {
            Self = self,
            Current = ParameterSet.WithoutTrigger(from),
            OnBegin = ParameterSet.WithoutTrigger(from),
        };
    }

    [Serializable]
    public struct Owner : ISaveOverride
    {
        public static implicit operator Owner(NiBehaviour mb) => new Owner(mb);
        public Owner(NiBehaviour niBehaviour)
        {
            NiBehaviour = niBehaviour;
            StateMachine = null;
            State = null;
        }
        //public static implicit operator Owner(IUidObject owner) => new Owner(owner);
        public Owner(IUidObject owner)
        {
            NiBehaviour = owner as NiBehaviour;
            StateMachine = null;
            State = null;
        }

        public Owner(ReactionStateMachine sm)
        {
            NiBehaviour = sm;
            StateMachine = sm;
            State = null;
        }
        public Owner(ReactionStateMachine.State state)
        {
            NiBehaviour = state.StateMachine;
            StateMachine = state.StateMachine;
            State = state;
        }

        public NiBehaviour NiBehaviour;
        public ReactionStateMachine StateMachine;
        public ReactionStateMachine.State State;
        public GameObject Self => NiBehaviour != null ? NiBehaviour.gameObject : StateMachine.gameObject;
        public GameObject GameObject => NiBehaviour != null ? NiBehaviour.gameObject : StateMachine.gameObject;
        public bool IsNull => NiBehaviour == null && StateMachine == null;
        public string GameObjectName
        {
            get
            {
                if (NiBehaviour != null)
                {
                    return Formatting.FormatShortName(NiBehaviour.GetNameOrNull());
                }
                if (StateMachine != null)
                {
                    return Formatting.FormatShortName(StateMachine.GetNameOrNull());
                }
                return "null";
            }
        }
        public string ComponentName
        {
            get
            {
                if (NiBehaviour != null)
                {
                    return Formatting.FormatShortName(NiBehaviour.GetType().FullName);
                }
                if (StateMachine != null)
                {
                    if (State != null)
                    {
                        return $"{Formatting.FormatShortName(StateMachine.GetType().FullName)}.State({State.StateName})";
                    }
                    return Formatting.FormatShortName(StateMachine.GetType().FullName);
                }
                return "null";
            }
        }
        public override string ToString()
        {
            if (NiBehaviour != null)
            {
                return $"{Formatting.FormatShortName(NiBehaviour)}.{NiBehaviour.GetType().FullName}";
            }
            if (StateMachine != null)
            {
                if (State != null)
                {
                    return $"{Formatting.FormatShortName(StateMachine)}\").State(\"{State.StateName}";
                }
                return $"{Formatting.FormatShortName(StateMachine)}\").StateMachine";
            }
            return "null";
        }

        public void AddUpdate(IUpdate update, EventParameters parameters)
        {
            var obj = GameObject;
            if (obj != null)
            {
                if (!obj.TryGetComponent<ActionUpdate>(out var actionUpdate))
                    actionUpdate = obj.AddComponent<ActionUpdate>();
                actionUpdate.Updates.AddUpdate(this, parameters, update);
            }
        }
        public void AddLateUpdate(ILateUpdate update, EventParameters parameters)
        {
            var obj = GameObject;
            if (obj != null)
            {
                if (!obj.TryGetComponent<ActionLateUpdate>(out var actionUpdate))
                    actionUpdate = obj.AddComponent<ActionLateUpdate>();
                actionUpdate.LateUpdates.AddUpdate(this, parameters, update);
            }
        }
        public void AddFixedUpdate(IFixedUpdate update, EventParameters parameters)
        {
            var obj = GameObject;
            if (obj != null)
            {
                if (!obj.TryGetComponent<ActionFixedUpdate>(out var actionUpdate))
                    actionUpdate = obj.AddComponent<ActionFixedUpdate>();
                actionUpdate.FixedUpdates.AddUpdate(this, parameters, update);
            }
        }
        public void RemoveUpdate(IUpdate update)
        {
            var obj = GameObject;
            if (obj != null)
            {
                if (obj.TryGetComponent<ActionUpdate>(out var actionUpdate))
                {
                    actionUpdate.Updates.RemoveUpdate(update);
                }
            }
        }
        public void RemoveLateUpdate(ILateUpdate update)
        {
            var obj = GameObject;
            if (obj != null)
            {
                if (obj.TryGetComponent<ActionLateUpdate>(out var actionUpdate))
                {
                    actionUpdate.LateUpdates.RemoveUpdate(update);
                }
            }
        }
        public void RemoveFixedUpdate(IFixedUpdate update)
        {
            var obj = GameObject;
            if (obj != null)
            {
                if (obj.TryGetComponent<ActionFixedUpdate>(out var actionUpdate))
                {
                    actionUpdate.FixedUpdates.RemoveUpdate(update);
                }
            }
        }

        public void Save(StreamContext context, IOutput io)
        {
            if (StateMachine)
            {
                io.Save(context, "rsm", StateMachine.Uid);
            }
            if(State != null)
            {
                io.Save(context, "group", State.StateGroup.Index);
                io.Save(context, "state", State.Index);
            }
            else if(NiBehaviour != null)
            {
                io.Save(context, "nb", NiBehaviour.Uid);
            }
        }

        public void Load(StreamContext context, IInput io)
        {
            if(io.TryLoad<Uid>(context, "rsm", out var rsmUid))
            {
                var rsm = NiEngine.IO.SaveOverrides.NiBehaviourSO.FindNiBehaviourByUid<ReactionStateMachine>(rsmUid);
                if (rsm == null)
                {
                    context.LogFailure($"Could not find ReactionStateMachine with Uid {rsmUid}");
                    return;
                }
                StateMachine = rsm;
                NiBehaviour = rsm;
                if (io.TryLoad<int>(context, "group", out var indexGroup))
                {
                    var indexState = io.Load<int>(context, "state");
                    State = rsm.Groups[indexGroup].States[indexState];
                }
                else
                {
                    State = null;
                }
                
            } else if (io.TryLoad<Uid>(context, "nb", out var nbUid))
            {
                var nb = NiEngine.IO.SaveOverrides.NiBehaviourSO.FindNiBehaviourByUid<NiBehaviour>(nbUid);
                if (nb == null)
                {
                    context.LogFailure($"Could not find NiBehaviour with Uid {nbUid}");
                    return;
                }
                NiBehaviour = nb;
                StateMachine = null;
                State = null;
            }
        }
    }
}