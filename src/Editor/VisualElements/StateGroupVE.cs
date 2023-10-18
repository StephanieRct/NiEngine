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
    public class StateGroupVE : VisualElement
    {
        public static Texture2D _IconGroup0;
        public static Texture2D IconGroup0 => _IconGroup0 ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/NiEngine/src/Editor/Assets/IconGroup0.png");
        public static Texture2D _IconGroup1;
        public static Texture2D IconGroup1 => _IconGroup1 ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/NiEngine/src/Editor/Assets/IconGroup1.png");

        public ReactionStateMachine.StateGroup Group => Parent.StateMachine.Groups[Index];
        public ReactionStateMachineVE Parent;
        public int Index;
        NameFoldout Fold;

        //public Foldout Foldout;
        //public TextField GroupName;
        //public VisualElement States;
        public ListView2 StateList;
        //public Button BtDelete;
        //public SerializedObject SerializedObject;
        public SerializedProperty Property;
        public SerializedProperty PropName;
        public SerializedProperty PropNotes;
        public SerializedProperty PropHasActiveState;
        public SerializedProperty PropStates;
        public StateGroupVE(ReactionStateMachineVE parent)//, SerializedObject serializedObject)
        {
            Parent = parent;
            //Index = index;
            //Property = prop;
            ////Group = group;
            //PropName = Property.FindPropertyRelative("GroupName").FindPropertyRelative("Name");
            //PropNotes = Property.FindPropertyRelative("Notes");
            //PropHasActiveState = Property.FindPropertyRelative("HasActiveState");
            //PropStates = Property.FindPropertyRelative("States");

            //Group = group;
            //SerializedObject = serializedObject;
            Build();
        }
        public void Rename(string name)
        {
            PropName.stringValue = name;
            PropName.serializedObject.ApplyModifiedProperties();
        }
        public void ToggleFold(bool value)
        {
            Property.isExpanded = value;
            Property.serializedObject.ApplyModifiedProperties();
        }
        public void Delete()
        {

            Parent.GroupList.DeleteItemAt(Index);
        }
        public void Refresh(int afterStateIndex = 0, bool forced = false)
        {
            StateList.RefreshItemsAfter(afterStateIndex, forced);
            //PropStates = Property.FindPropertyRelative("States");
            //StateList.BindProperty(PropStates);

        }
        //public ReactionStateMachine.State AddNewState()
        //{
        //    var state = new ReactionStateMachine.State();
        //    Group.States.Add(state);
        //    PropStates.serializedObject.Update();
        //    //PropStates.InsertArrayElementAtIndex(PropStates.arraySize);
        //    var newPropState = PropStates.GetArrayElementAtIndex(PropStates.arraySize - 1);

        //    var itor = newPropState.Copy();
        //    int d = itor.depth;
        //    while (itor.Next(true))
        //    {
        //        if (itor.depth <= d)
        //            break;
        //        itor.isExpanded = false;
        //    }

        //    var item = new StateVE(this, newPropState, PropStates.arraySize - 1);
        //    item.PromptRename();
        //    States.Add(item);
        //    //PropStates.serializedObject.ApplyModifiedProperties();
        //    //RefreshAfter(PropStates.arraySize - 1);
        //    RefreshAfter(PropStates.arraySize - 1);
        //    return state;
        //}
        void Build()
        {

            Fold = new NameFoldout();
            Fold.ShowColorIndicator(false);
            Fold.OnRename = Rename;
            Fold.OnToggle = ToggleFold;
            Fold.OnDelete = Delete;
            Add(Fold);


            StateList = new ListView2(header: false, rawItems:true);
            StateList.Track = false;
            StateList.SetAddButtonText("Add State");
            StateList.CreateItem = () => new StateVE(this);
            StateList.BindItem = (ve, prop, index) => (ve as StateVE)?.BindProperty(prop, index);
            Fold.VeContent.Add(StateList);

            //ReactionStateMachineEditor.GroupAsset.CloneTree(Fold.VeContent);
            //States = Fold.VeContent.Query<VisualElement>("States").First();
            //VisualElement btAddState = Fold.VeContent.Query<VisualElement>("btAddState").First();
            //btAddState.RegisterCallback<ClickEvent>(x => AddNewState());

            //{
            //    var state = new ReactionStateMachine.State();
            //    Group.States.Add(state);
            //    PropStates.serializedObject.Update();
            //    //PropStates.InsertArrayElementAtIndex(PropStates.arraySize);
            //    var newPropState = PropStates.GetArrayElementAtIndex(PropStates.arraySize - 1);

            //    var itor = newPropState.Copy();
            //    int d = itor.depth;
            //    while (itor.Next(true))
            //    {
            //        if (itor.depth <= d)
            //            break;
            //        itor.isExpanded = false;
            //    }

            //    var item = new StateVE(this, newPropState, PropStates.arraySize - 1);
            //    item.PromptRename();
            //    States.Add(item);
            //    //PropStates.serializedObject.ApplyModifiedProperties();
            //    //RefreshAfter(PropStates.arraySize - 1);
            //    RefreshAfter(PropStates.arraySize - 1);
            //});
            //RefreshAfter(0);
        }
        //void BindItemAt(int index)
        //{
        //    //var item = States[index] as NiPropertyField;
        //    //item.BindProperty(PropStates.GetArrayElementAtIndex(index));
        //    var item = States[index] as StateVE;
        //    item.BindProperty(this, PropStates.GetArrayElementAtIndex(index), index);
        //}
        //void AddItemToEndAndBindAt(int index)
        //{
        //    //var e = PropStates.GetArrayElementAtIndex(index);
        //    //var item = new NiPropertyField(e);
        //    //item.BindProperty(e);
        //    //States.Add(item);

        //    var e = PropStates.GetArrayElementAtIndex(index);
        //    var item = new StateVE(this, e, index);
        //    States.Add(item);
        //}

        //public void RefreshAfter(int index)
        //{
        //    var group = Group;

        //    int i = index;
        //    for (; i != States.childCount; ++i)
        //    {
        //        if (i < PropStates.arraySize)
        //            BindItemAt(i);
        //        else
        //        {
        //            // delete the remaining of items
        //            while (States.childCount > i)
        //                States.RemoveAt(i);
        //            return;
        //        }
        //    }
        //    for (; i < PropStates.arraySize; ++i)
        //        AddItemToEndAndBindAt(i);
        //    //{
        //    //    var e = PropStates.GetArrayElementAtIndex(i);
        //    //    var item = new StateVE(this, e, i);
        //    //    States.Add(item);
        //    //}
        //}
        public void SetGroupActive(bool value)
        {
            Group.HasActiveState = value;
            Fold.SetIcon(value ? IconGroup1 : IconGroup0);
        }
        public void BindProperty(SerializedProperty property, int index)
        {
            if (Property == property) return;
            Index = index;
            Property = property;

            PropName = Property.FindPropertyRelative("GroupName").FindPropertyRelative("Name");
            PropNotes = Property.FindPropertyRelative("Notes");
            PropHasActiveState = Property.FindPropertyRelative("HasActiveState");
            PropStates = Property.FindPropertyRelative("States");

            
            Fold.Text = PropName.stringValue;
            Fold.SetIcon(PropHasActiveState.boolValue ? IconGroup1 : IconGroup0);
            Fold.SetContentVisible(Property.isExpanded);

            Fold.VeIcon.Unbind();
            Fold.VeIcon.TrackPropertyValue(PropHasActiveState, x =>
            {
                Fold.SetIcon(x.boolValue ? IconGroup1 : IconGroup0);
            });
            StateList.BindProperty(PropStates);
            //style.backgroundColor = (index % 2) == 0 ? new Color(0, 0, 0, 0) : new Color(1, 1, 1, 0.05f);
            //style.backgroundColor = new Color(0,0,0, 0.2f);
            //RefreshAfter(0);
        }
    }


}