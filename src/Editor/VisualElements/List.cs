//#define NIEDITOR_PRINT
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using NiEngine;

namespace NiEditor
{


    public class ListView2 : EditorFieldVe
    {
        [System.Diagnostics.Conditional("NIEDITOR_PRINT")]
        void Log(string msg)
        {
            Debug.Log(msg);
        }
        public static VisualTreeAsset _ListAsset;
        public static VisualTreeAsset _ListFoldAsset;
        public static VisualTreeAsset _ListItemAsset;
        public static VisualTreeAsset ListAsset => _ListAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/List.uxml");
        public static VisualTreeAsset ListFoldAsset => _ListFoldAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/ListFold.uxml");
        public static VisualTreeAsset ListItemAsset => _ListItemAsset ??= AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/NiEngine/src/Editor/Assets/ListItem.uxml");

        //public new class UxmlFactory : UxmlFactory<ListView2, UxmlTraits> { }

        //// Add the two custom UXML attributes.
        //public new class UxmlTraits : VisualElement.UxmlTraits
        //{
        //    UxmlStringAttributeDescription m_String =
        //        new UxmlStringAttributeDescription { name = "my-string", defaultValue = "default_value" };

        //    public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        //    {
        //        base.Init(ve, bag, cc);
        //        var ate = ve as List;

        //        ate.myString = m_String.GetValueFromBag(bag, cc);
        //    }
        //}

        //// Must expose your element class to a { get; set; } property that has the same name 
        //// as the name you set in your UXML attribute description with the camel case format
        //public string myString { get; set; }

        //SerializedProperty Property;
        SerializedProperty ExpandedProperty;
        VisualElement VeContent;
        VisualElement VeFold;
        VisualElement VeFoldVeContent;
        Toggle Toggle;
        Button BtAdd;
        Label LbText;
        Label LbCount;
        EditorField EditorField;
        public bool Track = true;
        public class Item
        {
            public ListView2 List;
            public SerializedProperty ParentProperty;
            public SerializedProperty Property;
            public int Index;
            public VisualElement RootVisualElement;

            public VisualElement VeContent;
            public NiPropertyField PfContent;

            public Item(ListView2 list, SerializedProperty parentProperty, int index)
            {
                List = list;
                ParentProperty = parentProperty;
                Index = index;
                VeContent = default;
                Build();
            }
            void Build()
            {
                RootVisualElement = ListItemAsset.CloneTree();
                var tfItemIndex = RootVisualElement.Query<TextField>("tfIndex").First();
                var btItemDelete = RootVisualElement.Query<Button>("btDelete").First();
                var veItemContent = RootVisualElement.Query<VisualElement>("veContent").First();
                tfItemIndex.RegisterCallback<KeyDownEvent>(x =>
                {
                    if (x.keyCode != KeyCode.Return && x.keyCode != KeyCode.KeypadEnter) return;
                    if (!int.TryParse(tfItemIndex.value, out var newIndex)) return;
                    newIndex = Math.Clamp(newIndex, 0, ParentProperty.arraySize - 1);
                    if (newIndex == Index) return;
                    List.MoveItem(Index, newIndex);
                });
                btItemDelete.RegisterCallback<ClickEvent>(x => List.DeleteItemAt(Index));
                VeContent = List.CreateItem?.Invoke();
                if (VeContent == null)
                {
                    var ef = new NiEngine.EditorField(showPrefixLabel: false, List.EditorField?.Inline ?? false);
                    PfContent = new NiPropertyField(ef);
                    PfContent.RegisterValueChangeCallback(x=>List.m_OnChangeCallback?.Invoke(x));
                    VeContent = PfContent;
                }
                veItemContent.Add(VeContent);
            }
            //public void Delete()
            //{
            //    ParentProperty.DeleteArrayElementAtIndex(Index);
            //    ParentProperty.serializedObject.ApplyModifiedProperties();
            //    List.RefreshItemsAfter(Index);
            //}
            public void RemoveFromParent()
            {
                RootVisualElement.parent.Remove(RootVisualElement);
            }
            public void MoveToIndex(int index)
            {
                var parent = RootVisualElement.parent;
                parent.Remove(RootVisualElement);
                Index = index;
                parent.Insert(Index, RootVisualElement);
            }
            public void BindProperty(SerializedProperty property, int index, bool forced = false)
            {

                Index = index;
                var tfItemIndex = RootVisualElement.Query<TextField>("tfIndex").First();
                tfItemIndex.value = index.ToString();

                if((forced && Property != property) || Property == null || property.propertyPath != Property.propertyPath)
                {
                    PfContent?.BindProperty(property);
                    List.BindItem?.Invoke(VeContent, property, index);
                    Property = property;
                }
                RootVisualElement.style.backgroundColor = (index % 2) == 0 ? new Color(0, 0, 0, 0) : new Color(1, 1, 1, 0.02f);
            }
        }


