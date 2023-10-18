using System;
using UnityEngine;

namespace NiEngine
{
    /// <summary>
    /// Represent a single Layer. Useful for as inspector properties.
    /// </summary>
    [Serializable]
    public class GameObjectLayer
    {
        [SerializeField]
        private int m_LayerIndex = 0;

        public int LayerIndex
        {
            get { return m_LayerIndex; }
        }

        public void Set(int _layerIndex)
        {
            if (_layerIndex > 0 && _layerIndex < 32)
            {
                m_LayerIndex = _layerIndex;
            }
        }

        public int Mask
        {
            get { return 1 << m_LayerIndex; }
        }
    }
}