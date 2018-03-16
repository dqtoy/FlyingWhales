using UnityEngine;
using System.Collections;

public class QuestInfoUI : UIMenu {

    [SerializeField] private UILabel questInfoLbl;
    [SerializeField] private UIButton showQuestLogsBtn;

	public OldQuest.Quest currentlyShowingQuest{
		get { return _data as OldQuest.Quest; }
	}
    public override void OpenMenu() {
        base.OpenMenu();
        UpdateQuestInfo();
    }

    public override void SetData(object data) {
        if (currentlyShowingQuest != null) {
            currentlyShowingQuest.onTaskInfoChanged = null;
        }
        base.SetData(data);
        (data as OldQuest.Quest).onTaskInfoChanged = UpdateQuestInfo;
        if (isShowing) {
            UpdateQuestInfo();
        }
    }

    public void UpdateQuestInfo() {
		if (currentlyShowingQuest == null) {
            return;
        }
        string text = string.Empty;
		text += "[b]OldQuest.Quest ID:[/b] " + currentlyShowingQuest.id.ToString();
		text += "\n[b]OldQuest.Quest Type:[/b] " + currentlyShowingQuest.questName;
		text += "\n[b]Done:[/b] " + currentlyShowingQuest.isDone.ToString();
		text += "\n[b]Is Waiting:[/b] " + currentlyShowingQuest.isWaiting.ToString();
		text += "\n[b]Is Expired:[/b] " + currentlyShowingQuest.isExpired.ToString();
		if (currentlyShowingQuest.assignedParty == null) {
            text += "\n[b]Assigned Party:[/b] NONE";
        } else {
			text += "\n[b]Assigned Party:[/b] " + currentlyShowingQuest.assignedParty.urlName;
			if(currentlyShowingQuest.currentAction == null) {
                text += "\n[b]Current Action:[/b] Forming Party";
            } else {
				text += "\n[b]Current Action:[/b] " + currentlyShowingQuest.currentAction.ToString();
            }
        }
        questInfoLbl.text = text;

		if(currentlyShowingQuest.taskLogs.Count > 0) {
            //enable button
            showQuestLogsBtn.GetComponent<BoxCollider>().enabled = true;
            showQuestLogsBtn.SetState(UIButtonColor.State.Normal, true);
        } else {
            //disable button
            showQuestLogsBtn.GetComponent<BoxCollider>().enabled = false;
            showQuestLogsBtn.SetState(UIButtonColor.State.Disabled, true);

        }

    }
    public void ShowQuestLogs() {
		UIManager.Instance.ShowQuestLog(currentlyShowingQuest);
    }
}
