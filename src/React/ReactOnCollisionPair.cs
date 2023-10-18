
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;
using System.Linq;
using NiEngine.Expressions.GameObjects;

namespace NiEngine
{

    /// <summary>
    /// 
    /// </summary>
    [AddComponentMenu("Nie/Object/ReactOnCollisionPair")]
    public class ReactOnCollisionPair : NiBehaviour
    {
        #region Data
        [Header("Reaction Condition:")]

        [Tooltip("Name for this object.")]
        [NotSaved]
        public string ThisReactionName;

        [Tooltip("Name for the other object. \n\rFor the reaction to happen between 2 ReactOnCollisionPair: \n\r* this.ThisReactionName must be equal to other.OtherReactionName \n\r AND \n\r\t* this.OtherReactionName must be equal to other.ThisReactionName.")]
        [NotSaved]
        public string OtherReactionName;

        [Tooltip("Minimum magnitude of the relative velocity required when a collision happens in order to trigger the reaction. Use for collision only (not with triggers)")]
        [NotSaved]
        public float MinimumRelativeVelocity = 0;

        [NotSaved]
        public bool ReactWithOtherTriggerObject = true;
        [NotSaved]
        public bool ReactWithOtherColliderObject = true;
        [NotSaved]
        public bool ReactWithOtherRigidbodyObject = true;
        [NotSaved]
        public bool ReactOnCollision = true;
        [NotSaved]
        public bool ReactOnTrigger = true;

        [Tooltip("With an attached Rigidbody, Will react to all collisions on the rigidbody attached to this gameobject. A NiRigidbody component will be added for that purpose to the Rigidbody gameobject if none exists.")]
        [NotSaved]
        public bool ReactOnRigidbodyCollision = false;

        //#region old
        //[Tooltip("Will react to collision enter/exit on this gameobject with other matching ReactOnCollisionPair")]
        //public bool ReactToCollision = true;

        //[Tooltip("With an attached Rigidbody, will react to collision with any collider components on this gameobject")]
        //public bool ReactToColliderCollision = false;


        //[Tooltip("Will react to triggers enter/exit on this gameobject when another matching ReactOnCollisionPair that has its ReactToOtherTrigger set to true")]
        //public bool ReactToTrigger = true;
        //#endregion

        //[Tooltip("Will react to triggers enter/exit on the other matching ReactOnCollisionPair that has its ReactToTrigger set to true.")]
        //public bool ReactToOtherTrigger = true;

        [Tooltip("Only react with objects of these layers")]
        [NotSaved]
        public LayerMask ObjectLayerMask = -1;

        [Tooltip("Time in second to delay the reaction.")]
        [NotSaved]
        public float ReactionDelay = 0;

        [Tooltip("If check and reaction is delayed, the 2 objects must be touching for the full duration of the delay.")]
        [NotSaved]
        public bool MustTouchDuringDelay;

        [Tooltip("Once this ReactOnCollisionPair reacts with another ReactOnCollisionPair, the same reaction cannot be triggered again within the cooldown period, in seconds.")]
        [NotSaved]
        public float ReactionCooldown = 0;

        [Tooltip("If reaction is delayed, do not trigger new reactions during the delay.")]
        [NotSaved]
        public bool SingleAtOnce = false;

        [NotSaved]
        public bool LogCollisions;
        [NotSaved]
        public bool LogMatches;
        [NotSaved]
        public bool LogMatchFailures;
        [NotSaved]
        public bool LogConditionsFailures;

        [Tooltip("The trigger object to use for the matching ReactOnCollisionPair. If not set, the trigger object is 'Self'")]
        [SerializeReference, ObjectReferencePicker, EditorField(showPrefixLabel: true, inline: false)]
        public IExpressionGameObject OverrideTriggerObject;


        public ConditionSet Conditions;

        public StateActionSet OnBegin;

        public ActionSet OnEnd;

        //public EventStateProcessor Processor;





        public GameObject TargetObject => gameObject;// ThisObject != null ? TargetObject : gameObject;

