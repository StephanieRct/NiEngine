using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NiEngine;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace NiEditor
{

    //public class ReactionStateMachineVE : VisualElement
    //{
    //    public ReactionStateMachine StateMachine => SerializedObject.targetObject as ReactionStateMachine;
    //    //public List<StateGroupVE> StateGroupVEs = new();


    //    ListView2 GroupList;

    //    VisualElement RootGroup;

    //    public SerializedObject SerializedObject;

    //    public SerializedProperty PropGroups;
    //    public ReactionStateMachineVE(SerializedObject serializedObject)
    //    {
    //        SerializedObject = serializedObject;
    //        PropGroups = SerializedObject.FindProperty("Groups");
    //        Build();
    //    }

    //    public void OnUndo()
    //    {
    //        Debug.Log("OnUndo");
    //        Clear();
    //        Build();
    //    }
    //    void Build()
    //    {
    //        //StateMachine = SerializedObject.targetObject as ReactionStateMachine;
    //        ReactionStateMachineEditor.StateMachineAsset.CloneTree(this);

    //        RootGroup = this.Query<VisualElement>("Groups").First();

    //        for (int i = 0; i != PropGroups.arraySize; i++)
    //            AddGroupToEnd(PropGroups.GetArrayElementAtIndex(i), i, StateMachine.Groups[i]);

    //        VisualElement btAddGroup = this.Query<VisualElement>("btAddGroup").First();
    //        btAddGroup.RegisterCallback<ClickEvent>(x =>
    //        {
    //            var group = new ReactionStateMachine.StateGroup();
    //            StateMachine.Groups.Add(group);
    //            SerializedObject.Update();

    //            //PropGroups.InsertArrayElementAtIndex(PropGroups.arraySize);
    //            var newPropGroup = PropGroups.GetArrayElementAtIndex(PropGroups.arraySize - 1);

    //            var itor = newPropGroup.Copy();
    //            int d = itor.depth;
    //            while (itor.Next(true))
    //            {
    //                if (itor.depth <= d)
    //                    break;
    //                itor.isExpanded = false;
    //            }

    //            // add to ui
    //            AddGroupToEnd(newPropGroup, PropGroups.arraySize - 1, group);
    //            //SerializedObject.Update();
    //            SerializedObject.ApplyModifiedProperties();

    //        });


    //    }
    //    //public ReactionStateMachine GetStateMachine()
    //    //{
    //    //    StateMachine = SerializedObject.targetObject as ReactionStateMachine;
    //    //    return StateMachine;
    //    //}
    //    public void DeleteGroupAt(int index, VisualElement ve)
    //    {
    //        PropGroups.DeleteArrayElementAtIndex(index);
    //        PropGroups.serializedObject.ApplyModifiedProperties();
    //        RootGroup.Remove(ve);
    //        RefreshAfter(0);
    //    }
    //    void AddGroupToEnd(SerializedProperty prop, int index, ReactionStateMachine.StateGroup group)//, SerializedObject serializedObject)
    //    {
    //        //var groupRoot = new NiPropertyField(prop);
    //        //groupRoot.BindProperty(prop);
    //        //RootGroup.Add(groupRoot);

    //        var newGroup = new StateGroupVE(this, prop, index);
    //        RootGroup.Add(newGroup);

    //    }
    //    public void RefreshAfter(int index)
    //    {
    //        Debug.Log($"ReactionStateMachineVE.RefreshAfter({index})");
    //        var sm = StateMachine;

    //        int i = index;
    //        for (; i != RootGroup.childCount; ++i)
    //        {
    //            var item = RootGroup[i] as StateGroupVE;
    //            if (i < PropGroups.arraySize)
    //            {
    //                item.BindProperty(this, PropGroups.GetArrayElementAtIndex(i), i);
    //            }
    //            else
    //            {
    //                // delete the remaining of items
    //                while (RootGroup.childCount > i)
    //                    RootGroup.RemoveAt(i);
    //                return;
    //            }
    //        }
    //        for (; i < PropGroups.arraySize; ++i)
    //        {
    //            var e = PropGroups.GetArrayElementAtIndex(i);
    //            var item = new StateGroupVE(this, e, i);
    //            RootGroup.Add(item);
    //        }
    //    }
    //}

    public class ReactionStateMachineVE : VisualElement
    {
        public ReactionStateMachineEditor Editor;
        public ReactionStateMachine StateMachine => SerializedObject.targetObject as ReactionStateMachine;
        //public List<StateGroupVE> StateGroupVEs = new();


        

        public SerializedObject SerializedObject;

        public SerializedProperty PropGroups;

        public ListView2 GroupList;
        public ReactionStateMachineVE(ReactionStateMachineEditor editor, SerializedObject serializedObject)
        {
            Editor = editor;
            
            SerializedObject = serializedObject;
            PropGroups = SerializedObject.FindProperty("Groups");
            Build();
        }

        public void OnUndo()
        {
            Debug.Log("OnUndo");
            Clear();
            Build();
        }
        public void Refresh()
        {
            Clear();
            Build();
        }
        void Build()
        {
            GroupList = new ListView2(header:false, rawItems: true);
            GroupList.Track = false;
            GroupList.SetAddButtonText("Add Group");
            GroupList.CreateItem = () => new StateGroupVE(this);
            GroupList.BindItem = (ve, prop, index) => (ve as StateGroupVE)?.BindProperty(prop, index);
            GroupList.BindProperty(PropGroups);
            Add(GroupList);
        }
    }
}