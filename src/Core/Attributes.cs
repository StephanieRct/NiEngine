using System;
using UnityEngine;

namespace NiEngine
{
    public class EditorField : PropertyAttribute
    {
        public bool PrefixAligned;
        public bool ShowPrefixLabel;
        public bool Inline;
        public bool IsPrefix;
        public float FlexGrow;

        public float MinWidth;
        public float MaxWidth;
        public bool Header;
        public bool ScrollView;
        public bool Track;
        public bool RuntimeOnly;
        public string Prefix;
        public string Suffix;
        public bool AddToEnd;
        public bool Unfold;
        public EditorField(bool showPrefixLabel = true, bool prefixAligned = false, bool inline = false, float flexGrow = 1, float minWidth = -1, float maxWidth = -1, bool header = true, bool scrollView = false, bool track = false, bool runtimeOnly = false, bool isPrefix = false, string suffix = null, string prefix = null, bool addToEnd = false, bool unfold = false)
        {
            ShowPrefixLabel = showPrefixLabel;
            PrefixAligned = prefixAligned;
            Inline = inline;
            FlexGrow = flexGrow;
            MinWidth = minWidth;
            MaxWidth = maxWidth;
            Header = header;
            ScrollView = scrollView;
            Track = track;
            RuntimeOnly = runtimeOnly;
            IsPrefix = isPrefix;
            Suffix = suffix;
            Prefix = prefix;
            AddToEnd = addToEnd;
            Unfold = unfold;
        }
    }
    public class EditorFieldThis : EditorField
    {
        public EditorFieldThis()
            : base(showPrefixLabel:false, inline :true, isPrefix:true, suffix: ".")
        {

        }
    }
    //public class EditorClass : PropertyAttribute
    //{
    //    public bool ShowPrefixLabel;
    //    public bool Inline;
    //    public EditorClass(bool showPrefixLabel = false, bool inline = false)
    //    {
    //        ShowPrefixLabel = showPrefixLabel;
    //        Inline = inline;
    //    }
    //}

    public class ObjectReferencePicker : PropertyAttribute// : EditorField
    {
        public Type BaseType;

        public ObjectReferencePicker()
        {

        }
        public ObjectReferencePicker(Type baseType)//, bool showPrefixLabel = false, bool inline = false)
            //: base(showPrefixLabel, inline)
        {
            BaseType = baseType;
        }
    }


}