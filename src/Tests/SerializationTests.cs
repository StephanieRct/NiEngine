using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using NiEngine.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Data;
using System.Drawing;

namespace NiEngine.Tests
{

    //public class DataNode
    //{
    //    public DataNode Parent;
    //    public object Key;
    //    public object Data;
    //    public List<DataNode> Children = new();
    //    public string FullId
    //    {
    //        get
    //        {
    //            if(Parent != null)
    //            {
    //                return $"{Parent.FullId}.{{{Key}}}";
    //            }
    //            return $"{{{Key}}}";
    //        }
    //    }
    //    public override string ToString()
    //    {
    //        return Key.ToString();
    //    }
    //}
    //public class TestDataOutput : IDataOutput
    //{
    //    public DataNode Root = new();
    //    public DataNode CurrentNode;
    //    public TestDataOutput()
    //    {
    //        Root = new();
    //        CurrentNode = Root;
    //    }
    //    public TestDataOutput(DataNode root)
    //    {
    //        Root = root;
    //        CurrentNode = root;
    //    }
    //    public void Data(SaveContext context, Type type, object data)
    //    {
    //        Assert.IsNull(CurrentNode.Data, "Data must be set only once");
    //        CurrentNode.Data = data;
    //    }
    //    public bool ScopeBegin(SaveContext context, object key)
    //    {
    //        var n = new DataNode();
    //        n.Parent = CurrentNode;
    //        n.Key = key;
    //        CurrentNode.Children.Add(n);
    //        CurrentNode = n;
    //        return true;
    //    }

    //    public void ScopeEnd(SaveContext context, object key)
    //    {
    //        CurrentNode = CurrentNode.Parent;
    //    }
    //}
    //public class TestDataInput : IDataInput
    //{
    //    public DataNode Root;
    //    public DataNode CurrentNode;
    //    public TestDataInput(DataNode root)
    //    {
    //        Root = root;
    //        CurrentNode = Root;
    //    }

    //    public IEnumerable<object> AllScopeKeys
    //    {
    //        get
    //        {
    //            foreach(var c in CurrentNode.Children)
    //            {
    //                yield return c.Key;
    //            }
    //        }
    //    }


    //    public object Data(SaveContext context, Type type)
    //    {
    //        return CurrentNode.Data;
    //    }

    //    public void DataInPlace(SaveContext context, Type type, object data)
    //    {
    //        context.LogError($"{nameof(TestDataInput)}.{nameof(DataInPlace)}: Could not load-in-place object of type '{type.FullName}'");
    //    }
    //    public object ObjectOfType(SaveContext context, Type type)
    //    {
    //        if (type.IsAssignableFrom(CurrentNode.Data?.GetType()))
    //            return CurrentNode.Data;

    //        context.LogError($"Could not load object of type '{type.FullName}'. Serialized object is of type '{CurrentNode.Data?.GetType()}'");
    //        return null;
    //    }
    //    public bool ScopeBegin(SaveContext context, object key)
    //    {
    //        var c = CurrentNode.Children.Find(c => c.Key == key);
    //        if(c == null)
    //        {
    //            //Assert.Fail($"Could not find scope key '{key}' under '{CurrentNode.FullId}'");
    //            return false;
    //        }
    //        CurrentNode = c;
    //        return true;
    //    }

    //    public void ScopeEnd(SaveContext context, object key)
    //    {
    //        CurrentNode = CurrentNode.Parent;
    //    }
    //}
    public enum TestEnum
    {
        Enum0 = 0,
        Enum1 = 1,
        Enum2 = 2,
        Enum3 = 3,
        Enum31 = 31,
    }
    public struct StructInt
    {
        public int i;
        public StructInt(int  i) { this.i = i; }
    }

    public delegate int DelegateA(int a);
    public struct StructDelegate
    {
        public int Value;
        public event DelegateA Delegate;

        public StructDelegate(int value)
        {
            Delegate = default;
            Value = value;
        }

        public StructDelegate(int value, DelegateA a)
        {
            Value = value;
            Delegate = a;
        }

        public int Invoke()
        {
            return Delegate?.Invoke(Value) ?? 0;
        }    
    }
    public class ClassInt : IComparable//: IEquatable<ClassInt>
    {
        public int i;
        public ClassInt() { }
        public ClassInt(int i) { this.i = i; }

