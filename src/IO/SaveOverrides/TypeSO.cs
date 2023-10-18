using System;

namespace NiEngine.IO.SaveOverrides
{
    public class TypeSO : ISaveOverrideProxy
    {
        private static TypeSO _Instance = null;
        public static TypeSO Instance => _Instance ??= new();
        public SupportType IsSupportedType(Type type) => type == typeof(Type) ? SupportType.Supported : SupportType.Unsupported;
        public void Save(StreamContext context, Type type, object obj, IOutput io)
        {
            var typeString = StreamContext.TypeToString(type);
            io.Save(context, "typename", typeString);
        }

        public object Load(StreamContext context, Type type, IInput io)
        {
            var typeString = io.Load<string>(context, "typename");
            var result = StreamContext.StringToType(typeString);
            return result;
        }
        public void SaveInPlace(StreamContext context, Type type, object obj, IOutput io)
        {
            Save(context, type, obj, io);
        }
        public void LoadInPlace(StreamContext context, Type type, ref object obj, IInput io)
        {
            throw new NotImplementedException();
        }
    }
}