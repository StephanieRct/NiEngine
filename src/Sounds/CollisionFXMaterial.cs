using System.Collections;
using UnityEngine;
using Unity.Mathematics;

namespace NiEngine
{
    /// <summary>
    /// Define the material name used when a collision between 2 CollisionFXMaterial happens
    /// </summary>
    [AddComponentMenu("Nie/Object/CollisionFXMaterial")]
    public class CollisionFXMaterial : NiBehaviour
    {
        [Tooltip("Registry to used when a collision happens")]
        public CollisionFXRegistry Registry;

        [Tooltip("Material name to use when matching this CollisionFXRegistry with another CollisionFXMaterial from the CollisionFXRegistry when a collision happens")]
        public string MaterialName;

        [Tooltip("Only react with objects of these layers")]
        public LayerMask ObjectLayerMask = -1;

        public ConditionSet Conditions;

        float m_CoolDownTimer = 0;

        void Update()
        {
            if (m_CoolDownTimer > 0)
                m_CoolDownTimer -= Time.deltaTime;
        }

        public bool CanReactVelocity(float relativeVelocity)
        {
            if (!enabled) return false;
            if (m_CoolDownTimer > 0) return false;
            if (!Registry.CanReactVelocity(relativeVelocity)) return false;
            return true;
        }
        public bool CanReactWith(CollisionFXMaterial other, Vector3 position, float relativeVelocity)
        {
            if (!other.enabled) return false;
            if ((ObjectLayerMask.value & (1 << other.gameObject.layer)) == 0) return false;
            if (!Conditions.Pass(new(this), EventParameters.Trigger(gameObject, gameObject, other.gameObject, position)))
                return false;
            return true;
        }
        void Collide(CollisionFXMaterial other, Vector3 position, float relativeVelocity)
        {
            if (CanReactWith(other, position, relativeVelocity))
                if (Registry.TryGetPair(MaterialName, other.MaterialName, out var pair))
                {
                    var obj = Instantiate(pair.Sound.gameObject, position, Quaternion.identity);
                    if (obj.TryGetComponent<SoundFX>(out var soundFX))
                    {
                        soundFX.OneShotDestroy = true;
                        soundFX.PlayOnAwake = true;
                        var volume = Registry.ComputeVolume(relativeVelocity);
                        soundFX.Volume = volume;
                    }
                    m_CoolDownTimer = Registry.Cooldown;
                }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!enabled) return;
            if (!CanReactVelocity(collision.relativeVelocity.magnitude)) return;
            foreach (var fx in collision.gameObject.GetComponents<CollisionFXMaterial>())
                Collide(fx, collision.GetContact(0).point, collision.relativeVelocity.magnitude);
        }
    }
}