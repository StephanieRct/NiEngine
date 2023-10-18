using NiEngine.IO.SaveOverrides;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NiEngine.IO
{
    public class RootStreamInput : RootStream, IInput
    {
        public new IInput BaseStream;

        public IEnumerable<object> Keys => BaseStream.Keys;

        public RootStreamInput(IInput baseStream)
            : base(baseStream)
        {
            BaseStream = baseStream;
            TypeRegistry = new TypeRegistry();
            Init();
        }

        public void LoadMetaData(StreamContext context)
        {
            using (var scopeGo = context.ScopeKey("GameObjectMeta", this))
                GameObjectSO.LoadMetaData(context, this);

            using (var scopeNb = context.ScopeKey("NiBehaviourMeta", this))
                NiBehaviourSO.LoadMetaData(context, this);
        }

        public void LoadGameObjects(StreamContext context)
        {
            GameObjectSO.LoadGameObjects(context, this);
        }

        #region IInput interface

        public T Load<T>(StreamContext context, object key)
        {
            return TypeRegistry.Load<T>(context, BaseStream, this, key);
        }

        public object Load(StreamContext context, object key, Type type)
        {
            return TypeRegistry.Load(context, BaseStream, this, key, type);
        }

        public void LoadInPlace<T>(StreamContext context, object key, ref T target)
        {
            TypeRegistry.LoadInPlace<T>(context, BaseStream, this, key, ref target);
        }

        public void LoadInPlace(StreamContext context, object key, Type type, ref object target)
        {
            TypeRegistry.LoadInPlace(context, BaseStream, this, key, type, ref target);
        }

        public bool TryLoad<T>(StreamContext context, object key, out T obj)
        {
            return TypeRegistry.TryLoad<T>(context, BaseStream, this, key, out obj);
        }

        public bool TryLoad(StreamContext context, object key, Type type, out object obj)
        {
            return TypeRegistry.TryLoad(context, BaseStream, this, key, type, out obj);
        }

        public bool TryLoadInPlace<T>(StreamContext context, object key, ref T obj)
        {
            return TypeRegistry.TryLoadInPlace<T>(context, BaseStream, this, key, ref obj);
        }

        public bool TryLoadInPlace(StreamContext context, object key, Type type, ref object target)
        {
            return TypeRegistry.TryLoadInPlace(context, BaseStream, this, key, type, ref target);
        }

        #endregion
    }
}
