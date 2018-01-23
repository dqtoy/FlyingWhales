using UnityEngine;
using System.Collections;

public class QuestLogsUI : UIMenu {

    internal bool isShowing = false;

    [SerializeField] private UILabel questLogsLbl;

    private Quest currentlyShowingQuest;

    public void ShowQuestLogs(Quest quest) {
        currentlyShowingQuest = quest;
        isShowing = true;
        this.gameObject.SetActive(true);
    }

    public void HideQuestLogs() {
        isShowing = false;
        this.gameObject.SetActive(false);
    }

    public void UpdateQuestLogs() {
        string text = string.Empty;
        for (int i = 0; i < currentlyShowingQuest.questLogs.Count; i++) {
            string currLog = currentlyShowingQuest.questLogs[i];
            text += currLog + "\n - ";
        }
        questLogsLbl.text = text;
    }
}
