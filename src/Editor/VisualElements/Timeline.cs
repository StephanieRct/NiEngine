using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace NiEditor
{
    public class Timeline : VisualElement
    {
        public struct RowRef
        {
            public ITimelineData Data;
            public int RowIndex;
            public int Depth;
            public RowRef(ITimelineData data, int rowIndex, int depth)
            {
                Data = data;
                RowIndex = rowIndex;
                Depth = depth;
            }
        }
        public interface ITimelineData
        {
            VisualElement MakeItem(int columnIndex);
            void BindItemAt(RangeInt frames, int rowIndex, VisualElement ve);
            void UnbindItemAt(RangeInt frames, int rowIndex, VisualElement ve);
            int GetColumnCount();
            int GetRowCount();

            /// <summary>
            /// Column, rowCountBefor, rowCountAfter
            /// </summary>
            /// <param name="onRowCountChange"></param>
            void AddCallbackOnRowCountChange(Action<ITimelineData, int, int> onRowCountChange);
            void RemoveCallbackOnRowCountChange(Action<ITimelineData, int, int> onRowCountChange);

        }
        public interface IComplexTableData : ITimelineData
        {
            public RowRef ResolveCell(int globalRowIndex, int columnIndex);
        }
    }
}