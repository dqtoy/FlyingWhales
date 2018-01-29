using UnityEngine;
using System.Collections;

public class QuestInfoUI : UIMenu {

    internal bool isShowing = false;

    [SerializeField] private UILabel questInfoLbl;
    [SerializeField] private UIButton showQuestLogsBtn;

    private Quest currentlyShowingQuest;

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
        if(currentlyShowingQuest != null) {
            currentlyShowingQuest.onQuestInfoChanged = null;
        }
        currentlyShowingQuest = quest;
        quest.onQuestInfoChanged = UpdateQuestInfo;
        if (isShowing) {
            UpdateQuestInfo();
        }
    }

    public void UpdateQuestInfo() {
        if (currentlyShowingQuest == null) {
            return;
        }
        string text = string.Empty;
        text += "[b]Quest ID:[/b] " + currentlyShowingQuest.id.ToString();
        text += "\n[b]Quest Type:[/b] " + currentlyShowingQuest.questType.ToString();
        text += "\n[b]Done:[/b] " + currentlyShowingQuest.isDone.ToString();
        text += "\n[b]Is Waiting:[/b] " + currentlyShowingQuest.isWaiting.ToString();
        text += "\n[b]Is Expired:[/b] " + currentlyShowingQuest.isExpired.ToString();
        if (currentlyShowingQuest.assignedParty == null) {
            text += "\n[b]Assigned Party:[/b] NONE";
        } else {
            text += "\n[b]Assigned Party:[/b] " + currentlyShowingQuest.assignedParty.name;
            if(currentlyShowingQuest.currentAction == null) {
                text += "\n[b]Current Action:[/b] Forming Party";
            } else {
                text += "\n[b]Current Action:[/b] " + currentlyShowingQuest.currentAction.ToString();
            }
        }
        questInfoLbl.text = text;

        if(currentlyShowingQuest.questLogs.Count > 0) {
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
