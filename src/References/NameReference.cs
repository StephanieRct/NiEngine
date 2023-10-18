using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace NiEngine
{


    [Serializable]
    public struct NameReference : IComparable, IComparable<NameReference>, IEquatable<NameReference>
    {
        public string Name;
        public static implicit operator NameReference(string value) => new NameReference { Name = value };
        public static implicit operator string(NameReference value) => value.Name;
        public NameReference(string name)
        {
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
        public static bool operator ==(NameReference a, NameReference b)
        {
            return a.Name == b.Name;
        }
        public static bool operator !=(NameReference a, NameReference b)
        {
            return a.Name != b.Name;
        }
        int IComparable.CompareTo(object obj)
        {
            if (!(obj is NameReference sn)) return -1;
            return Name.CompareTo(sn.Name);
        }
        int IComparable<NameReference>.CompareTo(NameReference sn)
        {
            return Name.CompareTo(sn.Name);
        }
        bool IEquatable<NameReference>.Equals(NameReference other)
        {
            return Name.Equals(other.Name);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is NameReference sn)) return false;
            return Name.Equals(sn.Name);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

}