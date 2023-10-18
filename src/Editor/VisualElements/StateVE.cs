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
    public class StateVE : VisualElement
    {
        [System.Diagnostics.Conditional("NIEDITOR_PRINT")]
        void Log(string msg)
        {
            Debug.Log(msg);
        }
        public static Texture2D _IconState0;
        public static Texture2D IconState0 => _IconState0 ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/NiEngine/src/Editor/Assets/IconState0.png");
        public static Texture2D _IconStateBegin;
        public static Texture2D IconStateBegin => _IconStateBegin ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/NiEngine/src/Editor/Assets/IconStateBegin.png");
        public static Texture2D _IconState1;
        public static Texture2D IconState1 => _IconState1 ??= AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/NiEngine/src/Editor/Assets/IconState1.png");

        public static ReactionStateMachine.State CopiedState;
        public ReactionStateMachine.State State => Parent.Group.States[Index];
        public ReactionStateMachine StateMachine => Parent.Parent.StateMachine;
        public StateGroupVE Parent;

        SerializedProperty Property;
        SerializedProperty PropIsExpended;
        SerializedProperty PropName;
        SerializedProperty PropIsActiveState;
        SerializedProperty PropBegan;

        //SerializedProperty PropHasConditions;
        
        SerializedProperty PropConditions;
        SerializedProperty PropOnBegin;
        //SerializedProperty PropOnUpdate;
        SerializedProperty PropOnEnd;

        NameFoldout Fold;

        PropertyField PfLastEventBeginParameters;
        VisualElement VeContentParent;
        //VisualElement VeContent;
        List<VisualElement> VeIsActive;

        //PropertyField PfConditions;

        //ListView2 LvConditions;
        ListView2 LvOnBegin;
        ListView2 LvOnEnd;

        //PropertyField PfOnBegin;
        //PropertyField PfOnUpdate;
        //PropertyField PfOnEnd;
        public int Index;
        private bool m_Built;
        public StateVE(StateGroupVE parent)
        {
            Parent = parent;
            Build();
        }
        public StateVE(StateGroupVE parent, SerializedProperty prop, int index)
        {
            Parent = parent;
            Index = index;
            Log($"StateVE ctor StateName:{State.StateName}");
            //State = state;
            Build();
            BindProperty(prop, index);

        }
        void Build()
        {
            Fold = new NameFoldout();

            //VeContent = Fold.Query<VisualElement>("veContent").First();
            Add(Fold);
            VeIsActive = this.Query<VisualElement>("veIsActive").ToList();

        }

        bool BuildContent()
        {
            if (m_Built) return false;
            ReactionStateMachineEditor.StateAsset.CloneTree(Fold.VeContent);
            var veStateContent = Fold.VeContent.Query<VisualElement>("veStateContent").First();

            //LvConditions = new ListView2();
            //LvConditions.SetMaxHeight(512);
            //LvConditions.SetIcon(Assets.IconCondition);
            ////LvConditions.ShowIcon(false);
            //LvConditions.SetColor(new Color(0.75f, 0.75f, 0));
            ////LvConditions.style.marginBottom = 2;

            LvOnBegin = new ListView2(header:false);
            LvOnBegin.SetAddButtonText("+ Begin");
            LvOnBegin.SetMaxHeight(512);
            //LvOnBegin.style.marginBottom = 2;
            LvOnBegin.SetIcon(Assets.IconAction);
            //LvOnBegin.ShowIcon(false);
            //LvOnBegin.SetColor(new Color(0.0f, 0.75f, 0.75f));

            LvOnEnd = new ListView2(header:false);
            LvOnEnd.SetAddButtonText("+ End");
            LvOnEnd.SetMaxHeight(512);
            //LvOnEnd.style.marginBottom = 6;
            LvOnEnd.SetIcon(Assets.IconAction);
            //LvOnEnd.ShowIcon(false);
            LvOnEnd.SetColor(new Color(0, 0, 0, 0.4f));
            
            //veStateContent.Add(LvConditions);
            veStateContent.Add(LvOnBegin);
            //veStateContent.Add(new Label("On End:"));
            veStateContent.Add(LvOnEnd);
            if (EditorMenu.ShowRuntimeFields || EditorApplication.isPlaying)
            {
                PfLastEventBeginParameters = new PropertyField();
                PfLastEventBeginParameters.style.marginLeft = 16;
                veStateContent.Add(PfLastEventBeginParameters);
            }
            Fold.VeContent.Query<VisualElement>("veIsActive").ToList(VeIsActive);
            Refresh();
            m_Built = true;
            return true;
        }

        void BindActiveState()
        {
            if (VeIsActive?.Count > 0)
            {
                var element = VeIsActive[0];
                element.Unbind();
                element.TrackPropertyValue(PropIsActiveState, x =>
                {
                    UpdateIcon();
                    //Fold.SetIcon(x.boolValue ? IconState1 : IconState0);
                    foreach (var v in VeIsActive)
                        v.style.backgroundColor = x.boolValue ? Color.green : Color.black;
                });
                element.TrackPropertyValue(PropBegan, x =>
                {
                    UpdateIcon();
                });
            }
        }
        void BindContent()
        {
            Log($"StateVE BindContent StateName:{State.StateName}");
            
            //LvConditions.SetText($"{State.StateName}.{PropConditions.displayName}");
            LvOnBegin.SetText($"{State.StateName}.{PropOnBegin.displayName}");
            LvOnEnd.SetText($"{State.StateName}.{PropOnEnd.displayName}");

            //if (PropHasConditions.boolValue)
            //{
            //    LvConditions.BindProperty(PropConditions.FindPropertyRelative("Conditions"));
            //}
            //LvConditions.style.display = PropHasConditions.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            LvOnBegin.BindProperty(PropOnBegin.FindPropertyRelative("Actions"));
            LvOnEnd.BindProperty(PropOnEnd.FindPropertyRelative("Actions"));

            if(PfLastEventBeginParameters != null)
                PfLastEventBeginParameters.BindProperty(Property.FindPropertyRelative("Processor").FindPropertyRelative("LastOnBeginParameters"));
            BindActiveState();
        }
        public void BindProperty(SerializedProperty property, int index)
        {
            if (property == Property && Index == index) return;

            Index = index;
            Property = property;

            PropIsExpended = Property.FindPropertyRelative("IsExpended");
            PropName = Property.FindPropertyRelative("StateName").FindPropertyRelative("Name");
            PropIsActiveState = Property.FindPropertyRelative("IsActiveState");
            PropBegan = Property.FindPropertyRelative("Began");
            //PropHasConditions = property.FindPropertyRelative("HasConditions");
            
            PropConditions = property.FindPropertyRelative("Conditions");
            PropOnBegin = property.FindPropertyRelative("OnBegin");
            PropOnEnd = property.FindPropertyRelative("OnEnd");
            
            Fold.OnRename = null;
            Fold.OnToggle = null;
            Fold.OnDelete = null;
            Fold.OnIconClick = null;


            if (PropIsExpended.boolValue)
            {
                BuildContent();
                BindContent();
            }
            else
                BindActiveState();
            Refresh();


            Fold.SetContentVisible(PropIsExpended.boolValue);
            UpdateIcon();
            Fold.OnDelete = OnDelete;
            Fold.OnIconClick = OnIconClick;
            Fold.VeIcon.RegisterCallback<DragEnterEvent>(x => OnDragEnter());
            Fold.VeIcon.RegisterCallback<DragPerformEvent>(x => OnDrag());
            Fold.VeIcon.focusable = true;
            Fold.VeIcon.pickingMode = PickingMode.Position;
            Fold.VeIcon.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Copy", (x) => { CopiedState = (ReactionStateMachine.State)this.State.Clone(); });
                var pastStatus = CopiedState != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;

                evt.menu.AppendAction("Paste/Before", (x) => 
                {
                    if (CopiedState != null)
                    {
                        Parent.Group.InsertStateAt(Index, (ReactionStateMachine.State)CopiedState.Clone());
                        Property.serializedObject.ApplyModifiedProperties();
                        Property.serializedObject.Update();
                        Parent.Refresh(Index, true);
                    }
                }, pastStatus);
                evt.menu.AppendAction("Paste/InPlace", (x) =>
                {
                    if (CopiedState != null)
                    {
                        Parent.Group.SetStateAt(Index, (ReactionStateMachine.State)CopiedState.Clone());
                        Property.serializedObject.ApplyModifiedProperties();
                        Property.serializedObject.Update();
                        Parent.Refresh(Index, true);
                    }
                }, pastStatus);
                evt.menu.AppendAction("Paste/After", (x) =>
                {
                    if (CopiedState != null)
                    {
                        Parent.Group.InsertStateAt(Index+1, (ReactionStateMachine.State)CopiedState.Clone());
                        Property.serializedObject.ApplyModifiedProperties();
                        Property.serializedObject.Update();
                        Parent.Refresh(Index + 1, true);
                    }
                }, pastStatus);
                evt.menu.AppendAction("Insert/Before", (x) =>
                {
                    Parent.Group.InsertStateAt(Index, new ReactionStateMachine.State());
                    Property.serializedObject.ApplyModifiedProperties();
                    Property.serializedObject.Update();
                    Parent.Refresh(Index, true);
                });
                evt.menu.AppendAction("Insert/After", (x) =>
                {
                    Parent.Group.InsertStateAt(Index + 1, new ReactionStateMachine.State());
                    Property.serializedObject.ApplyModifiedProperties();
                    Property.serializedObject.Update();
                    Parent.Refresh(Index, true);
                });
            }));

            //property.
            Fold.OnRename += n =>
            {
                PropName.stringValue = n;
                PropName.serializedObject.ApplyModifiedProperties();
            };
            Fold.OnToggle += n =>
            {
                PropIsExpended.boolValue = n;
                Property.serializedObject.ApplyModifiedProperties();
                if (n && BuildContent())
                    BindContent();
            };
            style.backgroundColor = (index % 2) == 0 ? new Color(0, 0, 0, 0) : new Color(1, 1, 1, 0.02f);
        }

        void UpdateIcon()
        {
            Fold.SetIcon(PropIsActiveState.boolValue ? (PropBegan.boolValue ? IconState1 : IconStateBegin) : IconState0);
        }
        void OnDelete()
        {
            Parent.StateList.DeleteItemAt(Index);
            //Parent.PropStates.DeleteArrayElementAtIndex(Index);
            //Parent.PropStates.serializedObject.ApplyModifiedProperties();
            //Parent.RefreshAfter(Index);
            //Parent.RefreshAfter(0);
        }

        void OnIconClick()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                if (Property.serializedObject.targetObject is ReactionStateMachine rsm)
                {
                    rsm.StateToToggleOnUpdate = PropName.stringValue;

                }
            }
            else
            {
                Undo.RecordObject(StateMachine, "Set Initial State");
                if (!State.IsActiveState)
                {
                    foreach (var s in Parent.Group.States)
                    {
                        s.IsActiveState = false;
                        s.Began = false;
                    }
                    State.IsActiveState = true;
                    State.Began = false;
                    Parent.SetGroupActive(true);

                } else if (!State.Began)
                {
                    State.Began = true;
                }
                else
                {

                    State.IsActiveState = false;
                    State.Began = false;
                    Parent.SetGroupActive(false);
                }

                //    if (!State.IsActiveState)
                //    foreach (var s in Parent.Group.States)
                //        s.IsActiveState = false;
                //var value = !State.IsActiveState;
                //State.IsActiveState = value;
                //Parent.SetGroupActive(value);
                Parent.PropStates.serializedObject.Update();
            }
        }

        void OnDragEnter()
        {

            if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] is GameObject triggerObject)
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            else
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        }
        void OnDrag()
        {


            if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences[0] is GameObject triggerObject)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                if (Property.serializedObject.targetObject is ReactionStateMachine rsm)
                {
                    foreach (var (group, state) in rsm.AllStatesNamed(PropName.stringValue))
                    {
                        if (!UnityEditor.EditorApplication.isPlaying)
                        {
                            group.Handshake(rsm);
                            state.Handshake(rsm, group);
                        }
                        var parameters = EventParameters.Trigger(rsm.gameObject, rsm.gameObject, triggerObject);
                        if (state.IsActiveState)
                        {
                            Debug.Log($"Deactivating State '{state.StateName.Name}' on gameObject '{rsm.gameObject.name}' with trigger object '{triggerObject.name}'.");
                            group.DeactivateAllState(parameters);
                        }
                        else
                        {
                            Debug.Log($"Activating State '{state.StateName.Name}' on gameObject '{rsm.gameObject.name}' with trigger object '{triggerObject.name}'.");
                            group.DeactivateAllState(parameters);
                            group.SetActiveState(rsm, state, parameters);
                        }
                        if (!UnityEditor.EditorApplication.isPlaying)
                            Property.serializedObject.Update();
                    }

                }
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
        }


        public void PromptRename()
        {
            Fold.PromptRename();
        }
        public void Refresh()
        {
            Fold.Text = PropName.stringValue;
            if(VeIsActive is not null)
                foreach (var v in VeIsActive)
                    v.style.backgroundColor = PropIsActiveState.boolValue ? Color.green : Color.black;
        }
    }
}