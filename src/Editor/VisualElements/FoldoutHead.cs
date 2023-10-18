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

    public class FoldoutHead : VisualElement
    {
        public Toggle TgExpand { get; private set; }
        public VisualElement VePrefix { get; private set; }
        public Label LbText { get; private set; }
        public Label LbCount { get; private set; }

        public FoldoutHead()
        {
            Assets.FoldoutHeadAsset.CloneTree(this);
            TgExpand = this.Query<Toggle>("tgExpand");
            VePrefix = this.Query<VisualElement>("vePrefix");
            LbText = this.Query<Label>("lbText");
            LbCount = this.Query<Label>("lbCount");
            style.flexGrow = 1;
            TgExpand.RegisterValueChangedCallback(x =>
            {
                m_ExpandCallbacks?.Invoke(x);
            });
        }
        public void SetCount(int value)
        {
            if (value > 0)
            {
                LbCount.text = $"({value})";
                TgExpand.style.visibility = Visibility.Visible;
                LbCount.style.display = DisplayStyle.Flex;
                //TgExpand.style.display = DisplayStyle.Flex;
            }
            else
            {
                TgExpand.style.visibility = Visibility.Hidden;
                LbCount.style.display = DisplayStyle.None;
                //TgExpand.style.display = DisplayStyle.None;
                LbCount.text = "";
            }
        }
        EventCallback<ChangeEvent<bool>> m_ExpandCallbacks;
        public void RegisterExpandChangedCallback(EventCallback<ChangeEvent<bool>> callback)
        {
            m_ExpandCallbacks += callback;
        }
        public void UnregisterAllExpandChangedCallback()
        {
            m_ExpandCallbacks = null;
        }
    }
}