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

    public abstract class FoldoutHeadTableData : TableView.ExpandableTableData
    {
        public override VisualElement MakeItem(int columnIndex) => new FoldoutHead();
        public abstract void BindFoldoutAt(int columnIndex, int rowLocalIndex, FoldoutHead foldout);
        public override void BindItemAt(int columnIndex, int rowLocalIndex, bool expanded, int depth, VisualElement ve)
        {
            if (ve is FoldoutHead foldout)
            {
                foldout.TgExpand.value = expanded;
                BindFoldoutAt(columnIndex, rowLocalIndex, foldout);
                foldout.UnregisterAllExpandChangedCallback();
                foldout.RegisterExpandChangedCallback(x =>
                {
                    SetExpandAt(rowLocalIndex, x.newValue);
                });
            }
        }
        public override void UnbindItemAt(int columnIndex, int rowIndex, VisualElement ve)
        {
            if (ve is FoldoutHead item)
            {
                item.UnregisterAllExpandChangedCallback();
            }
        }
    }

    public class TableView : VisualElement
    {
        static VisualTreeAsset _TableViewAsset;
        public static VisualTreeAsset TableViewAsset => _TableViewAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/TableView.uxml");

        public struct RowRef
        {
            public ITableData Data;
            public int RowIndex;
            public int Depth;
            public RowRef(ITableData data, int rowIndex, int depth)
            {
                Data = data;
                RowIndex = rowIndex;
                Depth = depth;
            }
        }

        public interface IHeader
        {
            VisualElement MakeItem(int columnIndex);
            float GetPreferredWidth(int columnIndex);
            int GetColumnCount();
        }

        public class HeaderLabel : IHeader
        {
            public string[] Text;
            public HeaderLabel(params string[] text)
            {
                Text = text;
            }
            public VisualElement MakeItem(int columnIndex) => new Label(Text[columnIndex]);
            public float GetPreferredWidth(int columnIndex) => RectLayout.WidthOf(Text[columnIndex]);
            public int GetColumnCount() => Text.Length;
        }

        public interface ITableData
        {
            VisualElement MakeItem(int columnIndex);
            void BindItemAt(int columnIndex, int rowIndex, VisualElement ve);
            void UnbindItemAt(int columnIndex, int rowIndex, VisualElement ve);
            int GetColumnCount();
            int GetRowCount();

            /// <summary>
            /// Column, rowCountBefor, rowCountAfter
            /// </summary>
            /// <param name="onRowCountChange"></param>
            void AddCallbackOnRowCountChange(Action<ITableData, int, int> onRowCountChange);
            void RemoveCallbackOnRowCountChange(Action<ITableData, int, int> onRowCountChange);

        }

        public interface IComplexTableData : ITableData
        {
            public RowRef ResolveCell(int globalRowIndex, int columnIndex);
        }

        public class JoinedTableData : IComplexTableData
        {
            List<ITableData> Datas = new();
            Action<ITableData, int, int> WhenRowCountChange;
            int TotalRowCount;
            public JoinedTableData()
            {
            }
            public JoinedTableData(params ITableData[] datas)
            {
                Datas.AddRange(datas);
                foreach(var data in datas)
                {
                    TotalRowCount += data.GetRowCount();
                    data.AddCallbackOnRowCountChange(OnChildRowCountChanged);
                }
            }

            public void Add(ITableData data)
            {
                Datas.Add(data);
                data.AddCallbackOnRowCountChange(OnChildRowCountChanged);
                OffsetRowCount(data.GetRowCount());
            }

            public void Remove(ITableData data)
            {
                Datas.Remove(data);
                data.RemoveCallbackOnRowCountChange(OnChildRowCountChanged);
                OffsetRowCount(-data.GetRowCount());
            }

            public void OnChildRowCountChanged(ITableData data, int oldCount, int newCount)
            {
                OffsetRowCount(newCount - oldCount);
            }

            void OffsetRowCount(int count)
            {
                if (count == 0) return;
                var oldTotalRowCount = TotalRowCount;
                TotalRowCount += count;
                WhenRowCountChange?.Invoke(this, oldTotalRowCount, TotalRowCount);
            }

            public RowRef ResolveCell(int globalRowIndex, int columnIndex)
            {
                var currentRowIndex = 0;
                foreach(var d in Datas)
                {
                    var count = d.GetRowCount();
                    if (currentRowIndex + count > globalRowIndex)
                    {
                        var rr = new RowRef(d, globalRowIndex - currentRowIndex, 0);
                        //if (columnIndex == 0)
                        //    Debug.Log($"[{GetHashCode()}][{GetType().Name}]:{globalRowIndex} => [{rr.GetHashCode()}][{rr.Data.GetType().FullName}]:{rr.RowIndex}");
                        return rr;
                    }
                    currentRowIndex += count;
                }
                //if (columnIndex == 0)
                //    Debug.Log($"[{GetHashCode()}][{nameof(JoinedTableData)}]:{globalRowIndex} => : out of range");
                return default;
            }

            public VisualElement MakeItem(int columnIndex)
            {
                return Datas[0].MakeItem(columnIndex);
            }
            public void BindItemAt(int columnIndex, int globalRowIndex, VisualElement ve)
            {
                var target = ResolveCell(globalRowIndex, columnIndex);

                if (target.Data != null)
                    target.Data.BindItemAt(columnIndex, target.RowIndex, ve);
            }
            public void UnbindItemAt(int columnIndex, int rowIndex, VisualElement ve)
            {
                var target = ResolveCell(rowIndex, columnIndex);
                if (target.Data != null)
                    target.Data.UnbindItemAt(columnIndex, target.RowIndex, ve);
            }
            public int GetColumnCount()
            {
                return Datas.First().GetColumnCount();
            }
            public int GetRowCount()
            {
                return TotalRowCount;
            }
            public void AddCallbackOnRowCountChange(Action<ITableData, int, int> onRowCountChange)
            {
                WhenRowCountChange += onRowCountChange;
            }
            public void RemoveCallbackOnRowCountChange(Action<ITableData, int, int> onRowCountChange)
            {
                WhenRowCountChange -= onRowCountChange;
            }
        }

        public abstract class ExpandableTableData : IComplexTableData
        {
            public class Node
            {
                public int Index;
                public bool IsExpanded;
                public ITableData SubData;
            }

            /// <summary>
            /// (int localRowIndex, Node node)
            /// </summary>
            SortedDictionary<int, Node> ExpandedRows = new();
            int RowCountWithExpand = 0;
            int LastLocalRowCount = -1;

            int LastResolvedGlobalIndex = -1;
            RowRef LastResolvedRowRef;

            protected Action<ITableData, int, int> WhenRowCountChange;

            public virtual float GetColumnIndent(int columnIndex, int depth) => depth * 12.0f;
            public abstract VisualElement MakeItem(int columnIndex);
            public abstract void BindItemAt(int columnIndex, int rowLocalIndex, bool expanded, int depth, VisualElement ve);
            void ITableData.BindItemAt(int columnIndex, int rowIndex, VisualElement ve)
            {
                var rr = ResolveCell(rowIndex, columnIndex);
                //if (columnIndex == 0)
                //    Debug.Log($"[{GetType().FullName}]:{rowIndex} => [{rr.Data.GetType().FullName}]:{rr.RowIndex}");
                if (rr.Data == this)
                    BindItemAt(columnIndex, rr.RowIndex, IsExpanded(rr.RowIndex), rr.Depth, ve);
                else
                    rr.Data.BindItemAt(columnIndex, rr.RowIndex, ve);

                var indent = GetColumnIndent(columnIndex, rr.Depth);
                ve.style.left = ve.style.left.value.value + indent;
                ve.style.width = ve.style.width.value.value - indent;
            }

            public abstract void UnbindItemAt(int columnIndex, int rowIndex, VisualElement ve);
            public abstract int GetColumnCount();
            public void AddCallbackOnRowCountChange(Action<ITableData, int, int> onRowCountChange)
            {
                WhenRowCountChange += onRowCountChange;
            }
            public void RemoveCallbackOnRowCountChange(Action<ITableData, int, int> onRowCountChange)
            {
                WhenRowCountChange -= onRowCountChange;
            }

            void OnChildRowCountChanged(ITableData column, int oldRowCount, int newRowCount)
            {
                LastResolvedGlobalIndex = -1;
                var rowCountBefore = RowCountWithExpand;
                var count = newRowCount - oldRowCount;
                RowCountWithExpand += count;
                if (rowCountBefore != RowCountWithExpand)
                    WhenRowCountChange?.Invoke(this, rowCountBefore, RowCountWithExpand);
            }

            void ComputeRowCount()
            {
                var total = GetLocalRowCount();
                foreach (var n in ExpandedRows.Values)
                    if (n.IsExpanded)
                        total += n.SubData.GetRowCount();
                RowCountWithExpand = total;
                LastResolvedGlobalIndex = -1;
            }

            public int GetRowCount()
            {
                var localRowCount = GetLocalRowCount();
                if (LastLocalRowCount != localRowCount)
                {
                    LastLocalRowCount = localRowCount;
                    ComputeRowCount();
                }
                return RowCountWithExpand;
            }

            public bool IsExpanded(int localRowIndex)
            {
                if (ExpandedRows.TryGetValue(localRowIndex, out var n))
                    return n.IsExpanded;
                return false;
            }

            public void SetExpandAt(int localRowIndex, bool expand)
            {
                int rowCountBefore = RowCountWithExpand;
                if (!ExpandedRows.TryGetValue(localRowIndex, out Node node))
                {
                    if (!expand)
                        return;
                    node = new Node();
                    node.Index = localRowIndex;
                    node.IsExpanded = false;
                    node.SubData = null;
                    ExpandedRows.Add(localRowIndex, node);
                }

                if (expand && !node.IsExpanded)
                {
                    node.IsExpanded = true;
                    if (node.SubData == null)
                    {
                        node.SubData = MakeExpand(localRowIndex);
                    }
                    RowCountWithExpand += node.SubData.GetRowCount();
                    node.SubData.AddCallbackOnRowCountChange(OnChildRowCountChanged);
                }
                else if (!expand && node.IsExpanded)
                {
                    node.IsExpanded = false;
                    RowCountWithExpand -= node.SubData.GetRowCount();
                    node.SubData.RemoveCallbackOnRowCountChange(OnChildRowCountChanged);
                }

                if (RowCountWithExpand != rowCountBefore)
                {
                    LastResolvedGlobalIndex = -1;
                    WhenRowCountChange?.Invoke(this, rowCountBefore, RowCountWithExpand);
                }
            }

            public RowRef ResolveCell(int globalRowIndex, int columnIndex)
            {
                if (LastResolvedGlobalIndex == globalRowIndex)
                    return LastResolvedRowRef;

                int currentGlobalRow = 0;
                int lastNodeRow = 0;
                int totalSubRowCount = 0;
                foreach (var (index, node) in ExpandedRows)
                {
                    // count number of rows between this node and the last node. All these rows are not expanded
                    var nonExpandedCount = index - lastNodeRow;
                    if (currentGlobalRow + nonExpandedCount >= globalRowIndex)
                    {
                        break;
                        //// Target is in the non expanded ones or the current node index.
                        //return new RowRef(this, globalRowIndex - totalSubRowCount);
                    }
                    // Target is not in the nonExpandedCount, add them to the current count. + 1 to skip the current node row
                    currentGlobalRow += nonExpandedCount + 1;
                    
                    if (node.IsExpanded)
                    {
                        var subRowCount = node.SubData.GetRowCount();
                        //if (currentGlobalRow + subRowCount >= globalRowIndex)
                        if(globalRowIndex < currentGlobalRow + subRowCount)
                        {
                            // Target is in this node sub data
                            var r = new RowRef(node.SubData, globalRowIndex - currentGlobalRow, 1);

                            //if (columnIndex == 0)
                            //    Debug.Log($"[{GetHashCode()}][{GetType().Name}]:{globalRowIndex} => [{r.GetHashCode()}][{r.Data.GetType().Name}]:{r.RowIndex}");
                            //if (r.Data is IComplexTableData complex)
                            //{
                            //    r = complex.ResolveCell(r.RowIndex, columnIndex);
                            //    r.Depth += 1;
                            //}
                            LastResolvedGlobalIndex = globalRowIndex;
                            LastResolvedRowRef = r;
                            return r;
                        }
                        // Target is not in this node sub column
                        // add the number of row in the expanded sub column
                        currentGlobalRow += subRowCount;
                        totalSubRowCount += subRowCount;
                    }
                    lastNodeRow = index + 1;

                }
                // Target is not in any expanded node.
                var result = new RowRef(this, globalRowIndex - totalSubRowCount, 0);
                //if (columnIndex == 0)
                //    Debug.Log($"[{GetHashCode()}][{GetType().Name}]:{globalRowIndex} => :{result.RowIndex}");
                // Target is not in any expanded node.
                return result;
            }

            protected abstract int GetLocalRowCount();
            protected abstract ITableData MakeExpand(int localRowIndex);
        }

        public float RowHeight = 18;
        public Vector2 ScrollWheelSpeed = new(32, 18);
        public Vector2 ScrollTopLeft => new Vector2(ScH.value, ScV.value);
        public IHeader Header;
        public ITableData Data;

        Scroller ScV;
        Scroller ScH;
        VisualElement VeHeader;
        VisualElement VeContent;
        float FullHeight;
        float FullWidth;
        bool Built = false;
        struct PooledVisualElement
        {
            public VisualElement VisualElement;
            public int ColumnIndex;
            public int RowIndex;
        }
        List<List<PooledVisualElement>> VisualElementPool = new();

        PooledVisualElement GetNextPooledVisualElement(int colIndex)
        {
            if (VisualElementPool[colIndex].Count > 0)
            {
                var ve = VisualElementPool[colIndex][0];
                VisualElementPool[colIndex].RemoveAt(0);
                return ve;
            }
            var newPve = new PooledVisualElement
            {
                VisualElement = Data.MakeItem(colIndex),
                ColumnIndex = 0,
                RowIndex = 0,
            };
            VeContent.Add(newPve.VisualElement);
            return newPve;
        }

        public void RefreshViewArea()
        {
            //Debug.Log($"Table Refresh");
            var pos = ScrollTopLeft;
            var rowAtPos = pos.y / RowHeight;
            int row = (int)rowAtPos;
            var visualY = -(pos.y % RowHeight);
            var rowCount = Data.GetRowCount();
            var colCount = Data.GetColumnCount();
            List<List<PooledVisualElement>> nextVisualElementPool = new();
            for (int i = 0; i != colCount; i++)
                nextVisualElementPool.Add(new());
            while (visualY < VeContent.resolvedStyle.height && row < rowCount)
            {
                var visualX = -pos.x;
                for (int col = 0; col < colCount; col++)
                {
                    var ve = GetNextPooledVisualElement(col);
                    ve.ColumnIndex = col;
                    ve.RowIndex = row;
                    nextVisualElementPool[col].Add(ve);
                    var w = Header.GetPreferredWidth(col);
                    var h = RowHeight;
                    ve.VisualElement.style.position = Position.Absolute;
                    ve.VisualElement.style.left = visualX;
                    ve.VisualElement.style.top = visualY;
                    ve.VisualElement.style.width = w;
                    ve.VisualElement.style.height = h;
                    ve.VisualElement.style.maxWidth = w;
                    ve.VisualElement.style.maxHeight = h;
                    ve.VisualElement.style.overflow = Overflow.Hidden;
                    Data.BindItemAt(col, row, ve.VisualElement);
                    visualX += w;

                }
                visualY += RowHeight;
                ++row;
            }
            foreach (var pve in VisualElementPool)
                foreach (var ve in pve)
                {
                    Data.UnbindItemAt(ve.ColumnIndex, ve.RowIndex, ve.VisualElement);
                    VeContent.Remove(ve.VisualElement);
                }
            VisualElementPool = nextVisualElementPool;
            FullHeight = Data.GetRowCount() * RowHeight;
            UpdateScrollers(VeContent.resolvedStyle.width, VeContent.resolvedStyle.height);
        }

        void UpdateScrollers(float w, float h)
        {
            ScV.lowValue = 0;
            ScV.highValue = Mathf.Max(FullHeight - h, 0);
            ScH.lowValue = 0;
            ScH.highValue = Mathf.Max(FullWidth - w, 0);
        }

        public void Build()
        {
            if (Built)
            {
                RefreshViewArea();
                return;
            }
            Clear();

            style.flexGrow = 1;
            TableViewAsset.CloneTree(this);
            ScV = this.Query<Scroller>("scV");
            ScH = this.Query<Scroller>("scH");
            VeHeader = this.Query<VisualElement>("veHeader");
            VeContent = this.Query<VisualElement>("veContent");
            ScV.RegisterCallback<ChangeEvent<float>>(x => RefreshViewArea());
            ScH.RegisterCallback<ChangeEvent<float>>(xx => RefreshViewArea());
            VeContent.RegisterCallback<GeometryChangedEvent>(x =>
            {
                UpdateScrollers(x.newRect.width, x.newRect.height);
                RefreshViewArea();
            });
            VeContent.RegisterCallback<WheelEvent>(x =>
            {
                ScH.value += x.delta.x * ScrollWheelSpeed.x;
                ScV.value += x.delta.y * ScrollWheelSpeed.y;
            });

            FullWidth = 0;
            for (int i = 0; i != Header.GetColumnCount(); i++)
            {
                //Add Header;
                var veHeader = Header.MakeItem(i);
                var w = Header.GetPreferredWidth(i);
                veHeader.style.width = w;
                veHeader.style.minWidth = w;
                veHeader.style.maxWidth = w;
                VeHeader.Add(veHeader);
                FullWidth += w;
                VisualElementPool.Add(new());
            }
            FullHeight = Data.GetRowCount() * RowHeight;
            RefreshViewArea();
            Built = true;
        }
    }
}