        public int CompareTo(object other)
        {
            if (other is ClassInt o)
                return i.CompareTo(o.i);
            return -1;
        }

        //public override bool Equals(object other)
        //{
        //    if(other is ClassInt o)
        //        return i == o.i;
        //    return false;
        //}
        //public override int GetHashCode()
        //{
        //    return i.GetHashCode();
        //}
        //public bool Equals(ClassInt other)
        //{
        //    return i == other.i;
        //}
    }
    public class ClassWithRef : IComparable//: IEquatable<ClassInt>
    {
        public int Value;
        public object Target;
        public ClassWithRef() { }

        public ClassWithRef(int value)
        {
            Value = value;
        }
        public ClassWithRef(int value, object target)
        {
            Value = value;
            Target = target;
        }
        public int CompareTo(object other)
        {
            if (other is ClassWithRef o)
                return Value.CompareTo(o.Value);
            return -1;
        }
        //public override bool Equals(object other)
        //{
        //    if (other is ClassWithRef o)
        //        return Value == o.Value;
        //    return false;
        //}
        //public override int GetHashCode()
        //{
        //    return Value.GetHashCode();
        //}
        //public bool Equals(ClassInt other)
        //{
        //    return i == other.i;
        //}
    }
    //public static class SaveTestUtils
    //{

    //    public static T TestDataInOut<T>(T goIn)
    //    {
    //        TestDataOutput output = new();
    //        SaveContext outputContext = new();
    //        outputContext.SaveData(goIn, output);

    //        SaveContext inputContext = new();
    //        TestDataInput input = new(output.Root);

    //        return (T)inputContext.LoadData(typeof(T), input);
    //    }

    //    public static void TestDataInOutEqual<T>(T dataIn)
    //        where T : new()
    //    {
    //        var dataOut = TestDataInOut<T>(dataIn);
    //        Assert.AreEqual(dataIn, dataOut);
    //    }




    //    public static void TestObjectInOutEqual<T>(T dataIn)
    //        where T : new()
    //        => TestObjectInOutEqual(dataIn, Assert.AreEqual);
    //    public static void TestObjectInOutEqual<T>(T dataIn, Action<object, object> testEqual)
    //        where T : new()
    //    {
    //        Type dataInType = dataIn.GetType();

    //        TestDataOutput output = new();
    //        SaveContext outputContext = new();
    //        outputContext.SaveObject(dataIn.GetType(), dataIn, output);
    //        Assert.AreEqual(dataInType.AssemblyQualifiedName, output.Root.Children[0].Key);
    //        //Assert.AreEqual(dataIn, output.Root.Children[0].Data);

    //        SaveContext inputContext = new();
    //        TestDataInput input = new(output.Root);
    //        T dataOut = (T)inputContext.LoadObject(input);
    //        testEqual(dataIn, dataOut);
    //    }




    //    #region String Data In-Out
    //    public static T StringDataInOut<T>(T dataIn)
    //    {
    //        StringDataOutput output = new();
    //        SaveContext outputContext = new();
    //        outputContext.SaveData(dataIn, output);

    //        Debug.Log(output.Result);

    //        SaveContext inputContext = new();
    //        StringDataInput input = new(inputContext, output.Result);
    //        return (T)inputContext.LoadData(typeof(T), input);
    //    }
    //    public static T StringDataInOutEqual<T>(T dataIn)
    //        => StringDataInOutEqual(dataIn, CompareEqual);
    //    public static T StringDataInOutEqual<T>(T dataIn, Action<object, object> testEqual)
    //    {
    //        var dataOut = StringDataInOut(dataIn);
    //        testEqual(dataIn, dataOut);
    //        return dataOut;
    //    }
    //    #endregion



    //    #region String Object In-Out
    //    public static T StringObjectInOut<T>(T dataIn)
    //    {
    //        StringDataOutput output = new();
    //        SaveContext outputContext = new();
    //        outputContext.SaveObject(typeof(T), dataIn, output);

    //        Debug.Log(output.Result);

    //        SaveContext inputContext = new();
    //        StringDataInput input = new(inputContext, output.Result);
    //        return (T)inputContext.LoadObject(input);
    //    }
    //    public static T StringObjectInOutEqual<T>(T dataIn)
    //        => StringObjectInOutEqual(dataIn, CompareEqual);
    //    public static T StringObjectInOutEqual<T>(T dataIn, Action<object, object> testEqual)
    //    {
    //        var dataOut = StringObjectInOut(dataIn);
    //        testEqual(dataIn, dataOut);
    //        return dataOut;
    //    }
    //    #endregion



