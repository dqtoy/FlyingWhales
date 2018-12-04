
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQuestGiver {

    int id { get; }
    string name { get; }
    QUEST_GIVER_TYPE questGiverType { get; }
    IObject questGiverObj { get; }
}
