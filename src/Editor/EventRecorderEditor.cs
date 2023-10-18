using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NiEngine.Recording;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using NiEngine;

namespace NiEditor
{
    public class EventRecorderEditor : EditorWindow
    {

        [MenuItem("Tools/NiEngine/Event Recorder")]
        public static void ShowEventRecorderEditor()
        {
            // This method is called when the user selects the menu item in the Editor
            EditorWindow wnd = GetWindow<EventRecorderEditor>();
            wnd.titleContent = new GUIContent("NiEngine Event Recorder");
        }

        [MenuItem("Window/Analysis/NiEngine Event Recorder")]
        public static void ShowEventRecorderEditorWindow() => ShowEventRecorderEditor();

        EventRecorder m_Recorder;
        bool m_Recording;

        void StartRecording()
        {
            var er = new EventRecorder();
            EventRecorder.Instance = er;
            OnStartRecording();
        }
        void OnStartRecording()
        {
            m_Recorder = EventRecorder.Instance;
            Data = new EventChildrenTableData(m_Recorder.Root);
            Data.AddCallbackOnRowCountChange((TableView.ITableData x, int b, int a) =>
            {
                Refresh();
            });
            Table.Data = Data;
            Table.Build();

            BtRecord.text = "Stop";
            m_Recording = true;
        }
        void OnEndRecording()
        {
            BtRecord.text = "Record";
            m_Recording = false;
            m_Recorder = null;
        }
        bool IsUICreated => Table != null;
        void OnGUI()
        {
            if (!IsUICreated) return;
            if (EventRecorder.Instance != null)
            {
                if(EventRecorder.Instance != m_Recorder || !m_Recording)
                {
                    OnStartRecording();
                }
                if (EventCountLastUpdate != m_Recorder.Root.ChildCount)
                {
                    Refresh();
                    EventCountLastUpdate = m_Recorder.Root.ChildCount;
                }
            }
            else if (m_Recording)
            {
                OnEndRecording();
            }
        }
        void Update()
        {
            if (EventRecorder.Instance != null)
            {
                var recorder = EventRecorder.Instance;
                if (EventCountLastUpdate != recorder.Root.ChildCount)
                {
                    Repaint();
                }
            }
        }
        public void Refresh()
        {
            Table.RefreshViewArea();
            
            //EventCountLastUpdate = Data.Recorder.Events.Count;

        }
        int EventCountLastUpdate;
        Button BtRecord;
        //ListView LvRecords;
        TableView Table;
        EventChildrenTableData Data;
        public void CreateGUI()
        {
            var topbar = new VisualElement();
            topbar.style.flexDirection = FlexDirection.Row;
            BtRecord = new Button();
            BtRecord.text = "Record";
            topbar.Add(BtRecord);
            var tgRecordOnPlay = new Toggle();
            tgRecordOnPlay.style.flexDirection = FlexDirection.RowReverse;
            tgRecordOnPlay.label = "Record On Play";
            tgRecordOnPlay.value = NiEngine.EditorMenu.RecordOnPlay;
            tgRecordOnPlay.RegisterValueChangedCallback(e =>
            {
                NiEngine.EditorMenu.SetRecordOnPlay(e.newValue);
            });
            topbar.Add(tgRecordOnPlay);

            rootVisualElement.Add(topbar);

            //// Create a two-pane view with the left pane being fixed with
            //var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            
            //// Add the view to the visual tree by adding it as a child to the root element
            //rootVisualElement.Add(splitView);

            //// A TwoPaneSplitView always needs exactly two child elements
            //var leftPane = new VisualElement();
            //splitView.Add(leftPane);
            //var rightPane = new VisualElement();
            //splitView.Add(rightPane);


            Table = new TableView();
            Table.Header = new TableView.HeaderLabel(
                "Frame",
                "Hash                   ",
                "Event                                                                                                                      ", 
                "Owner GameObject                                         ",
                "Owner Component                                                              ",
                "Phase      ", 
                "Reaction                  ",
                "Trigger                                                                      "
                );

            //leftPane.Add(Table);
            rootVisualElement.Add(Table);
            BtRecord.RegisterCallback<ClickEvent>(x =>
            {
                if (EventRecorder.Instance == null) 
                    StartRecording();
                else
                {
                    EventRecorder.Instance = null;
                    BtRecord.text = "Record";
                }
            });
        }
        public enum ColumnId
        {
            TimeStamp,
            Hash,
            Foldout,
            GameObject,
            Component,
            Phase,
            Reaction,
            Trigger,
            Count
        }

        public class EventChildrenTableData : FoldoutHeadTableData
        {
            public EventRecord ParentRecord;

            public EventChildrenTableData(EventRecord record)
            {
                ParentRecord = record;
                //ParentRecord.OnNewChild += OnNewChild;
            }

            //void OnNewChild(Recording.EventRecord record)
            //{
            //    if (record is not Recording.ConditionRecord)
            //    {
                    
            //    }
            //}
            public override int GetColumnCount() => (int)ColumnId.Count;

            public override float GetColumnIndent(int columnIndex, int depth) => (ColumnId)columnIndex == ColumnId.Foldout ? depth * 8.0f : 0.0f;
            public override VisualElement MakeItem(int columnIndex)
            {
                return (ColumnId)columnIndex switch
                {
                    ColumnId.TimeStamp => new Label(),
                    ColumnId.Hash => new Label(),
                    ColumnId.Foldout => base.MakeItem(columnIndex),
                    ColumnId.GameObject => new Label(),
                    ColumnId.Component => new Label(),
                    ColumnId.Phase => new Label(),
                    ColumnId.Reaction => new Label(),
                    ColumnId.Trigger => new Label(),
                    _ => throw new NotImplementedException(),
                };
            }

            protected override int GetLocalRowCount()
            {
                return ParentRecord.ChildCount;
            }
            protected override TableView.ITableData MakeExpand(int localRowIndex)
            {
                var record = ParentRecord.GetChildAt(localRowIndex);
                return new EventChildrenTableData(record);
            }

            public override void BindFoldoutAt(int columnIndex, int localRowIndex, FoldoutHead foldout)
            {
                var record = ParentRecord.GetChildAt(localRowIndex);
                foldout.LbText.text = record.DisplayName;
                foldout.SetCount(record.ChildCount);
            }

            public override void BindItemAt(int columnIndex, int localRowIndex, bool expanded, int depth, VisualElement ve)
            {
                var record = ParentRecord.GetChildAt(localRowIndex);
                switch ((ColumnId)columnIndex)
                {
                    case ColumnId.TimeStamp:
                        (ve as Label).text = record.TimeStamp.Frame.ToString();
                        break;
                    case ColumnId.Hash:
                        (ve as Label).text = record.Id.ToString();
                        break;
                    case ColumnId.Foldout:
                        base.BindItemAt(columnIndex, localRowIndex, expanded, depth, ve);
                        break;
                    case ColumnId.GameObject:
                        (ve as Label).text = record.Owner.GameObjectName;
                        break;
                    case ColumnId.Component:
                        (ve as Label).text = record.Owner.ComponentName;
                        break;
                    case ColumnId.Phase:
                        (ve as Label).text = record.PhaseDisplayName;
                        break;
                    case ColumnId.Reaction:
                        if (record is ReactionRecord rr)
                            (ve as Label).text = rr.ReactionName;
                        else
                            (ve as Label).text = "";
                        break;
                    case ColumnId.Trigger:
                        (ve as Label).text = record.Parameters.Current.TriggerObject.GetNameOrNull();
                        break;
                }
            }
        }


    }

}