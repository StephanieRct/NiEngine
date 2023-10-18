using System;
using UnityEngine;

namespace NiEngine
{
    [Save]
    public class ActionUpdate : MonoBehaviour
    {
        [NonSerialized, Save]
        public UpdateSet Updates = new ();
        public bool DeleteOnEmpty = false;
#if UNITY_EDITOR
        public int UpdateCount;
#endif
        void Update()
        {
            if (!Updates.Update())
            {
                if (DeleteOnEmpty)
                    Component.Destroy(this);
            }
#if UNITY_EDITOR
            UpdateCount = Updates.Updates.Count;
#endif
        }
    }
}