        /// <summary>
        /// set only if SingleAtOnce is true
        /// </summary>
        ReactOnCollisionPair m_CurrentSingleReaction;

        // Keep track of what ReactOnCollisionPair are currently touching
        List<ReactionTrigger> m_TouchingWith = new();

        // Keep track of what Reaction is currently on cooldown
        List<ReactionTrigger> m_CooldownWith = new();

        [System.Serializable]
        class ReactionTrigger
        {
            public ReactOnCollisionPair Self;
            public ReactOnCollisionPair Other;
            public Vector3 Position;
            public float TimerCountdown;
            //public bool HasBegun;
            public bool MustEnd;
            //public EventParameters EventParameters;
            public EventStateProcessor Processor;
            public ReactionTrigger(ReactOnCollisionPair self, ReactOnCollisionPair other, float delay, Vector3 position)//, EventParameters parameters)
            {
                Self = self;
                Other = other;
                //Position = position;
                TimerCountdown = delay;
                Position = position;
                //EventParameters = parameters;
            }
            public bool Tick()
            {
                if (TimerCountdown >= 0)
                {
                    TimerCountdown -= Time.deltaTime;
                    return TimerCountdown < 0;
                }
                return false;
            }
            public void Begin()
            {

                var parameters = Processor.MakeBeginEvent(Self.gameObject, EventParameters.ParameterSet.Trigger(Self.gameObject, Other.gameObject, Position));
                if (Self.LogMatches)
                    parameters.Log(new(Self), "ROCP.LogMatches", $"[ROCP][{Self.GetNameOrNull()}] match begin with other:[{Other.gameObject.GetNameOrNull()}]");

                Processor.Begin(new(Self), Self.OnBegin, parameters);

                if (Self.ReactionCooldown > 0)
                {
                    TimerCountdown = Self.ReactionCooldown;
                    Self.m_CooldownWith.Add(this);
                }
                //HasBegun = true;

                if (MustEnd)
                {
                    End();
                }
            }
            public void End()
            {

                var parameters = Processor.MakeEndEvent(Self.gameObject, EventParameters.ParameterSet.Trigger(Self.gameObject, Other.gameObject, Position));
                if (Self.LogMatches)
                    parameters.Log(new(Self), "ROCP.LogMatches", $"[ROCP][{Self.GetNameOrNull()}] match end with other:[{Other.gameObject.GetNameOrNull()}]");

                Processor.End(new(Self), Self.OnBegin, Self.OnEnd, parameters);
            }
        }
        // Keep track of all Reaction currently on a delay
        List<ReactionTrigger> m_DelayReactions = new();
        #endregion

        [HideInInspector]
        [NonSerialized]
        public Rigidbody Rigidbody;
        public GameObject BodyOrSelf => Rigidbody != null ? Rigidbody.gameObject : gameObject;
        private void Awake()
        {

            Rigidbody = GetComponentInParent<Rigidbody>();
        }

        public EventParameters MakeEventParameters(ReactOnCollisionPair other, Vector3 position)
        {
            var ep = EventParameters.Trigger(gameObject, other.gameObject, position);
            if (other.OverrideTriggerObject != null)
            {
                var newTrigger = other.OverrideTriggerObject.GetValue(new (this), ep);
                ep.Current.TriggerObject = newTrigger;
            }

            return ep;
        }

        public bool CanReact(ReactOnCollisionPair other, EventParameters parameters)
        {
            if (!enabled)
            {
                if(LogMatchFailures)
                    parameters.Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{other.gameObject.GetNameOrNull()}] MatchFailures: this object is disabled.");
                return false;
            }

            if ((ObjectLayerMask.value & (1 << (other.BodyOrSelf).layer)) == 0)
            {
                if (LogMatchFailures)
                    parameters.Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{other.gameObject.GetNameOrNull()}] MatchFailures: mask failed: require:{ObjectLayerMask.value}, object layer is:{other.gameObject.layer}");
                return false;
            }