    //    #region String Data In-Out With References
    //    public static T StringDataInOutWithReferences<T>(T dataIn)
    //    {
    //        StringDataOutput output = new();
    //        SaveContext outputContext = new();
    //        outputContext.SaveKeyData("Data", dataIn, output);
    //        outputContext.SaveReferencedObjects(output, "Refs");

    //        Debug.Log(output.Result);

    //        SaveContext inputContext = new();
    //        StringDataInput input = new(inputContext, output.Result);
    //        inputContext.LoadReferencedObjects(input, "Refs");
    //        return (T)inputContext.LoadKeyData("Data", typeof(T), input);
    //    }
    //    public static T StringDataInOutWithReferencesEqual<T>(T dataIn)
    //        => StringDataInOutWithReferencesEqual(dataIn, CompareEqual);
    //    public static T StringDataInOutWithReferencesEqual<T>(T dataIn, Action<object, object> testEqual)
    //    {
    //        var dataOut = StringDataInOutWithReferences(dataIn);
    //        testEqual(dataIn, dataOut);
    //        return dataOut;
    //    }
    //    #endregion

    //    public static void CompareEqual(object a, object b)
    //    {
    //        if(a is IComparable ac)
    //            Assert.IsTrue(ac.CompareTo(b) == 0);
    //        else if (b is IComparable bc)
    //            Assert.IsTrue(bc.CompareTo(a) == 0);
    //        else
    //        Assert.AreEqual(a, b);
    //    }

    //    #region String Object In-Out With References
    //    public static T StringObjectInOutWithReferences<T>(T dataIn)
    //    {
    //        StringDataOutput output = new();
    //        SaveContext outputContext = new();
    //        outputContext.SaveKeyObject("Data", typeof(T), dataIn, output);
    //        outputContext.SaveReferencedObjects(output, "Refs");

    //        Debug.Log(output.Result);

    //        SaveContext inputContext = new();
    //        StringDataInput input = new(inputContext, output.Result);
    //        inputContext.LoadReferencedObjects(input, "Refs");
    //        return (T)inputContext.LoadKeyObject("Data", input);
    //    }
    //    public static T StringObjectInOutWithReferencesEqual<T>(T dataIn)
    //        => StringObjectInOutWithReferencesEqual(dataIn, CompareEqual);
    //    public static T StringObjectInOutWithReferencesEqual<T>(T dataIn, Action<object, object> testEqual)
    //    {
    //        var dataOut = StringObjectInOutWithReferences(dataIn);
    //        testEqual(dataIn, dataOut);
    //        return dataOut;
    //    }
    //    #endregion



    //    #region String References In-Out With References
    //    public static T StringReferenceInOutWithReferences<T>(T refIn)
    //    {
    //        object refOut = default;
    //        var id = Id.FromString("{Ref}");
    //        string data;
    //        {
    //            StringDataOutput output = new();
    //            SaveContext outputContext = new();
    //            using (var _ = outputContext.ScopeKey("Ref", output))
    //            {
    //                outputContext.SaveReference(refIn, output);
    //            }

    //            //Assert.AreEqual("Ref", output.Root.Children[0].Key);
    //            //Assert.AreEqual("{Ref}", output.Root.Children[0].Data);
    //            //if (refIn != null)
    //            //{
    //            //    var roIn = outputContext.SavedObjectReferences[id];
    //            //    testEqual(refIn, (T)roIn.Object);
    //            //}

    //            using (var _ = outputContext.ScopeKey("Refs", output))
    //            {
    //                outputContext.SaveReferencedObjects(output);
    //            }

    //            //Assert.AreEqual("Refs", output.Root.Children[1].Key);
    //            //Assert.AreEqual(id, output.Root.Children[1].Children[0].Key);
    //            //Assert.IsTrue(objectInType.AssemblyQualifiedName == (string)output.Root.Children[1].Children[0].Children[0].Key);
    //            //Assert.AreEqual(objectIn.i, output.Root.Children[1].Children[0].Children[0].Children[0].Data);
    //            data = output.Result;
    //        }
    //        Debug.Log(data);

