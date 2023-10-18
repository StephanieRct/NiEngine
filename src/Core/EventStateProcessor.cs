using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using NiEngine.Recording;

namespace NiEngine
{

    [Serializable]
    public struct EventStateProcessor
    {

        [NonSerialized]
        Recording.EventSource m_CurrentEventSource;

        public EventParameters.ParameterSet LastOnBeginParameters;

        public EventParameters NewEvent(GameObject self, EventParameters.ParameterSet parameters)
        {
            var ep = EventParameters.Parameters(self, parameters);
            m_CurrentEventSource = ep.MakeNewSource();
            return ep;
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
        EventParameters ContinueRecordSource(EventParameters parameters)
        {
            if (m_CurrentEventSource != null)
            {
                parameters.RecordEventSource = m_CurrentEventSource;
            }
            else if (parameters.RecordEventSource != null)
            {
                m_CurrentEventSource = parameters.RecordEventSource;
            }
            else
            {
                m_CurrentEventSource = parameters.MakeNewSource();
            }
            return parameters;
        }

        public EventParameters MakeConditionEvent(GameObject self, EventParameters.ParameterSet parameters)
            => EventParameters.Parameters(self, parameters);
        public EventParameters MakeBeginEvent(GameObject self, EventParameters.ParameterSet parameters)
            => NewEvent(self, parameters);
        public EventParameters MakeBeginEvent(EventParameters parameters)
            => AttachOrNewRecordSource(parameters);
        public EventParameters MakeEndEvent(GameObject self, EventParameters.ParameterSet parameters)
            => ContinueRecordSource(EventParameters.Parameters(self, parameters, LastOnBeginParameters));

        public EventParameters MakeEndEvent(EventParameters parameters)
            => ContinueRecordSource(parameters.WithBegin(LastOnBeginParameters));
        public bool Pass(Owner owner, ConditionSet conditions, EventParameters parameters, bool logFalseResults = false)
        {
            if (conditions == null)
                return true;

            // Does not create new record source since it would spam with false results
            // TODO keep the record and attach the begin/end events to it if the result of the conditions is true
            parameters.RecordEventSource?.BeginRecordConditionSet(owner, parameters);
            bool pass = conditions.Pass(owner, parameters, logFalseResults);
            parameters.RecordEventSource?.EndRecordConditionSet(result: pass);
            
            return pass;
        }

        public EventParameters Begin(Owner owner, StateActionSet onBeginOrNull, EventParameters parameters)
        {
            if (onBeginOrNull != null)
            {
                parameters = AttachOrNewRecordSource(parameters);
                parameters.RecordEventSource?.BeginRecordActionSet(owner, EventRecord.PhaseEnum.OnBegin, parameters);
                onBeginOrNull.OnBegin(owner, parameters);
                parameters.RecordEventSource?.EndRecordActionSet();
            }
            LastOnBeginParameters = parameters.Current;
            return parameters;
        }

        public EventParameters End(Owner owner, StateActionSet onBeginOrNull, ActionSet onEndOrNull, EventParameters parameters, EventParameters.ParameterSet parametersOnBegin)
        {
            if (onBeginOrNull != null || onEndOrNull != null)
            {
                parameters = ContinueRecordSource(parameters.WithBegin(parametersOnBegin));
                parameters.RecordEventSource?.BeginRecordActionSet(owner, EventRecord.PhaseEnum.OnEnd, parameters);
                onBeginOrNull?.OnEnd(owner, parameters);
                onEndOrNull?.Act(owner, parameters);
                parameters.RecordEventSource?.EndRecordActionSet();
                m_CurrentEventSource = null;
            }
            return parameters;
        }
        public EventParameters End(Owner owner, StateActionSet onBeginOrNull, ActionSet onEndOrNull, EventParameters parameters)
            => End(owner, onBeginOrNull, onEndOrNull, parameters, LastOnBeginParameters);



        public int Act(Owner owner, ActionSet actions, EventParameters parameters)
        {
            if (actions != null)
            {
                parameters.RecordEventSource?.BeginRecordActionSet(owner, EventRecord.PhaseEnum.Act, parameters);
                actions.Act(owner, parameters);
                parameters.RecordEventSource?.EndRecordActionSet();
                return 1;
            }

            return 0;
        }
    }
    
}