            if (SingleAtOnce && m_CurrentSingleReaction != null)
            {
                if (LogMatchFailures)
                    parameters.Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{other.gameObject.GetNameOrNull()}] MatchFailures: SingleAtOnce and currently has a reaction waiting to happen with [{m_CurrentSingleReaction.gameObject.GetNameOrNull()}]");
                return false;
            }

            if (!new EventStateProcessor().Pass(new (this), Conditions, parameters, LogConditionsFailures))
            {
                if (LogMatchFailures)
                    parameters.Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{other.gameObject.GetNameOrNull()}] MatchFailures: Condition failed.");
                return false;
            }

            return true;
        }

        public bool RequestReaction(ReactOnCollisionPair other, EventParameters parameters)
        {
            if (!CanReact(other, parameters))
            {
                return false;
            }

            if (!other.CanReact(this, MakeEventParameters(this, parameters.Current.TriggerPosition)))
            {
                if (LogMatchFailures)
                    parameters.Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{other.gameObject.GetNameOrNull()}] MatchFailures: other object cannot react.");
                return false;
            }
            
            if (other.ThisReactionName != OtherReactionName || other.OtherReactionName != ThisReactionName)
            {
                if (LogMatchFailures)
                    parameters.Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{other.gameObject.GetNameOrNull()}] MatchFailures: names do not match (\"{ThisReactionName}\":\"{OtherReactionName}\") -> (\"{other.ThisReactionName}\":\"{other.OtherReactionName}\").");
                return false;
            }

            if (SingleAtOnce) m_CurrentSingleReaction = other;
            return true;
        }

        //public bool MatchPairEnter(ReactOnCollisionPair other, Vector3 position)
        //{
        //    var parameters = MakeEventParameters(other, position);
        //    if (RequestReaction(other, parameters))
        //    {
        //        BeginTouch(other, parameters);
        //        return true;
        //    }
        //    return false;
        //}
        //public bool MatchPairExit(ReactOnCollisionPair other, Vector3 position)
        //{
        //    return EndTouchIfTouching(other);
        //}

        public int MatchPairsEnter(GameObject otherGameObject, Vector3 position)
        {
            int count = 0;
            foreach (var other in otherGameObject.GetComponents<ReactOnCollisionPair>())
            {
                var parameters = MakeEventParameters(other, position);
                if (RequestReaction(other, parameters))
                {
                    BeginTouch(other, parameters);
                    ++count;
                }
            }

            return count;
        }
        public int MatchPairsExit(GameObject otherGameObject, Vector3 position)
        {
            int count = 0;
            foreach (var other in otherGameObject.GetComponents<ReactOnCollisionPair>())
            {
                if(EndTouchIfTouching(other))
                    ++count;
            }

            return count;
        }
        private void Update()
        {
            // Update all reaction on delay.
            m_DelayReactions.RemoveAll(reaction =>
            {
                // abort the reaction if the other object was deleted
                if (reaction.Other == null)
                    return true;

                if (reaction.Tick())
                {
                    reaction.Begin();
                    return true;
                }
                return false;
            });


            // Update all reactions on cooldown
            m_CooldownWith.RemoveAll(reaction =>
            {
                if (reaction.Other == null)
                    return true;

                if (reaction.Tick())
                {
                    return true;
                }
                return false;
            });

            if (m_CurrentSingleReaction != null && m_DelayReactions.Count == 0 && m_CooldownWith.Count == 0)
                m_CurrentSingleReaction = null;
        }
        
        void OnDestroy()
        {
            foreach (var other in m_TouchingWith)
            {
                other.End();
                other.Other.EndTouchIfTouching(this);
            }
        }

#region Touching state
        void BeginTouch(ReactOnCollisionPair other, EventParameters parameters)
        {
            var reaction = new ReactionTrigger(this, other, ReactionDelay, parameters.Current.TriggerPosition);
            m_TouchingWith.Add(reaction);

            // React if not currently on cooldown
            if (m_CooldownWith.All(x => x.Other != other))
            {
                if (ReactionDelay == 0)
                    reaction.Begin();
                else
                    m_DelayReactions.Add(reaction);
            }
        }
        

        bool EndTouchIfTouching(ReactOnCollisionPair other)
        {
            // if reactions require the objects to always touch during the delay, remove all current reaction with the other object.
            if (MustTouchDuringDelay)
                m_DelayReactions.RemoveAll(reaction => reaction.Other == other);
            else
            {
                // reaction on a delay that do not have MustTouchDuringDelay
                // will end right after they begin
                foreach(var r in m_DelayReactions)
                {
                    if(r.Other == other)
                        r.MustEnd = true;
                }
            }
            var reaction = m_TouchingWith.FirstOrDefault(reaction => reaction.Other == other);
            if (reaction != null && m_TouchingWith.Remove(reaction))
            {
                reaction.End();
                return true;
            }
            return false;
        }
