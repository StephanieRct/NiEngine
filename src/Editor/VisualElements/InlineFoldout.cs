using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NiEditor
{
    // for content of NameFoldout
    public class EditableLabel
    {

        public Label LbName;
        public TextField TfName;
        public Action<string> OnRename;
    }
    public class InlineFoldout : VisualElement
    {

        // Head direction:columns
        // |- Prefix / icon
        // |- Toggle
        // |- Face
        // |- Inline
        // |- Right
        // VeContent

        public VisualElement VeHead;
        public Toggle TgFold;
        public VisualElement VeContentAligned = new();
        //public VisualElement VeContentAligned = new();
        public VisualElement VeLabel;
        public VisualElement VePrefix;
        public VisualElement VeFace;
        public VisualElement VeInline;
        public VisualElement VeContent;
        public VisualElement VeRight;
        bool m_ContentVisible;

        public bool Value
        {
            get => TgFold?.value ?? false;
            set => SetContentVisible(value);
        }
        public Action<bool> OnToggle;
        public Func<VisualElement> MakeContent;

        protected virtual string GenerateDebugName()
            => $"InlineFoldout(Toggle:{TgFold is not null}, VeFace:{VeFace is not null}, VeInline:{VeInline is not null}, VeRight:{VeRight is not null}, VeContent:{VeContent is not null})";
        public static InlineFoldout Empty() => new InlineFoldout { };
        public InlineFoldout(bool showToggle = true, bool updateLayout = true)
        {
            m_ContentVisible = false;
            ShowToggle(showToggle, updateLayout);
            //VeContentAligned.style.flexDirection = FlexDirection.Row;
            VeContentAligned.style.flexGrow = 1.0f;
            VeHead = new VisualElement();
            VeHead.name = $"InlineFoldout.Head";
            VeHead.style.flexDirection = FlexDirection.Row;

        }

        void BuildHead(VisualElement root)
        {
            if(TgFold is not null) 
                root.Add(TgFold);
            VeContentAligned.Insert(0, VeHead);
            root.Add(VeContentAligned);
            if (VeLabel is not null)
            {
                if (VeLabel.childCount > 0)
                    VeLabel[VeLabel.childCount - 1].style.flexShrink = 0;
                VeHead.Add(VeLabel);
            }
            if (VePrefix is not null)
            {
                if (VePrefix.childCount > 0)
                    VePrefix[VePrefix.childCount - 1].style.flexShrink = 0;
                VeHead.Add(VePrefix);
            }
            if (VeFace is not null)
                VeHead.Add(VeFace);
            if (VeInline is not null)
            {
                if (VeInline.childCount > 0)
                    VeInline[VeInline.childCount - 1].style.flexShrink = 0;
                VeHead.Add(VeInline);
            }

            if (VeRight is not null)
                VeHead.Add(VeRight);
        }

        public void ClearLayout()
        {
            this.RemoveAll();
            VeContentAligned.RemoveAll();
            VeHead.RemoveAll();
            TgFold = default;
            VePrefix = default;
            VeInline = default;
            VeContent = default;
            VeRight = default;
        }
        public void UpdateLayout()
        {
            this.RemoveAll();
            this.name = GenerateDebugName();
            BuildHead(this);
            if (VeContent != null)
            {
                style.flexDirection = FlexDirection.Row;
                style.alignItems = Align.FlexStart;
                //style.flexDirection = FlexDirection.Column;
                if (m_ContentVisible || TgFold is null)
                    VeContentAligned.Add(VeContent);

            }
            else
            {
                style.flexDirection = FlexDirection.Row;
            }
        }

        public void ShowToggle(bool visible, bool updateLayout = true)
        {
            if (visible)
            {
                if (TgFold == null)
                {
                    TgFold = new Toggle();
                    TgFold.AddToClassList("unity-foldout__toggle");
                    TgFold.value = m_ContentVisible;
                    TgFold.RegisterCallback<ChangeEvent<bool>>(x => { UpdateContentVisible(x.newValue); });
                    if(updateLayout)
                        UpdateLayout();
                }
            }
            else if (TgFold != null)
            {
                TgFold = null;
                if (updateLayout)
                    UpdateLayout();
            }
        }
        public VisualElement GetInlineElement(bool updateLayout = true)
        {
            if (VeInline == null)
            {
                VeInline = new VisualElement();
                VeInline.name = $"InlineFoldout.Inline";
                VeInline.style.flexDirection = FlexDirection.Row;
                VeInline.style.flexShrink = 0;
                //VeInline.style.flexGrow = 1;
                if (updateLayout)
                    UpdateLayout();
            }
            return VeInline;
        }

        public void RemoveInlineElement(bool updateLayout = true)
        {
            if (VeInline != null)
            {
                VeInline = null;
                if (updateLayout)
                    UpdateLayout();
            }
        }

        public VisualElement GetPrefixElement(bool updateLayout = true)
        {
            if (VePrefix == null)
            {
                VePrefix = new VisualElement();
                VePrefix.name = $"InlineFoldout.Prefix";
                VePrefix.style.flexDirection = FlexDirection.Row;
                VePrefix.style.flexShrink = 0;
                if (updateLayout)
                    UpdateLayout();
            }
            return VePrefix;
        }

        public void RemovePrefixElement(bool updateLayout = true)
        {
            if (VePrefix != null)
            {
                VePrefix = null;
                if (updateLayout)
                    UpdateLayout();
            }
        }
        bool BuildContent()
        {
            if (VeContent is null && MakeContent is not null)
            {
                VeContent = MakeContent.Invoke();
                VeContent.name = $"InlineFoldout.Content";
                return true;
            }
            return false;
        }
        public bool ReBuildContent(bool updateLayout = true)
        {
            if (MakeContent is not null)
            {
                VeContent = MakeContent.Invoke();
                VeContent.name = $"InlineFoldout.Content";
                if (updateLayout)
                    UpdateLayout();
                return true;
            }
            return false;
        }
        public VisualElement GetContentElement(bool updateLayout = true)
        {
            if (VeContent == null)
            {
                BuildContent();
                if(VeContent is null)
                    VeContent = new VisualElement();
                if (updateLayout)
                    UpdateLayout();
            }
            return VeContent;
        }

        public void RemoveContentElement(bool updateLayout = true)
        {
            if (VeContent != null)
            {
                VeContent = null;
                if (updateLayout)
                    UpdateLayout();
            }
        }
        public void SetContentVisible(bool visible, bool notify = true)
        {
            if(TgFold is not null)
                TgFold.value = visible;
            UpdateContentVisible(visible, notify);
        }
        void UpdateContentVisible(bool visible, bool notify = true)
        {
            bool changed = m_ContentVisible != visible;
            m_ContentVisible = visible;
            if (changed && visible && BuildContent())
            {
                UpdateLayout();
            }
            else
            {
                if (VeContent is not null && changed)
                {
                    if (visible)
                        VeContentAligned.Add(VeContent);
                    else
                        VeContentAligned.Remove(VeContent);
                }
                
            }
            if(notify && changed)
                OnToggle?.Invoke(m_ContentVisible);

        }
        

    }

}