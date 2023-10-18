using NiEngine.IO.SaveOverrides;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NiEngine.IO
{
    public class RootStreamOutput : RootStream, IOutput
    {
        public new IOutput BaseStream;

        public RootStreamOutput(IOutput baseStream)
            : base(baseStream)
        {
            BaseStream = baseStream;
            TypeRegistry = new TypeRegistry();
            Init();
        }


        public void SaveMetaData(StreamContext context)
        {
            using (var scopeGo = context.ScopeKey("GameObjectMeta", this))
                GameObjectSO.SaveMetaData(context, this);

            using (var scopeNb = context.ScopeKey("NiBehaviourMeta", this))
                NiBehaviourSO.SaveMetaData(context, this);
        }
        public void SaveAllGameObjectsInScene(StreamContext context)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                var roots = scene.GetRootGameObjects();
                foreach (var r in roots)
                {
                    GameObjectSO.SaveGameObjectHierarchy(context, r, this);
                }
            }

        }

        #region IOutput interface

        public void Save<T>(StreamContext context, object key, T value)
        {
            Type type = value.GetType() ?? typeof(T);
            TypeRegistry.SaveData(context, baseStream: BaseStream, nestedStream: this, key, type, value);
        }
        public void Save(StreamContext context, object key, Type type, object value)
        {
            TypeRegistry.SaveData(context, baseStream: BaseStream, nestedStream: this, key, type, value);
        }
        public void SaveInPlace<T>(StreamContext context, object key, T value)
        {
            Type type = value?.GetType() ?? typeof(T);
            TypeRegistry.SaveInPlace(context, baseStream: BaseStream, nestedStream: this, key, type, value);
        }
        public void SaveInPlace(StreamContext context, object key, Type type, object value)
        {
            TypeRegistry.SaveInPlace(context, baseStream: BaseStream, nestedStream: this, key, type, value);
        }
        #endregion
    }
}
