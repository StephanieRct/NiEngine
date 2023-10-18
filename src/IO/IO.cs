using System;
using System.Collections.Generic;
using UnityEngine;


namespace NiEngine.IO
{
    //public struct SavingState
    //{
    //    public bool Success;
    //    public string Message;
    //    public object Reference;
    //    public void Merge(SavingState other)
    //    {
    //        Success = Success && other.Success;

    //        Message = other.Message;
    //        Reference = other.Reference;
    //    }

    //    public static SavingState Successful => new SavingState
    //    {
    //        Success = true
    //    };
    //    public static SavingState Failure(string reason, object reference) => new SavingState
    //    {
    //        Success = false,
    //        Message = reason,
    //        Reference = reference
    //    };
    //}

    public struct Scope<T> : IDisposable
        where T : IStream
    {
        public object Key;
        public T IO;
        public StreamContext Context;
        public bool Success;
        //public bool ErrorOnFail { get; private set; }
        public Scope(StreamContext context, object key, T io, bool errorOnFail = true)
        {
            Context = context;
            Key = key;
            IO = io;
            //ErrorOnFail = errorOnFail;
            //Context.Scopes.Add(Key);
            if (!IO.ScopeBegin(Context, Key))
            {
                if (errorOnFail)
                    Context.LogError($"Failed to begin scope '{key}'", key);
                //Context.Scopes.RemoveAt(Context.Scopes.Count - 1);
                Success = false;
            }
            else
                Success = true;

        }
        public void Dispose()
        {
            if (Success)
            {
                IO.ScopeEnd(Context, Key);
//                var k = Context.Scopes[^1];
//                Context.Scopes.RemoveAt(Context.Scopes.Count - 1);
//#if UNITY_EDITOR
//                if (!k.Equals(Key))
//                {
//                    Debug.LogError($"SaveScope bad match\nReceived: {Key}\nShould be: {k}");
//                }
//#endif
            }
        }
    }

    public class StreamContext
    {
        public bool WithDebugData = true;
        public bool IgnoreUnhandledTypes;
        public bool StopOnError = true;
        public bool StopOnFailure = false;
        public bool ThrowOnError = true;
        public bool ThrowOnFailure = false;

        public bool MustStop;
        public int ErrorCount;

        public bool Success = true;
        public string Message;
        public object Reference;
        public bool LogError(string message, object reference = null)
        {
            ++ErrorCount;
            if (StopOnError)
                MustStop = true;
            Success = false;
            Message = message;
            Reference = reference;
            Debug.LogError(message, reference as UnityEngine.Object);
            if (ThrowOnError)
                throw new Exception(message);
            return MustStop;
        }
        public bool LogFailure(string message, bool mustStop = false, object reference = null)
        {
            ++ErrorCount;
            if (mustStop || StopOnFailure)
                MustStop = true;
            Success = false;
            Message = message;
            Reference = reference;
            Debug.LogError(message, reference as UnityEngine.Object);
            if (ThrowOnFailure)
                throw new Exception(message);
            return MustStop;
        }
        public void LogAddition(string message, object reference = null)
        {
            Debug.LogError($"[cont] {message}", reference as UnityEngine.Object);
        }
        public static string TypeToString(Type type)
        {
            return type.AssemblyQualifiedName;
        }
        public static Type StringToType(string s)
        {
            return Type.GetType(s);
        }
        public object CreateObject(Type type, object[] parameters = null)
        {
            if (parameters is not null)
                return Activator.CreateInstance(type, parameters);
            return Activator.CreateInstance(type);
        }
        public Array CreateArray(Type arrayType, int lenght)
        {
            return (Array)Activator.CreateInstance(arrayType, lenght);
        }

        public Array CreateArrayOf(Type elementType, int lenght)
            => CreateArray(elementType.MakeArrayType(), lenght);

        public Scope<T> ScopeKey<T>(object key, T io, bool errorOnFail = true)
            where T : IStream
        {
            return new Scope<T>(this, key, io, errorOnFail: errorOnFail);
        }
    }

    public interface IStream
    {
        public bool IsSupportedType(Type type);
        public bool ScopeBegin(StreamContext context, object key);
        public void ScopeEnd(StreamContext context, object key);
    }
    public interface IOutput : IStream
    {

        public void Save<T>(StreamContext context, object key, T value);
        public void Save(StreamContext context, object key, Type type, object value);

        // not implemented / supported
        //public void SaveReference<T>(StreamContext context, object key, T value)
        //    where T : class;

        // Save a value that will be loaded with a LoadInPlace call.
        public void SaveInPlace<T>(StreamContext context, object key, T value);

        // Save a value that will be loaded with a LoadInPlace call.
        public void SaveInPlace(StreamContext context, object key, Type type, object value);

    }
    public interface IInput : IStream
    {
        IEnumerable<object> Keys { get; }
        public T Load<T>(StreamContext context, object key);
        public object Load(StreamContext context, object key, Type type);

        public bool TryLoad<T>(StreamContext context, object key, out T obj);
        public bool TryLoad(StreamContext context, object key, Type type, out object obj);

        public void LoadInPlace<T>(StreamContext context, object key, ref T target);
        public void LoadInPlace(StreamContext context, object key, Type type, ref object target);

        public bool TryLoadInPlace<T>(StreamContext context, object key, ref T obj);
        public bool TryLoadInPlace(StreamContext context, object key, Type type, ref object target);


        //public T LoadReference<T>(StreamContext context, object key)
        //    where T : class;
    }

    public interface IStreamBranch
    {
        IOutput GetOutput(IOutput a);
        IInput GetInput(IInput a);
    }
    public interface ISaveOverride
    {
        void Save(StreamContext context, IOutput io);
        void Load(StreamContext context, IInput io);
    }
    //public interface ISaveCallback
    //{
    //    void BeforeSave(StreamContext context, IOutput io);
    //    void AfterLoad(StreamContext context, IInput io);
    //}
    public enum SupportType
    {
        Unsupported,
        Supported,
        Ignored,
    }
    public interface ISaveOverrideProxy
    {
        public SupportType IsSupportedType(Type type);
        void Save(StreamContext context, Type type, object obj, IOutput io);
        object Load(StreamContext context, Type type, IInput io);
        void SaveInPlace(StreamContext context, Type type, object obj, IOutput io);
        void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io);
    }


}
