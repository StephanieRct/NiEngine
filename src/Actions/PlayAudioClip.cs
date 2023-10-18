using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Interactions;
using Random = UnityEngine.Random;

namespace NiEngine.Actions
{

    [Serializable, ClassPickerName("Sound/PlayAudioClip")]
    public class PlayAudioClip : Action, IInitialize
    {
        [NotSaved]
        public AudioSource Source;
        [NotSaved]
        public AudioClip Clip;
        [NotSaved]
        public float PitchVariation;
        [NotSaved]
        public float Volume=1;
        [NotSaved]
        private float m_OriginalPitch;
        public override void Act(Owner owner, EventParameters parameters)
        {
            if(PitchVariation != 0 && m_OriginalPitch != 0)
                Source.pitch = m_OriginalPitch + m_OriginalPitch * (Random.value - 0.5f) * PitchVariation;

            Source.clip = Clip;
            Source.volume = Volume;
            Source.Play();
        }

        public void Initialize(Owner owner)
        {
            m_OriginalPitch = Source.pitch;
        }
    }

}