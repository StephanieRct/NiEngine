using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using NiEngine.Expressions.GameObjects;
using NiEngine.Expressions;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("GameObject./Spawn")]
    public class Spawn : Action
    {

        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionGameObject ObjectToSpawn;


        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionNiTransform SpawnPosition;
        
        public override void Act(Owner owner, EventParameters parameters)
        {
            var obj = ObjectToSpawn.GetValue(owner, parameters);
            var trans = SpawnPosition.GetValue(owner, parameters);
            if (obj)
            {
                var spawned = GameObjectExt.InstantiateSavable(obj);
                Move.Object(spawned, Move.Method.TransformSet, trans);
            }
        }
    }

    [Serializable, ClassPickerName("GameObject./SpawnMax")]
    public class SpawnMax : Action, IUidObjectHost
    {

        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionGameObject ObjectToSpawn;
        
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionNiTransform SpawnPosition;

        [NotSaved]
        public Move.Method MoveMethod = Move.Method.TransformSet;

        [NotSaved]
        public int Max;

        //public ConditionSet ConditionToReuse;
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionBool ConditionToReuse;
        [Save(saveInPlace: true)]
        public ActionSet OnSpawn;
        [Save(saveInPlace: true)]
        public ActionSet OnCannotSpawnAnymore;

        [Serializable]
        struct InternalState
        {
            public List<GameObject> SpawnedObjects;
            public EventProcessor Processor;
            public int ReuseIndex;
        }
        [SerializeField, EditorField(runtimeOnly: true)]
        InternalState Internals;

        public IEnumerable<IUidObject> Uids => OnSpawn.Uids.Concat(OnCannotSpawnAnymore.Uids);

        void AfterSpawn(Owner owner, EventParameters parameters, GameObject obj)
        {
            var trans = SpawnPosition.GetValue(owner, parameters);
            Move.Object(obj, MoveMethod, trans);
            Internals.Processor.Act(owner, OnSpawn, parameters);
        }

        bool TryReuse(Owner owner, EventParameters parameters, GameObject obj)
        {
            var parameters2 = parameters.WithOverride(parameters.Current.TriggerObject, obj);
            if(ConditionToReuse.Pass(owner, parameters2))
            //if (Internals.Processor.Pass(owner, ConditionToReuse, parameters2))
            {
                AfterSpawn(owner, parameters2, obj);
                return true;
            }
            return false;
        }

        public override void Act(Owner owner, EventParameters parameters)
        {
            if (Max == 0) 
                return;

            if (Internals.SpawnedObjects == null)
                Internals.SpawnedObjects = new();
            
            var obj = ObjectToSpawn.GetValue(owner, parameters);
            if (obj)
            {
                if (Internals.SpawnedObjects.Count >= Max)
                {
                    // reuse
                    bool found = false;
                    foreach (var i in Range.Ring(0, Internals.SpawnedObjects.Count, Internals.ReuseIndex))
                    {
                        if (TryReuse(owner, parameters, Internals.SpawnedObjects[i]))
                        {
                            Internals.ReuseIndex = i + 1;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Internals.Processor.Act(owner, OnCannotSpawnAnymore, parameters);
                    }
                }
                else
                {
                    // spawn new
                    var spawned = GameObjectExt.InstantiateSavable(obj);
                    Internals.SpawnedObjects.Add(spawned);
                    AfterSpawn(owner, parameters.WithOverrideTrigger(spawned), spawned);
                }
            }
        }

    }
}