#endregion

#region Collision Callbacks
        
        public EventParameters MakeEventParameters(Collision collision)
            => EventParameters.Trigger(gameObject, collision.gameObject, collision.contactCount > 0 ? collision.GetContact(0).point : transform.position);

        public EventParameters MakeEventParameters(Collider otherCollider)
            => EventParameters.Trigger(gameObject, otherCollider.gameObject, transform.position);

        string FormatCollision(Collision collision)
        {
            return $"[{this.GetNameOrNull()}]->[Col:{collision.collider.gameObject.GetNameOrNull()}, Rb:{collision.rigidbody?.gameObject.GetNameOrNull()}]";
        }
        string FormatCollision(Collider otherCollider)
        {
            return $"[{this.GetNameOrNull()}]->[Col:{otherCollider.gameObject.GetNameOrNull()}, Rb:{otherCollider.attachedRigidbody?.gameObject.GetNameOrNull()}]";
        }
        
        public void HandleCallback(bool enter, Collider collider, Rigidbody rigidbody, Vector3 position, Action<string> failure)
        {

            bool reactedWithCollider = false;
            if (collider.isTrigger)
            {
                if (ReactWithOtherTriggerObject)
                {
                    if(enter)
                        MatchPairsEnter(collider.gameObject, position);
                    else
                        MatchPairsExit(collider.gameObject, position);
                    reactedWithCollider = true;
                }
                else if (LogMatchFailures)
                    failure("MatchFailures with Trigger: other collider is a trigger and ReactWithOtherTriggerObject is false");
            }
            else
            {
                if (ReactWithOtherColliderObject)
                {
                    if (enter)
                        MatchPairsEnter(collider.gameObject, position);
                    else
                        MatchPairsExit(collider.gameObject, position);
                    reactedWithCollider = true;
                }
                else if (LogMatchFailures)
                    failure("MatchFailures with Collider: other collider is not a trigger and ReactWithOtherColliderObject is false");
            }

            // React with rigidbody only if:
            //  Other object has an attached rigidbody
            //  either of:
            //      reactedWithCollider is false
            //      or the rigidbody is not the same object as the collider
            //  We have ReactWithOtherRigidbodyObject true
            if (rigidbody != null
                && (!reactedWithCollider || rigidbody.gameObject != collider.gameObject))
            {
                if (ReactWithOtherRigidbodyObject)
                {
                    if (enter)
                        MatchPairsEnter(rigidbody.gameObject, position);
                    else
                        MatchPairsExit(rigidbody.gameObject, position);
                }
                else if (LogMatchFailures)
                    failure("MatchFailures with Rigidbody: other has a Rigidbody but ReactWithOtherRigidbodyObject is false");
            }

        }

        public void OnCollisionEnter(Collision collision)
        {
            // OnCollisionEnter is called event when gameobject is disabled.
            if (!enabled) return;
            
            if (!ReactOnCollision)
            {
                if (LogMatchFailures)
                    MakeEventParameters(collision).Log(new(this), "ROCP.OnCollisionEnter.LogMatchFailures", $"[ROCP]{FormatCollision(collision)} OnCollisionEnter MatchFailures: ReactOnCollision is false.");
                return;
            }

            if (LogCollisions)
                MakeEventParameters(collision).Log(new(this), "ROCP.OnCollisionEnter.LogCollisions", $"[ROCP]{FormatCollision(collision)} OnCollisionEnter");

            if (collision.relativeVelocity.sqrMagnitude < MinimumRelativeVelocity * MinimumRelativeVelocity)
            {
                if (LogMatchFailures)
                    MakeEventParameters(collision).Log(new(this), "ROCP.OnCollisionEnter.LogMatchFailures", $"[ROCP][{FormatCollision(collision)} OnCollisionEnter MatchFailures: relative velocity too low. {collision.relativeVelocity.magnitude} < {MinimumRelativeVelocity}.");
                return;
            }

            var position = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            HandleCallback(enter: true, collision.collider, collision.rigidbody, position, x =>
            {
                MakeEventParameters(collision).Log(new(this), "ROCP.OnCollisionEnter.LogMatchFailures", $"[ROCP]{FormatCollision(collision)} OnCollisionEnter {x}");
            });


            //bool reactedWithCollider = false;
            //if (collision.collider.isTrigger)
            //{
            //    if (ReactWithOtherTriggerObject)
            //    {
            //        MatchPairsEnter(collision.collider.gameObject, position);
            //        reactedWithCollider = true;
            //    }
            //    else if (LogMatchFailures)
            //        MakeEventParameters(collision).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP]{FormatCollision(collision)} MatchFailures with Trigger: other collider is a trigger and ReactWithOtherTriggerObject is false");
            //}
            //else
            //{
            //    if (ReactWithOtherColliderObject)
            //    {
            //        MatchPairsEnter(collision.collider.gameObject, position);
            //        reactedWithCollider = true;
            //    }
            //    else if (LogMatchFailures)
            //        MakeEventParameters(collision).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP]{FormatCollision(collision)} MatchFailures with Collider: other collider is not a trigger and ReactWithOtherColliderObject is false");
            //}

            //// React with rigidbody only if:
            ////  Other object has an attached rigidbody
            ////  either of:
            ////      reactedWithCollider is false
            ////      or the rigidbody is not the same object as the collider
            ////  We have ReactWithOtherRigidbodyObject true
            //if (collision.rigidbody != null 
            //    && (!reactedWithCollider || collision.rigidbody.gameObject != collision.collider.gameObject))
            //{
            //    if (ReactWithOtherRigidbodyObject)
            //    {
            //        MatchPairsEnter(collision.collider.gameObject, position);
            //    }
            //    else if (LogMatchFailures)
            //        MakeEventParameters(collision).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP]{FormatCollision(collision)} MatchFailures with Rigidbody: other has a Rigidbody but ReactWithOtherRigidbodyObject is false");
            //}











            //    if (!ReactToCollision)
            //{
            //    if (LogMatchFailures)
            //        MakeEventParameters(collision).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{collision.gameObject.GetNameOrNull()}] MatchFailures: OnCollisionEnter while ReactToCollision is false.");
            //    return;
            //}

            //if (collision.relativeVelocity.sqrMagnitude < MinimumRelativeVelocity * MinimumRelativeVelocity)
            //{
            //    if (LogMatchFailures)
            //        MakeEventParameters(collision).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{collision.gameObject.GetNameOrNull()}] MatchFailures: OnCollisionEnter relative velocity too low. {collision.relativeVelocity.magnitude} < {MinimumRelativeVelocity}.");
            //    return;
            //}

            //if(LogCollisions)
            //    MakeEventParameters(collision).Log(new(this), "ROCP.LogCollisions", $"[ROCP][{this.GetNameOrNull()}].OnCollisionEnter with Go:[{collision.gameObject.GetNameOrNull()}] Rb:[{collision.rigidbody?.gameObject.GetNameOrNull()}] Col:[{collision.collider.gameObject.GetNameOrNull()}]");

            //var position = collision.GetContact(0).point;

            //foreach (var other in collision.collider.gameObject.GetComponents<ReactOnCollisionPair>())
            //{
            //    var otherParameters = other.MakeEventParameters(this, position);
            //    if (other.RequestReaction(this, otherParameters))
            //    {
            //        var parameters = MakeEventParameters(other, position);
            //        BeginTouch(other, parameters);

            //        // if other.ReactToColliderCollision and there's a rigidbody and it's not the same gameobject as the collider gameobject
            //        //      test the collider gameobject for any ReactOnCollisionPair.
            //        //      this needs to be done since the collider gameobject will not receive OnCollisionEnter if it has a rigidbody attached
            //        if (other.ReactToColliderCollision && collision.rigidbody != null && collision.rigidbody.gameObject != collision.collider.gameObject)
            //            other.BeginTouch(this, otherParameters);

            //    }
            //}
        }

        public void OnCollisionExit(Collision collision)
        {
            // OnCollisionExit is called event when gameobject is disabled.
            if (!enabled) return;

            if (!ReactOnCollision)
            {
                if (LogMatchFailures)
                    MakeEventParameters(collision).Log(new(this), "ROCP.OnCollisionExit.LogMatchFailures", $"[ROCP]{FormatCollision(collision)} OnCollisionExit MatchFailures: ReactOnCollision is false.");
                return;
            }

            if (LogCollisions)
                MakeEventParameters(collision).Log(new(this), "ROCP.OnCollisionExit.LogCollisions", $"[ROCP]{FormatCollision(collision)} OnCollisionExit");

            if (collision.relativeVelocity.sqrMagnitude < MinimumRelativeVelocity * MinimumRelativeVelocity)
            {
                if (LogMatchFailures)
                    MakeEventParameters(collision).Log(new(this), "ROCP.OnCollisionExit.LogMatchFailures", $"[ROCP][{FormatCollision(collision)} OnCollisionExit MatchFailures: relative velocity too low. {collision.relativeVelocity.magnitude} < {MinimumRelativeVelocity}.");
                return;
            }

            var position = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            HandleCallback(enter: false, collision.collider, collision.rigidbody, position, x =>
            {
                MakeEventParameters(collision).Log(new(this), "ROCP.OnCollisionExit.LogMatchFailures", $"[ROCP]{FormatCollision(collision)} OnCollisionExit {x}");
            });
        }

        public void OnTriggerEnter(Collider otherCollider)
        {
            // OnTriggerEnter is called event when gameobject is disabled.
            if (!enabled) return;
            if (!ReactOnTrigger)
            {
                if (LogMatchFailures)
                    MakeEventParameters(otherCollider).Log(new(this), "ROCP.OnTriggerEnter.LogMatchFailures", $"[ROCP]{FormatCollision(otherCollider)} OnTriggerEnter MatchFailures: ReactToTrigger is false.");
                return;
            }

            if (LogCollisions)
                MakeEventParameters(otherCollider).Log(new(this), "ROCP.OnTriggerEnter.LogCollisions", $"[ROCP]{FormatCollision(otherCollider)} OnTriggerEnter");
            
            var position = transform.position;
            HandleCallback(enter: true, otherCollider, otherCollider.attachedRigidbody, position, x =>
            {
                MakeEventParameters(otherCollider).Log(new(this), "ROCP.OnTriggerEnter.LogMatchFailures", $"[ROCP]{FormatCollision(otherCollider)} OnTriggerEnter {x}");
            });
        }

        public void OnTriggerExit(Collider otherCollider)
        {
            // OnTriggerEnter is called event when gameobject is disabled.
            if (!enabled) return;
            if (!ReactOnTrigger)
            {
                if (LogMatchFailures)
                    MakeEventParameters(otherCollider).Log(new(this), "ROCP.OnTriggerExit.LogMatchFailures", $"[ROCP]{FormatCollision(otherCollider)} OnTriggerExit MatchFailures: while ReactToTrigger is false.");
                return;
            }

            if (LogCollisions)
                MakeEventParameters(otherCollider).Log(new(this), "ROCP.OnTriggerExit.LogCollisions", $"[ROCP]{FormatCollision(otherCollider)} OnTriggerExit");
            
            var position = transform.position;
            HandleCallback(enter: false, otherCollider, otherCollider.attachedRigidbody, position, x =>
            {
                MakeEventParameters(otherCollider).Log(new(this), "ROCP.OnTriggerExit.LogMatchFailures", $"[ROCP]{FormatCollision(otherCollider)} OnTriggerExit {x}");
            });
            
        }


        //public void OnCollisionExit(Collision collision)
        //{
        //    if (!enabled) return;
        //    if (!ReactToCollision)
        //    {
        //        if (LogMatchFailures)
        //            MakeEventParameters(collision).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{collision.gameObject.GetNameOrNull()}] MatchFailures: OnCollisionExit while ReactToCollision is false.");
        //        return;
        //    }

        //    if (collision.relativeVelocity.sqrMagnitude < MinimumRelativeVelocity * MinimumRelativeVelocity)
        //    {
        //        if (LogMatchFailures)
        //            MakeEventParameters(collision).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{collision.gameObject.GetNameOrNull()}] MatchFailures: OnCollisionExit relative velocity too low. {collision.relativeVelocity.magnitude} < {MinimumRelativeVelocity}.");
        //        return;
        //    }

        //    if (LogCollisions)
        //        MakeEventParameters(collision).Log(new(this), "ROCP.LogCollisions", $"[ROCP][{this.GetNameOrNull()}].OnCollisionExit with Go:[{collision.gameObject.GetNameOrNull()}] Rb:[{collision.rigidbody?.gameObject.GetNameOrNull()}] Col:[{collision.collider.gameObject.GetNameOrNull()}]");


        //    foreach (var other in collision.collider.gameObject.GetComponents<ReactOnCollisionPair>())
        //    {
        //        EndTouchIfTouching(other);
        //        // if other.ReactToColliderCollision and there's a rigidbody and it's not the same gameobject as the collider gameobject
        //        //      test the collider gameobject for any ReactOnCollisionPair.
        //        //      this needs to be done since the collider gameobject will not receive OnCollisionEnter if it has a rigidbody attached
        //        if (other.ReactToColliderCollision && collision.rigidbody != null && collision.rigidbody.gameObject != collision.collider.gameObject)
        //            other.EndTouchIfTouching(this);
        //    }
        //}


        //private void OnTriggerEnter(Collider otherCollider)
        //{
        //    if (!enabled) return;
        //    if (!ReactToTrigger)
        //    {
        //        if (LogMatchFailures)
        //            MakeEventParameters(otherCollider).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{otherCollider.gameObject.GetNameOrNull()}] MatchFailures: OnTriggerEnter while ReactToTrigger is false.");
        //        return;
        //    }

        //    if (LogCollisions)
        //        MakeEventParameters(otherCollider).Log(new(this), "ROCP.LogCollisions", $"[ROCP][{this.GetNameOrNull()}].OnTriggerEnter with Col:[{otherCollider.gameObject.GetNameOrNull()}] ARb:[{otherCollider.attachedRigidbody?.gameObject.GetNameOrNull()}]");

        //    foreach (var other in otherCollider.gameObject.GetComponents<ReactOnCollisionPair>())
        //        if (other.RequestReaction(this, other.MakeEventParameters(this, transform.position)))
        //            BeginTouch(other, MakeEventParameters(other, transform.position));
        //}

        //private void OnTriggerExit(Collider otherCollider)
        //{
        //    if (!enabled) return;
        //    if (!ReactToTrigger)
        //    {
        //        if (LogMatchFailures)
        //            MakeEventParameters(otherCollider).Log(new(this), "ROCP.LogMatchFailures", $"[ROCP][{this.GetNameOrNull()}]->[{otherCollider.gameObject.GetNameOrNull()}] MatchFailures: OnTriggerExit while ReactToTrigger is false.");
        //        return;
        //    }

        //    if (LogCollisions)
        //        MakeEventParameters(otherCollider).Log(new(this), "ROCP.LogCollisions", $"[ROCP][{this.GetNameOrNull()}].OnTriggerExit with Col:[{otherCollider.gameObject.GetNameOrNull()}] ARb:[{otherCollider.attachedRigidbody?.gameObject.GetNameOrNull()}]");

        //    foreach (var other in otherCollider.gameObject.GetComponents<ReactOnCollisionPair>())
        //        EndTouchIfTouching(other);
        //}
        #endregion
    }

}
