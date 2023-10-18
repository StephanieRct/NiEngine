using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine.Recording
{
    public class EventRecord
    {
        public EventRecord Parent { get; }
        public int Id;
        public EventRecorder Recorder;
        public EventSource Source;
        public Owner Owner;
        public EventTimeStamp TimeStamp;

        public bool IsRoot => Id == 0;
        //public Action<EventRecord> OnNewChild;
        public enum PhaseEnum
        {
            Condition,
            Act,
            OnBegin,
            OnUpdate,
            OnEnd,
        };
        public PhaseEnum Phase;
        public EventParameters Parameters;

        //public List<IUpdate> UpdatesAdded = new();

        public int ChildCount => m_Children.Count;
        public EventRecord GetChildAt(int index) => m_Children[index];
        List<EventRecord> m_Children = new();

        public void AddChild(EventRecord child)
        {
            m_Children.Add(child);
            //OnNewChild?.Invoke(child);
        }
        public void RemoveChild(EventRecord child)
        {
            m_Children.Remove(child);
        }

        public EventRecord(EventRecorder recorder)
        {
            Recorder = recorder;
        }

        public EventRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, PhaseEnum phase, EventParameters parameters, EventTimeStamp time)
        {
            Parent = parent;
            Phase = phase;
            if (phase == PhaseEnum.OnUpdate || phase == PhaseEnum.Condition)
                Id = recorder.GetNegativeNextId();
            else
                Id = recorder.GetNextId();
            Recorder = recorder;
            Source = source;
            Owner = owner;
            Parameters = parameters;
            TimeStamp = time;
        }

        public virtual string DisplayName => $"EventRecord";
        public virtual string PhaseDisplayName => Phase.ToString();
        public virtual bool IsEmpty => m_Children.All(x => x.IsEmpty);
    }

    public class ConditionSetRecord : EventRecord
    {
        public bool Result;
        public ConditionSetRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, EventParameters parameters, EventTimeStamp time)
            : base(parent, recorder, source, owner, PhaseEnum.Condition, parameters, time)
        {
        }
        public override string DisplayName => "ConditionSet";
        public override string PhaseDisplayName => Phase.ToString();

    }

    public class ConditionRecord : EventRecord
    {
        public ICondition Condition;
        public bool Result;
        public ConditionRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, ICondition condition, EventParameters parameters, EventTimeStamp time)
            : base(parent, recorder, source, owner, PhaseEnum.Condition, parameters, time)
        {
            Condition = condition;
        }

        public override bool IsEmpty => false;
        public override string DisplayName => $"C:{Condition.GetType().FullName} is {Result}";
        public override string PhaseDisplayName => Phase.ToString();
    }

    public class ActionSetRecord : EventRecord
    {
        public ActionSetRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, PhaseEnum phase, EventParameters parameters, EventTimeStamp time)
            : base(parent, recorder, source, owner, phase, parameters, time)
        {
        }
        public override string DisplayName => "ActionSet";
        public override string PhaseDisplayName => Phase.ToString();
    }

    public class ActionRecord : EventRecord
    {
        public IAction Action;
        public ActionRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, IAction action, EventParameters parameters, EventTimeStamp time)
            : base(parent, recorder, source, owner, PhaseEnum.Act, parameters, time)
        {
            Action = action;
        }
        public override bool IsEmpty => Action == null;
        public override string DisplayName => $"A:{Action.GetType().FullName}.Act";
        public override string PhaseDisplayName => Phase.ToString();
    }

    public class StateActionRecord : EventRecord
    {
        public IStateAction Action;
        public StateActionRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, IStateAction action, PhaseEnum phase, EventParameters parameters, EventTimeStamp time)
            : base(parent, recorder, source, owner, phase, parameters, time)
        {
            Action = action;
        }
        public override bool IsEmpty => Action == null;
        public override string DisplayName => $"SA:{Action.GetType().FullName}.{Phase}";
        public override string PhaseDisplayName => Phase.ToString();
    }

    public class UpdateRecord : EventRecord
    {
        public IUpdate Update;
        public UpdateRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, IUpdate update, EventParameters parameters, EventTimeStamp time)
            : base(parent, recorder, source, owner, PhaseEnum.Act, parameters, time)
        {
            Update = update;
        }
        //public override bool IsEmpty => Update == null;
        public override string DisplayName => $"U:{Update.GetType().FullName}.Update";
        public override string PhaseDisplayName => Phase.ToString();
    }

    public class ReactionRecord : EventRecord
    {
        public string ReactionName;
        public GameObject Target;

        public string TargetName;
        //public ReactionRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, string reactionName, EventParameters parameters, EventTimeStamp time)
        //    : base(parent, recorder, source, owner, PhaseEnum.Act, parameters, time)
        //{
        //    ReactionName = reactionName;
        //}
        public ReactionRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, GameObject target, string reactionName, EventParameters parameters, EventTimeStamp time)
            : base(parent, recorder, source, owner, PhaseEnum.Act, parameters, time)
        {
            ReactionName = reactionName;
            Target = target;
            TargetName = target.GetNameOrNull();
        }

        public override bool IsEmpty => false;
        public override string DisplayName => $"Reaction:{ReactionName} -> {TargetName}";
        public override string PhaseDisplayName => "";
    }

    public class ErrorRecord : EventRecord
    {
        public enum LogTypeEnum
        {
            Log,
            Info,
            Warning,
            Error,
        }
        public LogTypeEnum LogType;
        public string ErrorId;
        public string Message;
        public UnityEngine.Object Reference;

        public ErrorRecord(EventRecord parent, EventRecorder recorder, EventSource source, Owner owner, LogTypeEnum logType, string errorId, string message, UnityEngine.Object reference, EventParameters parameters, EventTimeStamp time)
            : base(parent, recorder, source, owner, PhaseEnum.Act, parameters, time)
        {
            LogType = logType;
            Message = message;
            Reference = reference;
            ErrorId = errorId;
        }
        public override string DisplayName => $"[{LogType}:{ErrorId}]: {Message}";
        public override string PhaseDisplayName => $"{LogType}:{ErrorId}";
    }
}