        List<Item> Items = new();

        public bool UseScollView;
        //public Action<SerializedProperty> OnNewItem;
        public Func<VisualElement> CreateItem;
        public Action<VisualElement, SerializedProperty, int> BindItem;

        EventCallback<SerializedPropertyChangeEvent> m_OnChangeCallback;
        public void RegisterValueChangeCallback(EventCallback<SerializedPropertyChangeEvent> ecb)
        {
            m_OnChangeCallback = ecb;
        }
        public bool Header { get; private set; }
        public bool HasRawItems { get; private set; }
        public bool HasMaxHeight { get; private set; }
        public ListView2(bool header=true, bool rawItems=false, bool useScollView = false)
        {
            Header = header;
            HasRawItems = rawItems;
            UseScollView = useScollView;
            Build();
        }
        public ListView2(EditorField ef, bool rawItems=false)
        {
            Header = ef?.Header ?? true;
            HasRawItems = rawItems;
            UseScollView = EditorField?.ScrollView ?? false;
            EditorField = ef;
            Build();
        }
        public void DeleteItemAt(int index)
        {
            Property.DeleteArrayElementAtIndex(index);
            Property.serializedObject.ApplyModifiedProperties();
            RefreshItemsAfter(index, true);
            m_OnChangeCallback?.Invoke(SerializedPropertyChangeEvent.GetPooled(Property));
        }
        public void ShowIcon(bool visible)
        {
            if (!Header) return;
            this.Query<VisualElement>("veIcon").First().style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        public void SetIcon(Texture2D icon)
        {
            if (!Header) return;
            this.Query<VisualElement>("veIcon").First().style.backgroundImage = icon;
        }
        public void SetColor(StyleColor color)
        {
            this.Query<VisualElement>("veColor").ForEach(x => x.style.backgroundColor = color);
        }
        public void SetText(string value)
        {
            if (!Header) return;
            LbText.text = value;
        }
        public void SetAddButtonText(string value)
        {
            BtAdd.text = value;
        }
        public void SetMaxNone()
        {
            if (HasMaxHeight)
            {
                VeFoldVeContent.style.maxHeight = StyleKeyword.None;
                var scroll = VeFoldVeContent[0] as ScrollView;
                VeFoldVeContent.RemoveAll();
                VeFoldVeContent.TransferAllFrom(scroll.contentContainer);
                HasMaxHeight = false;
            }
        }
        public void SetMaxHeight(float maxHeight)
        {
            VeFoldVeContent.style.maxHeight = maxHeight;
            //VeFoldVeContent.style.height = maxHeight;
            if (!HasMaxHeight)
            {
                var scroll = new ScrollView(ScrollViewMode.Vertical);
                //scroll.style.height = maxHeight;
                //scroll.contentContainer.style.maxHeight = maxHeight;
                //scroll.contentContainer.style.minHeight = StyleKeyword.Auto;
                //scroll.contentContainer.style.height = StyleKeyword.Auto;
                scroll.contentContainer.TransferAllFrom(VeFoldVeContent);
                VeContent = scroll.contentContainer;
                VeFoldVeContent.Add(scroll);
                HasMaxHeight = true;
            }
        }
        void Build()
        {
            if (Header)
            {
                ListAsset.CloneTree(this);
                Toggle = this.Query<Toggle>().First();
                LbCount = this.Query<Label>("lbCount").First();
                LbText = this.Query<Label>("lbText").First();
                VeFold = this.Query<VisualElement>("veFold").First();
                Toggle.RegisterCallback<ChangeEvent<bool>>(x => SetFold(x.newValue));
            }
            else
            {
                ListFoldAsset.CloneTree(this);
            }
            var contentRoot = this.Query<VisualElement>("veListContent").First();
            if (UseScollView)
            {
                var sw = new ScrollView();
                contentRoot.Add(sw);
                VeFoldVeContent = sw.contentContainer;
            }
            else
            {
                VeFoldVeContent = contentRoot;
            }
            VeContent = VeFoldVeContent;
            BtAdd = this.Query<Button>("btAdd").First();
            BtAdd.RegisterCallback<ClickEvent>(x => AddToEnd());

        }
        public void SetFold(bool value)
        {
            if (!Header) return;
            if(VeContent.childCount == 0)
                RefreshItemsAfter(0);
            VeFold.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
            if(ExpandedProperty != null)
                ExpandedProperty.boolValue = value;
            Property.isExpanded = value;
            Property.serializedObject.ApplyModifiedProperties();
        }

        //void RemoveAt(VisualElement veItem, int index)
        //{

        //    Property.DeleteArrayElementAtIndex(index);
        //    Property.serializedObject.ApplyModifiedProperties();


        //    VeContent.Remove(veItem);
        //}
        public void MoveItem(int fromIndex, int toIndex)
        {
            var item = Items[fromIndex];
            Items.RemoveAt(fromIndex);
            item.MoveToIndex(toIndex);
            Items.Insert(toIndex, item);

            Property.MoveArrayElement(fromIndex, toIndex);
            Property.serializedObject.ApplyModifiedProperties();

            RefreshItemsAfter(Math.Min(fromIndex, toIndex), forced:true);
        }
        void AddToEnd()
        {

            int index = Property.arraySize - 1;
            //Debug.Log($"AddToEnd Property.arrayElementType = {Property.arrayElementType}");
            //var eT = NiEngine.IO.StreamContext.StringToType(Property.arrayElementType);
            //Debug.Log($"AddToEnd eT= {eT?.FullName}");
            //if(eT != null && typeof(NiReference).IsAssignableFrom(eT))
            {
                var (fi, obj) = Property.FindFieldInfo();
                //Debug.Log($"AddToEnd fi.FieldType = {fi.FieldType.GetGenericTypeDefinition().FullName}");
                if (fi.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    //Debug.Log($"AddToEnd fi.FieldType == typeof(List<>)");
                    var eT = fi.FieldType.GenericTypeArguments.FirstOrDefault();
                    //Debug.Log($"AddToEnd eT = {eT.FullName}");

                    if (eT is not null )//&& typeof(NiReference).IsAssignableFrom(eT))
                    {
                        //Debug.Log($"AddToEnd eT is NiReference");
                        if (obj is IList list)
                        {
                            if (eT.IsClass || eT.IsInterface)
                            {
                                list.Add(null);
                                //Debug.Log($"AddToEnd Add Null");
                            }
                            else 
                            {
                                
                                var o = Activator.CreateInstance(eT);
                                list.Add(o);
                                //Debug.Log($"AddToEnd Add Default");
                            }
                            Property.serializedObject.ApplyModifiedProperties();
                            Property.serializedObject.Update();
                            if (!Track)
                                Refresh();
                            return;
                        }
                    }
                }
            }
            Property.InsertArrayElementAtIndex(Property.arraySize);
            //int index = Property.arraySize - 1;
            //var element = Property.GetArrayElementAtIndex(index);
            //var itor = element.Copy();
            //int d = itor.depth;
            //while (itor.NextVisible(true) && itor.depth > d)
            //{
            //    //Debug.Log($"Reset Property '{itor.propertyPath}' propertyType:{itor.propertyType}");
            //    itor.isExpanded = false;
            //    switch (itor.propertyType)
            //    {
            //        case SerializedPropertyType.Generic:
            //            if (itor.isArray)
            //            {
            //                itor.arraySize = 0;
            //                //for (int i = itor.arraySize-1; i >= 0; i--)
            //                //    itor.DeleteArrayElementAtIndex(i);
            //            }

            //            break;
            //        case SerializedPropertyType.ObjectReference:
            //            itor.objectReferenceValue = null;
            //            break;
            //        case SerializedPropertyType.ManagedReference:
            //            itor.managedReferenceValue = null;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Integer:
            //            itor.intValue = 0;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Boolean:
            //            itor.boolValue = false;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Float:
            //            itor.floatValue = 0;
            //            break;
            //        case UnityEditor.SerializedPropertyType.String:
            //            itor.stringValue = null;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Color:
            //            itor.colorValue = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.LayerMask:
            //            itor.intValue = 0;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Enum:
            //            itor.enumValueIndex = 0;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Vector2:
            //            itor.vector2Value = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Vector3:
            //            itor.vector3Value = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Vector4:
            //            itor.vector4Value = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Rect:
            //            itor.rectValue = default;
            //            break;
            //        //case UnityEditor.SerializedPropertyType.ArraySize:
            //        //    itor.intValue = 0;
            //        //    break;
            //        case UnityEditor.SerializedPropertyType.Character:
            //            itor.objectReferenceValue = null;
            //            break;
            //        case UnityEditor.SerializedPropertyType.AnimationCurve:
            //            itor.animationCurveValue = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Bounds:
            //            itor.boundsValue = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Gradient:
            //            itor.gradientValue = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Quaternion:
            //            itor.quaternionValue = Quaternion.identity;
            //            break;
            //        case UnityEditor.SerializedPropertyType.ExposedReference:
            //            itor.exposedReferenceValue = null;
            //            break;
            //        //case UnityEditor.SerializedPropertyType.FixedBufferSize:
            //        //    break;
            //        case UnityEditor.SerializedPropertyType.Vector2Int:
            //            itor.vector2IntValue = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Vector3Int:
            //            itor.vector3IntValue = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.RectInt:
            //            itor.rectIntValue = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.BoundsInt:
            //            itor.boundsIntValue = default;
            //            break;
            //        case UnityEditor.SerializedPropertyType.Hash128:
            //            itor.hash128Value = default;
            //            break;
            //        default:
            //            break;
            //    }
            //}


            //if (OnNewItem != null)
            //    OnNewItem(element);
            Property.serializedObject.ApplyModifiedProperties();
            //RefreshItemsAfter(index);
            //Refresh();
        }
        void AddItem(SerializedProperty property)
        {
            if (HasRawItems)
            {
                if (CreateItem != null)
                {
                    var item = CreateItem.Invoke();
                    BindItem.Invoke(item, property, VeContent.childCount);
                    VeContent.Add(item);
                }
                else
                {
                    var item = new NiPropertyField();
                    item.BindProperty(property);
                    VeContent.Add(item);
                }
            }
            else
            {
                var item = new Item(this, Property, Items.Count);
                item.BindProperty(property, Items.Count);
                VeContent.Add(item.RootVisualElement);
                Items.Add(item);
            }
            m_OnChangeCallback?.Invoke(SerializedPropertyChangeEvent.GetPooled(Property));
        }
        void BindItemAt(int index, bool forced)
        {

            if (HasRawItems)
            {
                if (CreateItem != null && BindItem != null)
                    BindItem.Invoke(VeContent[index], Property.GetArrayElementAtIndex(index), index);
                else
                    (VeContent[index] as NiPropertyField)?.BindProperty(Property.GetArrayElementAtIndex(index));
            }
            else
            {
                Items[index].BindProperty(Property.GetArrayElementAtIndex(index), index, forced);

            }
        }
        public void RefreshItemsAfter(int index, bool forced = false)
        {
            //Log($"ListView2 Property:{Property.propertyPath} RefreshItemsAfter({index}) ");
            int i = index;

            for (; i != VeContent.childCount; ++i)
            {
                if (i < Property.arraySize)
                {
                    Log($"[ListView2.BindItem] Row:{i}\nProperty:{Property.propertyPath}");
                    BindItemAt(i, forced);
                }
                else
                {
                    // delete the remaining of items
                    if (HasRawItems)
                        while (VeContent.childCount > i)
                            VeContent[VeContent.childCount - 1].RemoveFromHierarchy();
                    else
                    {
                        for (int iRemove = Items.Count - 1; iRemove >= i; --iRemove)
                            Items[iRemove].RemoveFromParent();
                        Items.RemoveRange(i, Items.Count - i);
                    }
                    
                    return;
                }
            }
            for (; i < Property.arraySize; ++i)
            {
                Log($"[ListView2.AddItem] Row:{i}\nProperty:{Property.propertyPath}");
                AddItem(Property.GetArrayElementAtIndex(i));
            }
        }
        void Refresh()
        {
            bool isExpanded = ExpandedProperty?.boolValue ?? Property.isExpanded;
            if (Header)
            {
                Toggle.value = isExpanded;
                VeFold.style.display = isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
                LbCount.text = Property.arraySize.ToString();
                LbText.tooltip = Property.tooltip;
            }
            //VeContent.RemoveAll();
            if(isExpanded || !Header)
                RefreshItemsAfter(0);
            //for (int i = 0; i != Property.arraySize; ++i)
            //{
            //    var e = Property.GetArrayElementAtIndex(i);
            //    var item = new Item(this, Property, i);
            //    item.BindProperty(e, i);
            //    VeContent.Add(item.RootVisualElement);
            //    Items.Add(item);
            //}
        }
        bool m_IgnorePropertyChange=false;
        void OnListChange(SerializedProperty property)
        {
            //Debug.Log($"[{Time.frameCount}] Refresh List {property.arraySize}");
            //Property = property;

            //this.Unbind();
            //this.TrackPropertyValue(property, OnListChange);
            Refresh();
        }
        public new bool BindProperty(SerializedProperty property, SerializedProperty expanded = null)
        {
            if(Track)//if (EditorField?.Track ?? false)
            {
                this.TrackPropertyValue(property, OnListChange);
            }
            if (!base.BindProperty(property)) return false;
            
            Log($"[ListView2.BindProperty]\nProperty:{property.propertyPath}");
            Property = property;
            ExpandedProperty = expanded;
            Refresh();
            //VeContent.TrackPropertyValue(Property, x => 
            //{
            //    Property = x;
            //    Refresh();
            //    //LbCount.text = property.arraySize.ToString();
            //    //VeContent.Clear();
            //    //for (int i = 0; i != Property.arraySize; ++i)
            //    //{
            //    //    var e = Property.GetArrayElementAtIndex(i);
            //    //    var pf = new PropertyField();
            //    //    pf.BindProperty(e);
            //    //    VeContent.Add(pf);
            //    //}
            //});
            return true;
        }
    }



    //public abstract class TreeViewItem : VisualElement
    //{
    //    public abstract void Expand(bool value);
    //}

    //public class TreeView : VisualElement
    //{
    //    public Func<TreeViewItem> MakeItem;

    //    //public Action<VisualElement, int> BindItem;
    //    //public Action<VisualElement, int, bool> ExpandItem;




    //}
}