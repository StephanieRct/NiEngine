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
    public class NameFoldout : VisualElement
    {
        public static VisualTreeAsset _Asset;
        public static VisualTreeAsset Asset => _Asset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/NameFoldout.uxml");


        public Toggle TgFold;
        public Label LbName;
        public TextField TfName;
        public VisualElement VeContent;
        public VisualElement VeIcon;
        public VisualElement VeEditName;
        VisualElement VeContentParent;
        bool m_ContentVisible;

        public Action<string> OnRename;
        public Action<bool> OnToggle;
        public System.Action OnDelete;
        public System.Action OnIconClick;
        public bool HasDeleteButton { get; private set; }
        public string Text
        {
            get => LbName.text;
            set
            {
                VeEditName.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.Flex : DisplayStyle.None;
                LbName.text = value;
            }
        }
        public NameFoldout(bool deleteButton = true)
        {
            HasDeleteButton = deleteButton;

            style.flexDirection = FlexDirection.Row;
            Asset.CloneTree(this);

            VeContent = this.Query<VisualElement>("veContent").First();
            VeContentParent = VeContent.parent;
            VeContent.parent.Remove(VeContent);
            m_ContentVisible = false;


            TgFold = this.Query<Toggle>("tgFold").First();
            TgFold.RegisterCallback<ChangeEvent<bool>>(x =>
            {
                UpdateContentVisible(x.newValue);
            });

            LbName = this.Query<Label>("lbName").First();

            TfName = this.Query<TextField>("tfName").First();
            TfName.style.display = DisplayStyle.None;
            TfName.RegisterCallback<KeyDownEvent>(x =>
            {
                if (x.keyCode != KeyCode.Return && x.keyCode != KeyCode.KeypadEnter) return;
                UpdateName(TfName.text);
            });
            TfName.RegisterCallback<FocusOutEvent>(x =>
            {
                UpdateName(TfName.text);
            });

            VeEditName = this.Query<VisualElement>("veEditName").First();
            VeEditName.RegisterCallback<ClickEvent>(x => PromptRename());
            LbName.RegisterCallback<ClickEvent>(x => PromptRename());

            VeIcon = this.Query<VisualElement>("veIcon").First();
            VeIcon.RegisterCallback<ClickEvent>(x => OnIconClick?.Invoke());

            var btDelete = this.Query<Button>("btDelete").First();
            if (HasDeleteButton)
                btDelete.RegisterCallback<ClickEvent>(x => OnDelete?.Invoke());
            else
                btDelete.style.display = DisplayStyle.None;


        }
        public void ShowColorIndicator(bool visible)
        {
            this.Query<VisualElement>("veIsActive").ForEach(x=> x.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None);
        }
        public void SetIcon(Texture2D icon)
        {
            VeIcon.style.backgroundImage = icon;
        }
        public void SetContentVisible(bool visible)
        {
            TgFold.value = visible;
            UpdateContentVisible(visible);
        }
        void UpdateContentVisible(bool visible)
        {
            if (visible && !m_ContentVisible)
                VeContentParent.Add(VeContent);
            else if (!visible && m_ContentVisible)
                VeContent.parent.Remove(VeContent);
            m_ContentVisible = visible;
            OnToggle?.Invoke(m_ContentVisible);
        }

        public void PromptRename()
        {
            LbName.style.display = DisplayStyle.None;
            TfName.style.display = DisplayStyle.Flex;
            TfName.value = LbName.text;
            TfName.Focus();
        }
        void UpdateName(string name)
        {
            LbName.style.display = DisplayStyle.Flex;
            TfName.style.display = DisplayStyle.None;
            Text = name;
            OnRename?.Invoke(TfName.text);
        }

    }

}