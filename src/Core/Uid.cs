using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NiEngine
{
    [Serializable]
    public struct Uid : IComparable<Uid>, IComparable, IEquatable<Uid>
    {
        public int i0;



        public int i1;
        public bool IsDefault => i0 == 0 && i1 == 0;
        static System.Random Random = new System.Random();
        public static Uid Default => new Uid();
        public static Uid NewUid() => new Uid
        {
            i0 = NextInt(),
            i1 = NextInt(),
        };

        public static int NextInt() => Random.Next();// UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        public int CompareTo(Uid other)
        {
            var c0 = i0.CompareTo(other.i0);
            if (c0 != 0) return c0;
            return i1.CompareTo(other.i1);
        }

        public int CompareTo(object obj)
        {
            if (obj is Uid u) return CompareTo(u);
            throw new InvalidCastException();
        }

        public bool Equals(Uid other) => this == other;
        public override bool Equals(object other)
        {
            if(other is Uid u)
                return Equals(u);
            return false;
        }

        public static bool operator ==(Uid a, Uid b) => a.i0 == b.i0 && a.i1 == b.i1;
        public static bool operator !=(Uid a, Uid b) => a.i0 != b.i0 || a.i1 != b.i1;

        public override string ToString()
            => $"{i0:X8}-{i1:X8}";
        public static bool TryParse(ReadOnlySpan<char> s, out Uid uid)
        {
            uid = default;
            if (s.Length != 17) return false;
            if (s[8] != '-') return false;
            var hex0 = s.Slice(0, 8);
            var hex1 = s.Slice(9, 8);
            uid.i0 = int.Parse(hex0, System.Globalization.NumberStyles.HexNumber);
            uid.i1 = int.Parse(hex1, System.Globalization.NumberStyles.HexNumber);
            return true;
        }

#if UNITY_EDITOR
        public static Uid FromSerializedProperty(SerializedProperty property)
        {
            int i0 = property.FindPropertyRelative("i0")?.intValue ?? 0;
            int i1 = property.FindPropertyRelative("i1")?.intValue ?? 0;
            return new Uid
            {
                i0 = i0,
                i1 = i1,
            };
        }
#endif
    }
}