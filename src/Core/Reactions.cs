using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine
{

    /// <summary>
    /// Implement by Condition, Action and StateAction to 
    /// observe when the owner state begins and ends
    /// </summary>
    public interface IReactionObserver
    {
        void OnReact(string reactionName, Owner owner, EventParameters parameters);
    }

    [AddComponentMenu("NiEngine/Object/Reactions")]
    public class Reactions : NiBehaviour, IReactionReceiver
    {
        public bool ReactEnabled => enabled;
        [Serializable]
        public class ReactionDefinition
        {
            [EditorField(inline: true), NotSaved(isDebug:true)]
            public string Name;
            public ConditionSet Conditions;
            public ActionSet Actions;

            [NonSerialized, NotSaved]
            List<IReactionObserver> Observers = null;

            public void Handshake(Reactions component, Owner owner)
            {
                foreach (var c in Conditions.Conditions)
                    Handshake(component, c);
                foreach (var a in Actions.Actions)
                    Handshake(component, a);

            }
            void Handshake(Reactions component, object obj)
            {
                if (obj is IInitialize initialize)
                    component.Initializers.Add(initialize);
            }
            public void AddObserver(IReactionObserver observer)
            {
                if (Observers == null) 
                    Observers = new();
                Observers.Add(observer);
            }
            public void RemoveObserver(IReactionObserver observer)
            {
                if (Observers == null) return;
                Observers.Remove(observer);
            }

            public int React(Owner owner, EventProcessor processor, EventParameters parameters)
                => processor.React(owner, Conditions, Actions, parameters);
        }

        [EditorField(header:false), Save(saveInPlace:true)]
        public List<ReactionDefinition> ReactionDefinitions = new();
        public EventProcessor Processor;

        [NonSerialized, NotSaved]
        List<IInitialize> Initializers = new();

        private void Awake()
        {
            var owner = new Owner(this);
            foreach (var def in ReactionDefinitions)
                def.Handshake(this, owner);

            foreach (var i in Initializers)
                i.Initialize(owner);
        }

        public void AddObserver(string reactionName, IReactionObserver observer)
        {
            foreach (var reaction in ReactionDefinitions)
                if (reaction.Name == reactionName)
                    reaction.AddObserver(observer);
        }
        public void RemoveObserver(string reactionName, IReactionObserver observer)
        {
            foreach (var reaction in ReactionDefinitions)
                if (reaction.Name == reactionName)
                    reaction.RemoveObserver(observer);
        }
        
        public bool HasReaction(string name, bool onlyEnabled, bool onlyActive, int maxLoop)
        {
            if (onlyActive) return false;
            if (onlyEnabled && !ReactEnabled) return false;
            foreach (var reaction in ReactionDefinitions)
                if (reaction.Name == name)
                    return true;
            return false;
        }
        public int React(string name, EventParameters parameters)
        {
            int count = 0;
            foreach (var reaction in ReactionDefinitions)
                if (reaction.Name == name)
                    count += reaction.React(new(this), Processor, parameters);
            return count;
        }
    }
}