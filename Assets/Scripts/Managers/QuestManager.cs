using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

    public static QuestManager Instance = null;

    public Dictionary<QUEST_TYPE, List<Quest>> availableQuests;

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        ConstructQuests();
    }

    private void ConstructQuests() {
        availableQuests = new Dictionary<QUEST_TYPE, List<Quest>>();
        QUEST_TYPE[] questTypes = Utilities.GetEnumValues<QUEST_TYPE>();
        for (int i = 0; i < questTypes.Length; i++) {
            QUEST_TYPE type = questTypes[i];
            availableQuests.Add(type, new List<Quest>());
        }
    }

    public void CreateQuest(QUEST_TYPE questType, object data) {
        Quest createdQuest = null;
        switch (questType) {
            case QUEST_TYPE.RELEASE_CHARACTER:
                createdQuest = new ReleaseCharacterQuest(data as ECS.Character);
                break;
            case QUEST_TYPE.BUILD_STRUCTURE:
                break;
            default:
                break;
        }
        if (createdQuest != null) {
            AddAvailableQuest(createdQuest);
        }
    }
    public void OnQuestDone(Quest doneQuest) {
        RemoveAvailableQuest(doneQuest);
        Messenger.Broadcast(Signals.QUEST_DONE, doneQuest);
    }

    private void AddAvailableQuest(Quest quest) {
        availableQuests[quest.questType].Add(quest);
    }
    private void RemoveAvailableQuest(Quest quest) {
        availableQuests[quest.questType].Remove(quest);
    }
    
    public void TakeQuest(QUEST_TYPE type, ECS.Character questTaker, object data) {
        CharacterQuestData questData = ConstructQuestData(type, questTaker, data);
        questTaker.AddQuestData(questData);
    }

    public CharacterQuestData ConstructQuestData(QUEST_TYPE type, ECS.Character questTaker, object data) {
        Quest quest = (GetQuest(type, data));
        if (quest != null) {
            switch (type) {
                case QUEST_TYPE.RELEASE_CHARACTER:
                    return new ReleaseCharacterQuestData(quest, questTaker, data as ECS.Character);
                default:
                    break;
            }
        }
        return null;
    }

    public Quest GetQuest(QUEST_TYPE questType, object data) {
        if (availableQuests.ContainsKey(questType)) {
            List<Quest> quests = availableQuests[questType];
            for (int i = 0; i < quests.Count; i++) {
                Quest currentQuest = quests[i];
                if (currentQuest.Equals(data)) {
                    return currentQuest;
                }
            }
        }
        return null;
    }

    //public CharacterAction GetNextQuestAction(QUEST_TYPE type, ECS.Character character, CharacterQuestData data) {
    //    if (availableQuests.ContainsKey(type)) {
    //        Quest quest = availableQuests[type];
    //        return quest.GetQuestAction(character, data);
    //    } else {
    //        throw new System.Exception("There is no available quest of type " + type.ToString());
    //    }
    //}


}
