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

        //string factionText = "<link=" + '"' + quest.factionOwner.id.ToString() + "_faction" + '"' + ">Faction Owner: <b>" + quest.factionOwner.name + "</b></link>";
        //string regionText = "<link=" + '"' + quest.region.coreTile.id.ToString() + "_hextile" + '"' + ">Region: <b>" + (quest.region.area != null ? quest.region.area.name : quest.region.name) + "</b></link>";

        string remainingDaysText = string.Empty;
        if(quest is DivineInterventionQuest) {
            if (PlayerManager.Instance.player.currentDivineInterventionTick > GameManager.ticksPerDay) {
                remainingDaysText = "\nRemaining Days: <b>" + GameManager.Instance.GetCeilingDaysBasedOnTicks(PlayerManager.Instance.player.currentDivineInterventionTick) + "</b>";
            } else {
                remainingDaysText = "\nRemaining Hours: <b>" + GameManager.Instance.GetCeilingHoursBasedOnTicks(PlayerManager.Instance.player.currentDivineInterventionTick) + "</b>";
            }
        }
        infoText.text = remainingDaysText;

        UpdateJobText();
    }

    private void UpdateJobText() {
        string jobInfo = "Associated Events: ";
        if (quest.jobQueue.jobsInQueue.Count > 0) {
            for (int i = 0; i < quest.jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem job = quest.jobQueue.jobsInQueue[i];
                jobInfo += "\n\t<link=" + '"' + i + "_job" + '"' + ">" + (i + 1) + ". <b>" + job.name + "</b></link>";
                //if (job.assignedCharacter != null) {
                //    jobInfo += "\n\t\t" + "<link=" + '"' + job.assignedCharacter.id.ToString() + "_character" + '"' + ">Assigned Character: <b>" + job.assignedCharacter.name + "</b></link>";
                //} else {
                //    jobInfo += "\n\t\tAssigned Character: <b>None</b>";
                //}
            }
        } else {
            jobInfo += "None";
        }
        jobText.text = jobInfo;
    }

    public void OnHoverJobText(object obj) {
        string hoverText = string.Empty;
        if(obj is string) {
            string text = (string) obj;
            if (text.Contains("_job")) {
                string id = text.Substring(0, text.IndexOf('_'));
                int index = int.Parse(id);
                JobQueueItem job = quest.jobQueue.jobsInQueue[index];
                if (job != null) {
                    if(job.jobType == JOB_TYPE.BUILD_GODDESS_STATUE) {
                        hoverText = "This quest aims to build a new Goddess Statue at " + quest.region.name + ". A Goddess Statue allows any resident to assist in speeding up the ritual by offering their own sincere prayer.";
                    } else if (job.jobType == JOB_TYPE.DESTROY_PROFANE_LANDMARK) {
                        hoverText = "This quest aims to destroy one of Ruinarch's Profane structures.";
                    } else if (job.jobType == JOB_TYPE.PERFORM_HOLY_INCANTATION) {
                        hoverText = "This quest aims to perform a holy incantation at a Hallowed Grounds. If successful, it will significantly speed up the ritual.";
                    }
                    if(job.assignedCharacter != null) {
                        hoverText += " " + job.assignedCharacter.name + " is currently undertaking this quest.";
                    }
                }
            }
        }

        if(hoverText != string.Empty) {
            UIManager.Instance.ShowSmallInfo(hoverText);
        }
    }

    public void OnHoverOutJobText() {
        if (UIManager.Instance.IsSmallInfoShowing()) {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
