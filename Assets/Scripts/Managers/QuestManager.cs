using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour {

    public static QuestManager Instance = null;

    public List<Quest> availableQuests = new List<Quest>();

    private void Awake() {
        Instance = this;
    }

    #region Quest Management
    public void AddQuestToAvailableQuests(Quest quest) {
        if (!availableQuests.Contains(quest)) {
            availableQuests.Add(quest);
        }
    }
    public void RemoveQuestFromAvailableQuests(Quest quest) {
        availableQuests.Remove(quest);
    }
    public List<Quest> GetAvailableQuestsForCharacter(ECS.Character character) {
        List<Quest> questsForCharacter = new List<Quest>();
        for (int i = 0; i < availableQuests.Count; i++) {
            Quest currAvailableQuest = availableQuests[i];
            if (currAvailableQuest.CanAcceptQuest(character)) {
                questsForCharacter.Add(currAvailableQuest);
            }
        }
        return questsForCharacter;
    }
    #endregion

}
