using System.Collections.Generic;

namespace NiEngine
{
    public static class Range
    {
        public static IEnumerable<int> Ring(int first, int count, int start)
        {
            if (count == 0)
                yield break;
            start = (start - first) % count;
            if (start < first || start > (first + count))
                yield break;
            for (int i = start; i < count; ++i)
                yield return i;

            for (int i = first; i < start; ++i)
                yield return i;
        }

        
    }
}