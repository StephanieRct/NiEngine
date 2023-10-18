using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine
{
    public class ReactOnInputKey : NiBehaviour
    {

        [Header("Input:")]
        [NotSaved]
        public KeyCode KeyCode;
        [NotSaved]
        public bool TriggerFromMainCamera = true;
        public ConditionSet Conditions;
        public StateActionSet OnKeyDown;
        public ActionSet OnKeyUp;
        public EventStateProcessor Processor;

        bool m_ReactedOnDown;

        public GameObject TargetObject => gameObject;
        public GameObject TriggerObject => TriggerFromMainCamera ? Camera.main.gameObject : gameObject;
        
        bool CanReact(EventParameters parameters)
        {
            if (!enabled) return false;
            return Processor.Pass(new (this), Conditions, parameters);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                var parameters = EventParameters.Trigger(gameObject, gameObject, TriggerObject);
                if (CanReact(parameters))
                {
                    m_ReactedOnDown = true;
                    Processor.Begin(new (this), OnKeyDown, parameters);
                }
            }
            if (Input.GetKeyUp(KeyCode))
            {
                var parameters = EventParameters.Trigger(gameObject, gameObject, TriggerObject);
                if (m_ReactedOnDown || CanReact(parameters))
                {
                    m_ReactedOnDown = false;
                    Processor.End(new(this), OnKeyDown, OnKeyUp, parameters);
                }
            }

        }
    }
}