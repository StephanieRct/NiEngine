using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine.Recording
{
    public class EventSource
    {
        public EventRecorder Recorder;

        public EventRecord CurrentRecord;

        public EventSource(EventRecorder recorder)
        {
            Recorder = recorder;
            CurrentRecord = recorder.Root;
        }
        public void RecordLog(Owner owner, EventParameters parameters, ErrorRecord.LogTypeEnum logType, string id, string message, UnityEngine.Object reference)
        {
            var record = new ErrorRecord(CurrentRecord, Recorder, this, owner, logType, id, message, reference, parameters, EventTimeStamp.Now);
            CurrentRecord?.AddChild(record);
        }
        public EventRecord SwapCurrentRecord(EventRecord record)
        {
            var previous = CurrentRecord;
            CurrentRecord = record;
            return previous;
        }
        public EventRecord BeginRecordConditionSet(Owner owner, EventParameters parameters)
        {
            if (Recorder.IgnoreConditions) return CurrentRecord;
            var record = new ConditionSetRecord(CurrentRecord, Recorder, this, owner, parameters, EventTimeStamp.Now);
            CurrentRecord.AddChild(record);
            CurrentRecord = record;
            return CurrentRecord;
        }
        public void EndRecordConditionSet(bool result)
        {
            if (Recorder.IgnoreConditions) return;
            if (Recorder.IgnoreEmptyConditions && CurrentRecord.IsEmpty)
                CurrentRecord.Parent.RemoveChild(CurrentRecord);
            else
                (CurrentRecord as ConditionSetRecord).Result = result;
            CurrentRecord = CurrentRecord.Parent;
        }

        public EventRecord BeginRecordCondition(Owner owner, ICondition condition, EventParameters parameters)
        {
            if (Recorder.IgnoreConditions) return CurrentRecord;
            var record = new ConditionRecord(CurrentRecord, Recorder, this, owner, condition, parameters, EventTimeStamp.Now);
            CurrentRecord.AddChild(record);
            CurrentRecord = record;
            return CurrentRecord;
        }
        public void EndRecordCondition(bool result)
        {
            if (Recorder.IgnoreConditions) return;
            if (Recorder.IgnoreEmptyConditions && CurrentRecord.IsEmpty)
                CurrentRecord.Parent.RemoveChild(CurrentRecord);
            else
                (CurrentRecord as ConditionRecord).Result = result;
            CurrentRecord = CurrentRecord.Parent;
        }




        public EventRecord BeginRecordActionSet(Owner owner, EventRecord.PhaseEnum phase, EventParameters parameters)
        {
            if (Recorder.IgnoreActionSets) return CurrentRecord;
            var record = new ActionSetRecord(CurrentRecord, Recorder, this, owner, phase, parameters, EventTimeStamp.Now);
            CurrentRecord.AddChild(record);
            CurrentRecord = record;
            return CurrentRecord;
        }
        public void EndRecordActionSet()
        {
            if (Recorder.IgnoreActionSets) return;
            if (Recorder.IgnoreEmptyUpdate && CurrentRecord.Phase == EventRecord.PhaseEnum.OnUpdate && CurrentRecord.IsEmpty)
                CurrentRecord.Parent.RemoveChild(CurrentRecord);
            CurrentRecord = CurrentRecord.Parent;
        }

        public EventRecord BeginRecordAction(Owner owner, IAction action, EventParameters parameters)
        {
            var record = new ActionRecord(CurrentRecord, Recorder, this, owner, action, parameters, EventTimeStamp.Now);
            CurrentRecord.AddChild(record);
            CurrentRecord = record;
            return CurrentRecord;
        }
        public void EndRecordAction()
        {
            if (CurrentRecord.IsEmpty)
                CurrentRecord.Parent.RemoveChild(CurrentRecord);
            CurrentRecord = CurrentRecord.Parent;
        }
        public EventRecord BeginRecordStateAction(Owner owner, IStateAction action, EventRecord.PhaseEnum phase, EventParameters parameters)
        {
            var record = new StateActionRecord(CurrentRecord, Recorder, this, owner, action, phase, parameters, EventTimeStamp.Now);
            CurrentRecord.AddChild(record);
            CurrentRecord = record;
            return CurrentRecord;
        }
        public void EndRecordStateAction()
        {
            if (CurrentRecord.IsEmpty)
                CurrentRecord.Parent.RemoveChild(CurrentRecord);
            CurrentRecord = CurrentRecord.Parent;
        }

        public EventRecord BeginRecordUpdate(Owner owner, IUpdate update, EventParameters parameters)
        {
            var record = new UpdateRecord(CurrentRecord, Recorder, this, owner, update, parameters, EventTimeStamp.Now);
            CurrentRecord.AddChild(record);
            CurrentRecord = record;
            return CurrentRecord;
        }
        public void EndRecordUpdate()
        {
            if (Recorder.IgnoreEmptyUpdate && CurrentRecord.IsEmpty)
                CurrentRecord.Parent.RemoveChild(CurrentRecord);
            CurrentRecord = CurrentRecord.Parent;
        }
        public EventRecord BeginRecordReaction(Owner owner, string name, EventParameters parameters)
        {
            var record = new ReactionRecord(CurrentRecord, Recorder, this, owner, parameters.Self, name, parameters, EventTimeStamp.Now);
            CurrentRecord.AddChild(record);
            CurrentRecord = record;
            return CurrentRecord;
        }
        public EventRecord BeginRecordReaction(Owner owner, GameObject target, string name, EventParameters parameters)
        {
            var record = new ReactionRecord(CurrentRecord, Recorder, this, owner, target, name, parameters, EventTimeStamp.Now);
            CurrentRecord.AddChild(record);
            CurrentRecord = record;
            return CurrentRecord;
        }

        public void EndRecordReaction()
        {
            CurrentRecord = CurrentRecord.Parent;
        }

    }
}