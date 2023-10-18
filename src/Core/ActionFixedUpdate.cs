using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace NiEngine
{
    [Save]
    public class ActionFixedUpdate : MonoBehaviour
    {
        [NonSerialized, Save]
        public FixedUpdateSet FixedUpdates = new();

        public bool DeleteOnEmpty = false;
#if UNITY_EDITOR
        public int UpdateCount;
#endif
        void FixedUpdate()
        {
            if(!FixedUpdates.Update())
            {
                if(DeleteOnEmpty)
                    Component.Destroy(this);
            }
#if UNITY_EDITOR
            UpdateCount = FixedUpdates.Updates.Count;
#endif
        }
    }
}