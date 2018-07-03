using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

    public static QuestManager Instance = null;

    public Dictionary<QUEST_TYPE, Quest> availableQuests;

    private void Awake() {
        Instance = this;
    }


}
