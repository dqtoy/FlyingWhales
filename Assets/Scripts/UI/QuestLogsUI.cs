using UnityEngine;
using System.Collections;

public class QuestLogsUI : UIMenu {

    internal bool isShowing = false;

    [SerializeField] private UILabel questLogsLbl;
    [SerializeField] private UIScrollView logsScrollView;

    private Quest currentlyShowingQuest;

    public void ShowQuestLogs(Quest quest) {
        if (currentlyShowingQuest != null) {
            currentlyShowingQuest.onTaskLogsChange = null;
        }
        currentlyShowingQuest = quest;
        quest.onTaskLogsChange = UpdateQuestLogs;
        isShowing = true;
        this.gameObject.SetActive(true);
        logsScrollView.ResetPosition();
    }

    public void HideQuestLogs() {
        isShowing = false;
        this.gameObject.SetActive(false);
    }

    public void UpdateQuestLogs() {
        string text = string.Empty;
		text += "[000000]";
        for (int i = 0; i < currentlyShowingQuest.taskLogs.Count; i++) {
            string currLog = currentlyShowingQuest.taskLogs[i];
            text +=  "- " + currLog + "\n";
        }
		text += "[-]";
        questLogsLbl.text = text;
        logsScrollView.UpdatePosition();
    }
}