    //        {
    //            SaveContext inputContext = new();
    //            StringDataInput input = new(inputContext, data);
    //            // Load all saved objects that may be referenced to
    //            using (var _ = inputContext.ScopeKey("Refs", input))
    //            {
    //                Assert.IsFalse(inputContext.MustStop);
    //                inputContext.LoadReferencedObjects(input);
    //                Assert.IsFalse(inputContext.MustStop);
    //            }
    //            Assert.IsFalse(inputContext.MustStop);
    //            //if (refIn != null)
    //            //{
    //            //    var roLoaded = inputContext.SavedObjectReferences[id];
    //            //    Assert.AreEqual(id, roLoaded.FullId);
    //            //    testEqual(refIn, (T)roLoaded.Object);
    //            //}

    //            // Load the reference
    //            using (var _ = inputContext.ScopeKey("Ref", input))
    //            {
    //                Assert.IsFalse(inputContext.MustStop);
    //                refOut = inputContext.LoadReference(input);
    //                Assert.IsFalse(inputContext.MustStop);
    //            }
    //            Assert.IsFalse(inputContext.MustStop);

    //            return (T)refOut;
    //        }
    //    }
    //    public static T StringReferenceInOutWithReferencesEqual<T>(T refIn)
    //        => StringReferenceInOutWithReferencesEqual(refIn, CompareEqual);
    //    public static T StringReferenceInOutWithReferencesEqual<T>(T refIn, Action<object, object> testEqual)
    //    {
    //        T refOut = StringReferenceInOutWithReferences(refIn);
    //        testEqual(refIn, refOut);
    //        return refOut;
    //    }
    //    #endregion
    //}

    //internal class SerializationTests
    //{
    //    [Test]
    //    public void IO2_1()
    //    {
    //        var context = new IO2.StreamContext();
    //        var stringOutput = new StringPrimitiveOutput();
    //        var graphOutput = new IO2.GraphOutput(stringOutput);
    //        IO2.IOutput output = graphOutput;
    //        var ci0 = new ClassInt(31);
    //        var cr1 = new ClassWithRef(3, ci0);
    //        output.Save(context, "cr1", cr1);
    //        output.Save(context, "ci0", ci0);
    //        graphOutput.SaveReferencedObjects(context);

    //        var data = stringOutput.Result;
    //        Debug.Log(data);

    //        var contextInput = new IO2.StreamContext();
    //        var sData = new IO2.StringData();
    //        sData.Parse(contextInput, data);
    //        var r = sData.Root;
    //    }
    //    [Test] public void StringDataDelegate()
    //    {
    //        Delegate d;

    //        int xx = 2;
    //        //var d = (int x) => { };
    //        StructDelegate sd = new(10, (int x) =>
    //        {
    //            return xx * x;
    //        });

    //        var sdOut = SaveTestUtils.StringDataInOutWithReferences(sd);
    //        var result = sdOut.Invoke();
    //        Assert.AreEqual(20, result);
    //        //SaveTestUtils.TestDataInOutEqual((int)31);
    //    }


    //    [Test] public void TestDataInt() => SaveTestUtils.TestDataInOutEqual((int)31);
    //    [Test] public void TestDataFloat() => SaveTestUtils.TestDataInOutEqual((float)31);
    //    [Test] public void TestDataEnum() => SaveTestUtils.TestDataInOutEqual(TestEnum.Enum3);
    //    [Test] public void TestDataStructInt() => SaveTestUtils.TestDataInOutEqual(new StructInt(32));
    //    [Test] public void TestDataClassInt() => SaveTestUtils.TestDataInOutEqual(new ClassInt(31));

    //    [Test] public void TestObjectInt() => SaveTestUtils.TestObjectInOutEqual((int)31);
    //    [Test] public void TestObjectFloat() => SaveTestUtils.TestObjectInOutEqual((float)31);
    //    [Test] public void TestObjectEnum() => SaveTestUtils.TestObjectInOutEqual(TestEnum.Enum3);
    //    [Test] public void TestObjectStructInt() => SaveTestUtils.TestObjectInOutEqual(new StructInt(32));
    //    [Test] public void TestObjectClassInt() => SaveTestUtils.TestObjectInOutEqual(new ClassInt(31));

