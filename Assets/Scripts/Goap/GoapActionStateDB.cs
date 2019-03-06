using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GoapActionStateDB {

    public static Dictionary<INTERACTION_TYPE, string[]> goapActionStates = new Dictionary<INTERACTION_TYPE, string[]>() {
        {INTERACTION_TYPE.EAT_FOOD, new string[]{
            "Eat Success",
            "Eat Fail",
            "Target Missing",
        } }
    };
}
