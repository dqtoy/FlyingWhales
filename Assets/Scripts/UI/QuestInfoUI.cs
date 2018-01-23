using UnityEngine;
using System.Collections;

public class QuestInfoUI : UIMenu {

    internal bool isShowing = false;

    [SerializeField] private UILabel questInfoLbl;

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
        currentlyShowingQuest = quest;
        if (isShowing) {
            UpdateQuestInfo();
        }
    }

    public void UpdateQuestInfo() {
        if (currentlyShowingQuest == null) {
            return;
        }
        string text = string.Empty;
        text += "[b]Quest Type:[/b] " + currentlyShowingQuest.questType.ToString();
        text += "\n[b]Done:[/b] " + currentlyShowingQuest.isDone.ToString();
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
    }
}