    //    [Test] public void StringDataInt() => SaveTestUtils.StringDataInOutEqual((int)31);
    //    [Test] public void StringDataFloat() => SaveTestUtils.StringDataInOutEqual((float)31);
    //    [Test] public void StringDataEnum() => SaveTestUtils.StringDataInOutEqual(TestEnum.Enum3);
    //    [Test] public void StringDataStructInt() => SaveTestUtils.StringDataInOutEqual(new StructInt(32));
    //    [Test] public void StringDataClassInt() => SaveTestUtils.StringDataInOutEqual(new ClassInt(31));
    //    [Test] public void StringDataArrayInt() => SaveTestUtils.StringDataInOutEqual(new int[] { 3, 7, 31, 5 });

    //    [Test] public void StringDataArrayClassInt() => SaveTestUtils.StringDataInOutWithReferencesEqual(new ClassInt[] { new ClassInt(5), new ClassInt(31), new ClassInt(7) });

    //    [Test] public void StringDataArrayClassIntWithNull() => SaveTestUtils.StringDataInOutWithReferencesEqual(new ClassInt[] { null, new ClassInt(31), null });
    //    [Test] public void StringDataListInt() => SaveTestUtils.StringDataInOutWithReferencesEqual(new List<int>(new int[]{3,7,31,5}));
    //    [Test] public void StringDataListClassInt() => SaveTestUtils.StringDataInOutWithReferencesEqual(new List<ClassInt>(new ClassInt[] { new ClassInt(5), new ClassInt(31), null }));
    //    [Test] public void StringDataDictionaryClassInt() => SaveTestUtils.StringDataInOutWithReferencesEqual(new Dictionary<ClassInt, ClassInt>(
    //        new KeyValuePair<ClassInt, ClassInt>[]
    //        {
    //            new(new (5), new (31)),
    //            new(new (61), null),
    //            new(new (7), new (13)),
    //        }));


    //    [Test] public void StringObjectInt() => SaveTestUtils.StringObjectInOutEqual((int)31);
    //    [Test] public void StringObjectFloat() => SaveTestUtils.StringObjectInOutEqual((float)31);
    //    [Test] public void StringObjectEnum() => SaveTestUtils.StringObjectInOutEqual(TestEnum.Enum3);
    //    [Test] public void StringObjectStructInt() => SaveTestUtils.StringObjectInOutEqual(new StructInt(32));
    //    [Test] public void StringObjectClassInt() => SaveTestUtils.StringObjectInOutEqual(new ClassInt(31));
    //    [Test] public void StringObjectArrayInt() => SaveTestUtils.StringObjectInOutEqual(new int[] { 3, 7, 31, 5 });
    //    [Test] public void StringObjectArrayClassInt() => SaveTestUtils.StringObjectInOutWithReferencesEqual(new ClassInt[] { new ClassInt(5), new ClassInt(31), new ClassInt(7) });
    //    [Test] public void StringObjectArrayClassIntWithNull() => SaveTestUtils.StringObjectInOutWithReferencesEqual(new ClassInt[] { null, new ClassInt(31), null });
    //    [Test] public void StringObjectListInt() => SaveTestUtils.StringObjectInOutWithReferencesEqual(new List<int>(new int[] { 3, 7, 31, 5 }));
    //    [Test] public void StringObjectListClassInt() => SaveTestUtils.StringObjectInOutWithReferencesEqual(new List<ClassInt>(new ClassInt[] { new ClassInt(5), new ClassInt(31), null }));
    //    [Test] public void StringObjectDictionaryClassInt() => SaveTestUtils.StringObjectInOutWithReferencesEqual(new Dictionary<ClassInt, ClassInt>(
    //        new KeyValuePair<ClassInt, ClassInt>[]
    //        {
    //            new(new (5), new (31)),
    //            new(new (61), null),
    //            new(new (7), new (13)),
    //        }));



    //    [Test]
    //    public void StringReferenceClassInt()
    //    {
    //        var dataOut = SaveTestUtils.StringReferenceInOutWithReferencesEqual(new ClassInt(31), Assert.AreEqual);


    //        //var id = Id.FromString("{Ref}");
    //        //DataNode rootNode = new();
    //        //{
    //        //    TestDataOutput output = new(rootNode);
    //        //    SaveContext outputContext = new();
    //        //    using (var _ = outputContext.ScopeKey("Ref", output))
    //        //    {
    //        //        outputContext.SaveReference(objectIn, output);
    //        //    }

