using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DivineInterventionQuest : Quest {

    public DivineInterventionQuest(Faction factionOwner, Region region) : base(factionOwner, region) {
        name = "Divine Intervention Quest";
    }

    #region Overrides
    public override void ActivateQuest() {
        base.ActivateQuest();
        Messenger.AddListener(Signals.TICK_STARTED, PerTickOnQuest);
    }
    public override void FinishQuest() {
        base.FinishQuest();
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickOnQuest);
    }
    #endregion

    private void PerTickOnQuest() {
        //Remove jobs at the end of the day if it hasn't been taken
        if (GameManager.Instance.tick == GameManager.ticksPerDay) {
            for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
                JobQueueItem job = jobQueue.jobsInQueue[i];
                if(job.assignedCharacter == null) {
                    jobQueue.CancelJob(job);
                    i--;
                }
            }
        }

        //Cancel jobs that are no longer applicable
        //Might have performance issue, but for now this will do
        //Will improve later
        for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = jobQueue.jobsInQueue[i];
            if(job.jobType == JOB_TYPE.BUILD_GODDESS_STATUE && !IsThereStillEmptyGoddessStatueSpot()) {
                jobQueue.CancelJob(job);
                i--;
            }else if (job.jobType == JOB_TYPE.DESTROY_PROFANE_LANDMARK && !AreThereProfaneLandmarks()) {
                jobQueue.CancelJob(job);
                i--;
            }else if (job.jobType == JOB_TYPE.PERFORM_HOLY_INCANTATION && !AreThereHallowedGrounds()) {
                jobQueue.CancelJob(job);
                i--;
            }
        }

        TryCreateBuildGoddessStatueJob();
        TryCreateDestroyProfaneLandmarkJob();
        TryCreatePerformHolyIncantationJob();
    }

    private void TryCreateBuildGoddessStatueJob() {
        if (GameManager.Instance.tick == 72 && UnityEngine.Random.Range(0, 100) < 20) { //72 = 6:00AM
            if (!jobQueue.HasJob(JOB_TYPE.BUILD_GODDESS_STATUE) && IsThereStillEmptyGoddessStatueSpot()) {
                //Create Job Here
            }
        }
    }
    private bool CanCharacterTakeBuildGoddessStatueJob(Character character) {
        return character.GetNormalTrait("Craftsman") != null;
    }
    private bool IsThereStillEmptyGoddessStatueSpot() {
        //TODO
        return false;
    }

    private void TryCreateDestroyProfaneLandmarkJob() {
        if (GameManager.Instance.tick == 72 && UnityEngine.Random.Range(0, 100) < 20) { //72 = 6:00AM
            if (!jobQueue.HasJob(JOB_TYPE.DESTROY_PROFANE_LANDMARK) && AreThereProfaneLandmarks()) {
                //Create Job Here
            }
        }
    }
    private bool CanCharacterTakeDestroyProfaneLandmarkJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER;
    }
    private bool AreThereProfaneLandmarks() {
        for (int i = 0; i < PlayerManager.Instance.player.playerFaction.ownedRegions.Count; i++) {
            BaseLandmark landmark = PlayerManager.Instance.player.playerFaction.ownedRegions[i].mainLandmark;
            if(landmark.specificLandmarkType == LANDMARK_TYPE.THE_PROFANE) {
                return true;
            }
        }
        return false;
    }

    private void TryCreatePerformHolyIncantationJob() {
        if (GameManager.Instance.tick == 72 && UnityEngine.Random.Range(0, 100) < 20) { //72 = 6:00AM
            if (!jobQueue.HasJob(JOB_TYPE.PERFORM_HOLY_INCANTATION) && AreThereHallowedGrounds()) {
                //Create Job Here
            }
        }
    }
    private bool CanCharacterTakePerformHolyIncantationJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.ADVENTURER;
    }
    private bool AreThereHallowedGrounds() {
        //TODO
        return false;
    }
}
