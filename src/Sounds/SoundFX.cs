using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine
{
    /// <summary>
    /// Adds more configurable options to an AudioSource such as variable pitch and alternative audio clip.
    /// </summary>
    [AddComponentMenu("Nie/Sound/SoundFX")]
    [RequireComponent(typeof(AudioSource))]
    public class SoundFX : MonoBehaviour
    {

        [Tooltip("Vary the pitch of the sound up and down randomly for each play within this range when played")]
        public float PitchVariation = 0;
        float m_OriginalPitch;
        bool m_UseDestroyImmediate = false;

        [Tooltip("Play when object Awake callback is called. Do not use PlayOnAwake from the Audio source as it will not play with effects from SoundFX script.")]
        public bool PlayOnAwake;

        [Tooltip("Will destroy the object after the first play of this sound has finished.")]
        public bool OneShotDestroy;

        [Tooltip("Randomly select an alternative clip at every play.")]
        public List<AudioClip> AlternativeClips;

#if UNITY_EDITOR
        //[EditorCools.Button("Play")]
        public void PlayInEditor()
        {
            var source = GetComponent<AudioSource>();
            GameObject gameObject = new GameObject("One shot audio");
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            gameObject.transform.position = Camera.main.transform.position;
            AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
            audioSource.clip = source.clip;
            audioSource.spatialBlend = 1f;
            audioSource.volume = 1;
            SoundFX soundFX = (SoundFX)gameObject.AddComponent(typeof(SoundFX));
            soundFX.PitchVariation = PitchVariation;
            soundFX.m_OriginalPitch = audioSource.pitch;
            soundFX.PlayOnAwake = false;
            soundFX.OneShotDestroy = true;
            soundFX.m_UseDestroyImmediate = true;
            soundFX.Play();
        }
#endif

        AudioSource _AudioSource;
        AudioSource m_AudioSource => _AudioSource ??= GetComponent<AudioSource>();
        public float Volume { get => m_AudioSource.volume; set => m_AudioSource.volume = value; }
        public AudioClip Clip { get => m_AudioSource.clip; set => m_AudioSource.clip = value; }
        public float Lifetime => m_AudioSource.clip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale) + 1; // add a buffer of 1s to the lifetime for any effects to end.

        void Start()
        {
            m_OriginalPitch = m_AudioSource.pitch;
            if (PlayOnAwake)
                Play();
        }

        public void Play()
        {
            PlaySound();
            if (OneShotDestroy)
                StartCoroutine(Destroyer());  //Destroy(gameObject, Lifetime);

        }

        private void PlaySound()
        {
            m_AudioSource.pitch = m_OriginalPitch + m_OriginalPitch * (Random.value - 0.5f) * PitchVariation;
            if (AlternativeClips != null)
            {
                var clipIndex = Random.Range(0, AlternativeClips.Count + 1) - 1;
                if (clipIndex >= 0)
                    m_AudioSource.clip = AlternativeClips[clipIndex];
            }
            m_AudioSource.Play();

        }

        IEnumerator Destroyer()
        {
            yield return new WaitForSeconds(Lifetime);
            if (m_UseDestroyImmediate)
                DestroyImmediate(gameObject);
            else
                Destroy(gameObject);
        }
    }
}