    //        //    Assert.AreEqual("Ref", output.Root.Children[0].Key);
    //        //    Assert.AreEqual("{Ref}", output.Root.Children[0].Data);

    //        //    var roIn = outputContext.SavedObjectReferences[id];
    //        //    Assert.AreEqual(objectIn, roIn.Object);

    //        //    using (var _ = outputContext.ScopeKey("Refs", output))
    //        //    {
    //        //        outputContext.SaveReferencedObjects(output);
    //        //    }

    //        //    Assert.AreEqual("Refs", output.Root.Children[1].Key);
    //        //    Assert.AreEqual(id, output.Root.Children[1].Children[0].Key);
    //        //    //Assert.IsTrue(objectInType.AssemblyQualifiedName == (string)output.Root.Children[1].Children[0].Children[0].Key);
    //        //    //Assert.AreEqual(objectIn.i, output.Root.Children[1].Children[0].Children[0].Children[0].Data);
    //        //}

    //        //{
    //        //    TestDataInput input = new(rootNode);
    //        //    SaveContext inputContext = new();
    //        //    // Load all saved objects that may be referenced to
    //        //    using (var _ = inputContext.ScopeKey("Refs", input))
    //        //    {
    //        //        Assert.IsFalse(inputContext.MustStop);
    //        //        inputContext.LoadReferencedObjects(input);
    //        //        Assert.IsFalse(inputContext.MustStop);
    //        //    }
    //        //    Assert.IsFalse(inputContext.MustStop);
    //        //    var roLoaded = inputContext.SavedObjectReferences[id];
    //        //    Assert.AreEqual(id, roLoaded.FullId);
    //        //    Assert.AreEqual(objectIn, roLoaded.Object);

    //        //    // Load the reference
    //        //    using (var _ = inputContext.ScopeKey("Ref", input))
    //        //    {
    //        //        Assert.IsFalse(inputContext.MustStop);
    //        //        objectOut = inputContext.LoadReference(input);
    //        //        Assert.IsFalse(inputContext.MustStop);
    //        //    }
    //        //    Assert.IsFalse(inputContext.MustStop);

    //        //    Assert.AreEqual(objectIn, objectOut);
    //        //}
    //    }

    //    [Test]
    //    public void StringReferenceClassIntNull()
    //    {
    //        var dataOut = SaveTestUtils.StringReferenceInOutWithReferencesEqual<ClassInt>(null, Assert.AreEqual);
    //    }

    //    [Test] public void TestDataGameObject1() 
    //    {
    //        var dataIn = new GameObject();
    //        dataIn.transform.localPosition = new Vector3(1,2,3);
    //        dataIn.transform.localRotation = Quaternion.Euler(10,15,20);
    //        dataIn.transform.localScale = new Vector3(4, 5, 6);

    //        var dataOut = SaveTestUtils.TestDataInOut(dataIn);

    //        Assert.IsTrue(dataIn.transform.localPosition == dataOut.transform.localPosition);
    //        Assert.IsTrue(dataIn.transform.localRotation == dataOut.transform.localRotation);
    //        Assert.IsTrue(dataIn.transform.localScale == dataOut.transform.localScale);
    //    }

    //    [Test]
    //    public void StringDataGameObject1() 
    //    {
    //        var dataIn = new GameObject();
    //        dataIn.transform.localPosition = new Vector3(1, 2, 3);
    //        dataIn.transform.localRotation = Quaternion.Euler(10, 15, 20);
    //        dataIn.transform.localScale = new Vector3(4, 5, 6);

    //        var dataOut = SaveTestUtils.StringDataInOut(dataIn);

    //        Assert.IsTrue(dataIn.transform.localPosition == dataOut.transform.localPosition);
    //        Assert.IsTrue(dataIn.transform.localRotation == dataOut.transform.localRotation);
    //        Assert.IsTrue(dataIn.transform.localScale == dataOut.transform.localScale);
    //    }

    //    [Test]
    //    public void JsonGameObject1()
    //    {
    //        var dataIn = new GameObject();
    //        dataIn.transform.localPosition = new Vector3(1, 2, 3);
    //        dataIn.transform.localRotation = Quaternion.Euler(10, 15, 20);
    //        dataIn.transform.localScale = new Vector3(4, 5, 6);

