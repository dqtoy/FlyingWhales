using UnityEngine;
using System.Collections;

public class QuestLogsUI : UIMenu {

    internal bool isShowing = false;

    [SerializeField] private UILabel questLogsLbl;
    [SerializeField] private UIScrollView logsScrollView;

    private Quest currentlyShowingQuest;

    public void ShowQuestLogs(Quest quest) {
        if (currentlyShowingQuest != null) {
            currentlyShowingQuest.onQuestLogsChange = null;
        }
        currentlyShowingQuest = quest;
        quest.onQuestLogsChange = UpdateQuestLogs;
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
            text +=  "- " + currLog + "\n";
        }
        questLogsLbl.text = text;
        logsScrollView.UpdatePosition();
    }
}
