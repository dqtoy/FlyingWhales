using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

    public static QuestManager Instance = null;

    public Dictionary<QUEST_TYPE, Quest> availableQuests;

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        ConstructQuests();
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
    
    public void TakeQuest(QUEST_TYPE type, ECS.Character questTaker, object data) {
        CharacterQuestData questData = ConstructQuestData(type, questTaker, data);
        questTaker.AddQuestData(questData);
    }

    public CharacterQuestData ConstructQuestData(QUEST_TYPE type, ECS.Character questTaker, object data) {
        switch (type) {
            case QUEST_TYPE.RELEASE_CHARACTER:
                return new ReleaseCharacterQuestData(availableQuests[type], questTaker, data as ECS.Character);
            default:
                break;
        }
        return null;
    }

    public CharacterAction GetNextQuestAction(QUEST_TYPE type, ECS.Character character, CharacterQuestData data) {
        if (availableQuests.ContainsKey(type)) {
            Quest quest = availableQuests[type];
            return quest.GetQuestAction(character, data);
        } else {
            throw new System.Exception("There is no available quest of type " + type.ToString());
        }
    }


}