    //        var j = JsonUtility.ToJson(dataIn);
    //        Debug.Log(j);
    //        //var dataOut = SaveTestUtils.StringDataInOut(dataIn);
    //        //
    //        //Assert.IsTrue(dataIn.transform.localPosition == dataOut.transform.localPosition);
    //        //Assert.IsTrue(dataIn.transform.localRotation == dataOut.transform.localRotation);
    //        //Assert.IsTrue(dataIn.transform.localScale == dataOut.transform.localScale);
    //    }

    //    //public void StringDataArrayT<T>(T[] dataIn)
    //    //{
    //    //    var dataOut = SaveTestUtils.StringDataInOut(dataIn);
    //    //    Assert.IsInstanceOf(typeof(T[]), dataOut);
    //    //    var arrayOut = (T[])dataOut;
    //    //    Assert.IsTrue(arrayOut.SequenceEqual(dataIn));
    //    //}
    //    //[Test]
    //    //public void StringDataArrayInt()
    //    //{
    //    //    var dataIn = new int[3] { 5, 31, 7 };
    //    //    var dataOut = SaveTestUtils.StringDataInOut(dataIn);
    //    //    Assert.IsInstanceOf(typeof(int[]), dataOut);
    //    //    var arrayOut = (int[])dataOut;
    //    //    Assert.IsTrue(arrayOut.SequenceEqual(new int[]{ 5,31,7}));
    //    //}
    //    //[Test]
    //    //public void StringDataArrayClassInt()
    //    //{
    //    //    var dataIn = new ClassInt[3] { new ClassInt(5), new ClassInt(31), new ClassInt(7) };

    //    //    var dataOut = SaveTestUtils.StringDataInOutWithReferences(dataIn);
    //    //    Assert.IsInstanceOf(typeof(ClassInt[]), dataOut);
    //    //    var arrayOut = (ClassInt[])dataOut;
    //    //    Assert.IsTrue(arrayOut.SequenceEqual(dataIn));
    //    //}
    //    //[Test]
    //    //public void StringDataArrayClassIntWithNull()
    //    //{
    //    //    var dataIn = new ClassInt[3] { null, new ClassInt(31), null };

    //    //    var dataOut = SaveTestUtils.StringDataInOutWithReferences(dataIn);
    //    //    Assert.IsInstanceOf(typeof(ClassInt[]), dataOut);
    //    //    var arrayOut = (ClassInt[])dataOut;
    //    //    Assert.IsTrue(arrayOut.SequenceEqual(dataIn));
    //    //}
    //    //[Test] 
    //    //public void GameObjectData()
    //    //{
    //    //
    //    //    var go = new GameObject();// GameObject.CreatePrimitive(PrimitiveType.Cube);>
    //    //    DataNode rootNode = new();
    //    //    TestDataOutput output = new(rootNode);
    //    //    SaveContext outputContext = new();
    //    //}


    //    [Test]
    //    public void StringDataGameObjectRSM()
    //    {
    //        var dataIn = new GameObject();
    //        dataIn.transform.localPosition = new Vector3(1, 2, 3);
    //        dataIn.transform.localRotation = Quaternion.Euler(10, 15, 20);
    //        dataIn.transform.localScale = new Vector3(4, 5, 6);
    //        var rsm = dataIn.AddComponent<ReactionStateMachine>();
    //        var g0 = rsm.AddGroup("MyGroup0");
    //        var s0 = g0.AddState("MyState0");

    //        var dataOut = SaveTestUtils.StringDataInOutWithReferences(dataIn);

    //        Assert.IsTrue(dataIn.transform.localPosition == dataOut.transform.localPosition);
    //        Assert.IsTrue(dataIn.transform.localRotation == dataOut.transform.localRotation);
    //        Assert.IsTrue(dataIn.transform.localScale == dataOut.transform.localScale);
    //    }


    //    [Test]
    //    public void StringObjectGraph()
    //    {
    //        var o0 = new ClassWithRef(0);
    //        var o1 = new ClassWithRef(1, o0);
    //        var dataOut = SaveTestUtils.StringReferenceInOutWithReferencesEqual(o1);

    //    }
    //    [Test]
    //    public void StringObjectGraphCyclic()
    //    {
    //        var o0 = new ClassWithRef(0);
    //        var o1 = new ClassWithRef(1, o0);
    //        o0.Target = o1;
    //        var dataOut = SaveTestUtils.StringReferenceInOutWithReferencesEqual(o1);

    //    }
    //}

}