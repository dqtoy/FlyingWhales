using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

    public static QuestManager Instance = null;

    public Dictionary<QUEST_TYPE, Quest> availableQuests;

    private void Awake() {
        Instance = this;
    }

    private void ConstructQuests() {
        availableQuests = new Dictionary<QUEST_TYPE, Quest>();
        QUEST_TYPE[] questTypes = Utilities.GetEnumValues<QUEST_TYPE>();
        for (int i = 0; i < questTypes.Length; i++) {
            QUEST_TYPE type = questTypes[i];
            switch (type) {
                case QUEST_TYPE.RELEASE_CHARACTER:
                    availableQuests.Add(type, new ReleaseCharacterQuest());
                    break;
                default:
                    break;
            }
        }
    }


}
