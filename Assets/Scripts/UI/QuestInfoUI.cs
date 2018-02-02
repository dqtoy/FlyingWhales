using UnityEngine;
using System.Collections;

public class QuestInfoUI : UIMenu {

    internal bool isShowing = false;

    [SerializeField] private UILabel questInfoLbl;
    [SerializeField] private UIButton showQuestLogsBtn;

    private Quest _currentlyShowingQuest;

	public Quest currentlyShowingQuest{
		get { return _currentlyShowingQuest; }
	}

    public void ShowMenu() {
        isShowing = true;
        this.gameObject.SetActive(true);
        //        tweenPos.PlayForward();
    }
    public void HideMenu() {
        isShowing = false;
        //        tweenPos.PlayReverse();
        this.gameObject.SetActive(false);
        //        UpdateCharacterInfo();
    }

    public void SetQuestAsShowing(Quest quest) {
		if(_currentlyShowingQuest != null) {
			_currentlyShowingQuest.onTaskInfoChanged = null;
        }
		_currentlyShowingQuest = quest;
        quest.onTaskInfoChanged = UpdateQuestInfo;
        if (isShowing) {
            UpdateQuestInfo();
        }
    }

    public void UpdateQuestInfo() {
		if (_currentlyShowingQuest == null) {
            return;
        }
        string text = string.Empty;
		text += "[b]Quest ID:[/b] " + _currentlyShowingQuest.id.ToString();
		text += "\n[b]Quest Type:[/b] " + _currentlyShowingQuest.questType.ToString();
		text += "\n[b]Done:[/b] " + _currentlyShowingQuest.isDone.ToString();
		text += "\n[b]Is Waiting:[/b] " + _currentlyShowingQuest.isWaiting.ToString();
		text += "\n[b]Is Expired:[/b] " + _currentlyShowingQuest.isExpired.ToString();
		if (_currentlyShowingQuest.assignedParty == null) {
            text += "\n[b]Assigned Party:[/b] NONE";
        } else {
			text += "\n[b]Assigned Party:[/b] " + _currentlyShowingQuest.assignedParty.urlName;
			if(_currentlyShowingQuest.currentAction == null) {
                text += "\n[b]Current Action:[/b] Forming Party";
            } else {
				text += "\n[b]Current Action:[/b] " + _currentlyShowingQuest.currentAction.ToString();
            }
        }
        questInfoLbl.text = text;

		if(_currentlyShowingQuest.taskLogs.Count > 0) {
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
		UIManager.Instance.ShowQuestLog(_currentlyShowingQuest);
    }
}
