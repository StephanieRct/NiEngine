using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace NiEngine
{
    [Save]
    public class ActionLateUpdate : MonoBehaviour
    {
        [NonSerialized, Save]
        public LateUpdateSet LateUpdates = new();
        public bool DeleteOnEmpty = false;
#if UNITY_EDITOR
        public int UpdateCount;
#endif
        void LateUpdate()
        {
            if (!LateUpdates.Update())
            {
                if (DeleteOnEmpty)
                    Component.Destroy(this);
            }
#if UNITY_EDITOR
            UpdateCount = LateUpdates.Updates.Count;
#endif
        }
    }
}