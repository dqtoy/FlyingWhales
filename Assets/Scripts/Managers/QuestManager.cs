using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour {

    public static QuestManager Instance = null;

    //public Dictionary<QUEST_TYPE, List<Quest>> availableQuests;

    private void Awake() {
        Instance = this;
    }

    public void Initialize() {
        //ConstructQuests();
    }

    public void RemoveQuestFromBoards(Quest quest) {
        List<BaseLandmark> landmarks = LandmarkManager.Instance.GetAllLandmarksWithQuestBoard();
        for (int i = 0; i < landmarks.Count; i++) {
            landmarks[i].questBoard.RemoveQuest(quest);
        }
    }

    //private void ConstructQuests() {
    //    availableQuests = new Dictionary<QUEST_TYPE, List<Quest>>();
    //    QUEST_TYPE[] questTypes = Utilities.GetEnumValues<QUEST_TYPE>();
    //    for (int i = 0; i < questTypes.Length; i++) {
    //        QUEST_TYPE type = questTypes[i];
    //        availableQuests.Add(type, new List<Quest>());
    //    }
    //}

    //public void CreateQuest(QUEST_TYPE questType, object data) {
    //    Quest createdQuest = null;
    //    switch (questType) {
    //        case QUEST_TYPE.RELEASE_CHARACTER:
    //            createdQuest = new ReleaseCharacterQuest(data as Character);
    //            break;
    //        case QUEST_TYPE.BUILD_STRUCTURE:
    //            createdQuest = new BuildStructureQuest()
    //            break;
    //        default:
    //            break;
    //    }
    //    if (createdQuest != null) {
    //        AddAvailableQuest(createdQuest);
    //    }
    //}
    //public void OnQuestDone(Quest doneQuest) {
        //RemoveAvailableQuest(doneQuest);
        //Messenger.Broadcast(Signals.QUEST_DONE, doneQuest);
    //}

    //public void AddAvailableQuest(Quest quest) {
    //    availableQuests[quest.questType].Add(quest);
    //}
    //private void RemoveAvailableQuest(Quest quest) {
    //    availableQuests[quest.questType].Remove(quest);
    //}
    
    //public void TakeQuest(QUEST_TYPE type, Character questTaker, object data = null) {
    //    //CharacterQuestData questData = ConstructQuestData(type, questTaker, data);
    //    //questTaker.AddQuestData(questData);
    //    //questTaker.OnTakeQuest(questData.parentQuest);
    //}
    //public void TakeQuest(Quest quest, Character questTaker, object data = null) {
    //    //CharacterQuestData questData = ConstructQuestData(quest, questTaker, data);
    //    //questTaker.AddQuestData(questData);
    //    questTaker.OnTakeQuest(quest);
    //}

    //private CharacterQuestData ConstructQuestData(QUEST_TYPE type, Character questTaker, object data) {
    //    Quest quest = (GetQuest(type, data));
    //    return ConstructQuestData(quest, questTaker, data);
    //}
    //private CharacterQuestData ConstructQuestData(Quest quest, Character questTaker, object data) {
    //    if (quest != null) {
    //        switch (quest.questType) {
    //            //case QUEST_TYPE.RELEASE_CHARACTER:
    //            //    return new ReleaseCharacterQuestData(quest, questTaker, data as Character);
    //            //case QUEST_TYPE.BUILD_STRUCTURE:
    //            //    return new BuildStructureQuestData(quest, questTaker);
    //            default:
    //                break;
    //        }
    //    }
    //    return null;
    //}

    //public Quest GetQuest(QUEST_TYPE questType, object data) {
    //    if (availableQuests.ContainsKey(questType)) {
    //        List<Quest> quests = availableQuests[questType];
    //        for (int i = 0; i < quests.Count; i++) {
    //            Quest currentQuest = quests[i];
    //            if (currentQuest.Equals(data)) {
    //                return currentQuest;
    //            }
    //        }
    //    }
    //    return null;
    //}

    //public CharacterAction GetNextQuestAction(QUEST_TYPE type, Character character, CharacterQuestData data) {
    //    if (availableQuests.ContainsKey(type)) {
    //        Quest quest = availableQuests[type];
    //        return quest.GetQuestAction(character, data);
    //    } else {
    //        throw new System.Exception("There is no available quest of type " + type.ToString());
    //    }
    //}


}
