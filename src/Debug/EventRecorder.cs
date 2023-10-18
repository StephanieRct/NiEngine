using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace NiEngine.Recording
{
    public struct EventTimeStamp
    {
        public int Frame;
        public static EventTimeStamp Now => new EventTimeStamp
        {
            Frame = Time.frameCount,
        };
    }
    
    public class EventRecorder
    {
        public static EventRecorder Instance;
        public EventRecord Root;

        public bool IgnoreConditions = true;
        public bool IgnoreActionSets = true;
        public bool IgnoreEmptyConditions = true;
        public bool IgnoreEmptyUpdate = true;

        int NextId = 1;
        int NextNegativeId = -1;
        public int GetNextId()
        {
            var id = NextId;
            ++NextId;
            if (NextId < 0)
                NextId = 1;
            return id;
        }

        public int GetNegativeNextId()
        {

            var id = NextNegativeId;
            --NextNegativeId;
            if (NextNegativeId > 0)
                NextNegativeId = -1;
            return id;
        }

        public EventRecorder()
        {
            Root = new(this);
        }
    }
}