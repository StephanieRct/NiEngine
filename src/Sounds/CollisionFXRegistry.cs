using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace NiEngine
{
    /// <summary>
    /// Pair of material names and the sound to play when a collision between 2 objects with matching material names happens.
    /// </summary>
    [Serializable]
    public class CollisionFXPair
    {
        public string MaterialA;
        public string MaterialB;
        public SoundFX Sound;
    }

    /// <summary>
    /// Registry of CollisionFXPair used by CollisionFXMaterial.
    /// When 2 CollisionFXMaterial collide, the CollisionFXPair matching both object's CollisionFXMaterial material name is activated.
    /// </summary>
    [AddComponentMenu("Nie/Sound/CollisionFXRegistry")]
    public class CollisionFXRegistry : MonoBehaviour
    {
        [Tooltip("Volume is 0 if the collision relative velocity magnitude is less or equal to this value")]
        public float LowVolumeVelocity = 1f;

        [Tooltip("Volume is 1 if the collision relative velocity magnitude is greater or equal to this value")]
        public float HighVolumeVelocity = 10f;

        [Tooltip("Do not react if velocity if lower than this")]
        public float MinimumVelocity = 0f;

        [Tooltip("Do not react if velocity if higher than this")]
        public float MaximumVelocity = 99999f;

        [Tooltip("Curve between Minimum Velocity and Maximum Velocity. Can be null.")]
        public AnimationCurve VelocityToVolumeCurve;

        [Tooltip("Cooldown time in seconds to trigger the same collision again.")]
        public float Cooldown = 0.1f;

        [Tooltip("Pairs of collision material")]
        public List<CollisionFXPair> CollisionFXPairs = new();

        public bool TryGetPair(string material0, string material1, out CollisionFXPair pair)
        {
            pair = CollisionFXPairs.Where(x => material0 == x.MaterialA && material1 == x.MaterialB).FirstOrDefault();
            return pair != null;
        }

        public float ComputeVolume(float relativeVelocity)
        {
            var ratio = math.saturate((relativeVelocity - LowVolumeVelocity) / (HighVolumeVelocity - LowVolumeVelocity));
            if (VelocityToVolumeCurve != null)
                return VelocityToVolumeCurve.Evaluate(ratio);
            return ratio;
        }

        public bool CanReactVelocity(float relativeVelocity) 
            => relativeVelocity >= LowVolumeVelocity && relativeVelocity >= MinimumVelocity && relativeVelocity <= MaximumVelocity;
    }
}