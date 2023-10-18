using UnityEngine;

namespace NiEngine
{
    // TODO add GameObjectReference
    [System.Serializable]
    public struct AnimatorStateReference
    {
        public Animator Animator;
        public string State;
        public int StateHash;
    }

}