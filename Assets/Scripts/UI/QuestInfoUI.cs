using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestInfoUI : MonoBehaviour {
    [Header("General")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI jobText;


    public Quest quest { get; private set; }

    public void ShowQuestInfoUI(Quest quest) {
        this.quest = quest;
        PopulateQuestInfo();
        gameObject.SetActive(true);
    }

    public void HideQuestInfoUI() {
        gameObject.SetActive(false);
    }

    public void UpdateQuestInfo() {
        UpdateJobText();
    }

    private void PopulateQuestInfo() {
        titleText.text = quest.name;
        descriptionText.text = quest.description;

        string factionText = "<link=" + '"' + quest.factionOwner.id.ToString() + "_faction" + '"' + ">Faction Owner: <b>" + quest.factionOwner.name + "</b></link>";
        string regionText = "<link=" + '"' + quest.region.coreTile.id.ToString() + "_hextile" + '"' + ">Region: <b>" + (quest.region.area != null ? quest.region.area.name : quest.region.name) + "</b></link>";

        infoText.text = "\n" + factionText + "\n" + regionText;
        UpdateJobText();
    }

    private void UpdateJobText() {
        string jobInfo = "Active Jobs: ";
        if (quest.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < quest.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem job = quest.jobQueue.jobsInQueue[i];
                jobInfo += "\n\t- " + job.name;
                if (job.assignedCharacter != null) {
                    jobInfo += "\n\t\t" + "<link=" + '"' + job.assignedCharacter.id.ToString() + "_character" + '"' + ">Assigned Character: <b>" + job.assignedCharacter.name + "</b></link>";
                } else {
                    jobInfo += "\n\t\tAssigned Character: <b>None</b>";
                }
            }
        } else {
            jobInfo += "None";
        }
        jobText.text = jobInfo;
    }
}
