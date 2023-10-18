using System;
using System.Collections.Generic;
using System.Linq;
using NiEngine.Recording;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine
{

    [Serializable]
    public struct EventProcessor
    {

        [NonSerialized]
        Recording.EventSource m_CurrentEventSource;

        public EventParameters.ParameterSet LastOnBeginEvent;
        

        public EventParameters MakeEvent(Owner owner, GameObject Trigger, Vector3 position)
        {
            var parameters = EventParameters.Trigger(owner.Self, owner.Self, Trigger, position);
            parameters.RecordEventSource = m_CurrentEventSource;
            return parameters;
        }

        public EventParameters MakeEvent(Owner owner, GameObject Trigger)
        {
            var parameters = EventParameters.Trigger(owner.Self, owner.Self, Trigger);
            parameters.RecordEventSource = m_CurrentEventSource;
            return parameters;
        }

        public EventParameters MakeEvent(Owner owner)
        {
            var parameters = EventParameters.WithoutTrigger(owner.Self, owner.Self);
            parameters.RecordEventSource = m_CurrentEventSource;
            return parameters;
        }

        public bool Pass(Owner owner, ConditionSet conditions, EventParameters parameters)
        {
            if (conditions == null)
                return true;

            parameters.RecordEventSource?.BeginRecordConditionSet(owner, parameters);
            bool pass = conditions.Pass(owner, parameters);
            parameters.RecordEventSource?.EndRecordConditionSet(pass);

            return pass;
        }

        public int Act(Owner owner, ActionSet actions, EventParameters parameters)
        {
            if (actions != null)
            {
                parameters = AttachOrNewRecordSource(parameters);
                parameters.RecordEventSource?.BeginRecordActionSet(owner, EventRecord.PhaseEnum.Act, parameters);
                actions.Act(owner, parameters);
                parameters.RecordEventSource?.EndRecordActionSet();
                return 1;
            }

            return 0;
        }

        public int React(Owner owner, ConditionSet conditions, ActionSet actions, EventParameters parameters)
        {
            if (!Pass(owner, conditions, parameters))
                return 0;

            return Act(owner, actions, parameters);

        }


        EventParameters NewRecordSource(EventParameters parameters)
        {
            m_CurrentEventSource = parameters.MakeNewSource();
            return parameters;
        }
        EventParameters AttachOrNewRecordSource(EventParameters parameters)
        {
            if (parameters.RecordEventSource != null)
            {
                m_CurrentEventSource = parameters.RecordEventSource;
                return parameters;
            }
            else
                return NewRecordSource(parameters);
        }
    }
}