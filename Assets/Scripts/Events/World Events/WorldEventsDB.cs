using System.Collections;
using UnityEngine;
namespace Events.World_Events {
    public class JobWorldEventData {
        public WORLD_EVENT_EFFECT[] neededEffects;
        public System.Func<Character, JOB_TYPE, Region> validRegionGetter;
    }
}