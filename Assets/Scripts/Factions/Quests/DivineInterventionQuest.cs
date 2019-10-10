//#define TESTING_VALUES

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DivineInterventionQuest : Quest {

    public DivineInterventionQuest(Faction factionOwner, Region region) : base(factionOwner, region) {
        name = "Divine Intervention Quest";
        description = factionOwner.name + " has begun the ritual that will call for the Goddess's Return. If the ritual finishes, this Divine Intervention will cleanse the world of all the Ruinarch's corruption and banish the demons back to the Nether Realm.";
    }

    public DivineInterventionQuest(SaveDataQuest data) : base(data) {
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
        //if (GameManager.Instance.tick == GameManager.ticksPerDay) {
        //    for (int i = 0; i < jobQueue.jobsInQueue.Count; i++) {
        //        JobQueueItem job = jobQueue.jobsInQueue[i];
        //        if(job.assignedCharacter == null) {
        //            jobQueue.CancelJob(job);
        //            i--;
        //        }
        //    }
        //}

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
        if (GameManager.Instance.tick == 72 || GameManager.Instance.tick == 132) { //72 = 6:00AM, 132 = 11:00AM
            string summary = GameManager.Instance.TodayLogString() + " Will try to create build goddess statue job";
#if TESTING_VALUES
            int roll = 0;
#else
            int roll = UnityEngine.Random.Range(0, 100);
#endif
            bool hasExistingJob = jobQueue.HasJob(JOB_TYPE.BUILD_GODDESS_STATUE);
            bool hasEmptyGoddessStatue = IsThereStillEmptyGoddessStatueSpot();
            summary += "\nRoll is: " + roll.ToString();
            summary += "\nHas Existing Job?: " + hasExistingJob.ToString();
            summary += "\nHas empty goddess statue?: " + hasEmptyGoddessStatue.ToString();
            if (roll < 20 && !hasExistingJob && hasEmptyGoddessStatue) {
                summary += "\nMet all requirements, creating build goddess statue job.";
                //Create Job Here
                CreateBuildGoddessStatueJob();
            }
            Debug.Log(summary);
        }
    }
    private bool IsThereStillEmptyGoddessStatueSpot() {
        return region.area.GetTileObjectsOfType(TILE_OBJECT_TYPE.GODDESS_STATUE).Where(x => x.state == POI_STATE.INACTIVE).ToList().Count > 0;
    }

    private void TryCreateDestroyProfaneLandmarkJob() {
        if (GameManager.Instance.tick == 72 || GameManager.Instance.tick == 132) { //72 = 6:00AM, 132 = 11:00AM
            string summary = GameManager.Instance.TodayLogString() + " Will try to create build goddess statue job";
#if TESTING_VALUES
            int roll = 0;
#else
            int roll = UnityEngine.Random.Range(0, 100);
#endif
            bool hasExistingJob = jobQueue.HasJob(JOB_TYPE.DESTROY_PROFANE_LANDMARK);
            bool hasProfaneLandmarks = AreThereProfaneLandmarks();
            summary += "\nRoll is: " + roll.ToString();
            summary += "\nHas Existing Job?: " + hasExistingJob.ToString();
            summary += "\nHas profane landmarks?: " + hasProfaneLandmarks.ToString();
            if (roll < 20 && !hasExistingJob && hasProfaneLandmarks) {
                summary += "\nMet all requirements, creating destroy profane job.";
                //Create Job Here
                CreateDestroyProfaneJob();
            }
            Debug.Log(summary);
        }
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
        if (GameManager.Instance.tick == 72 || GameManager.Instance.tick == 132) { //72 = 6:00AM, 132 = 11:00AM
            string summary = GameManager.Instance.TodayLogString() + " Will try to create build goddess statue job";
#if TESTING_VALUES
            int roll = 0;
#else
            int roll = UnityEngine.Random.Range(0, 100);
#endif
            bool hasExistingJob = jobQueue.HasJob(JOB_TYPE.PERFORM_HOLY_INCANTATION);
            bool hasHallowedGrounds = AreThereHallowedGrounds();
            summary += "\nRoll is: " + roll.ToString();
            summary += "\nHas Existing Job?: " + hasExistingJob.ToString();
            summary += "\nHas hallowed grounds?: " + hasHallowedGrounds.ToString();
            if (roll < 20 && !hasExistingJob && hasHallowedGrounds) {
                summary += "\nMet all requirements, creating holy incantation job.";
                //Create Job Here
                CreateHolyIncantationJob();
            }
            Debug.Log(summary);
        }
    }
    private bool AreThereHallowedGrounds() {
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region currRegion = GridMap.Instance.allRegions[i];
            if (currRegion.HasFeature(RegionFeatureDB.Hallowed_Ground_Feature)) {
                return true;
            }
        }
        return false;
    }

    #region Build Goddess Statue
    private void CreateBuildGoddessStatueJob() {
        List<TileObject> goddessStatues = region.area.GetTileObjectsOfType(TILE_OBJECT_TYPE.GODDESS_STATUE).Where(x => x.state == POI_STATE.INACTIVE).ToList();
        TileObject target = goddessStatues[Random.Range(0, goddessStatues.Count)];
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.BUILD_GODDESS_STATUE, INTERACTION_TYPE.CRAFT_TILE_OBJECT, target);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeBuildGoddessStatueJob);
        jobQueue.AddJobInQueue(job);

        //expires at end of day
        GameDate expiryDate = GameManager.Instance.Today();
        expiryDate.SetTicks(GameManager.ticksPerDay);
        SchedulingManager.Instance.AddEntry(expiryDate, () => CheckIfJobWillExpire(job), this);
    }
    #endregion

    #region Destroy Profane
    private void CreateDestroyProfaneJob() {
        CharacterStateJob job = new CharacterStateJob(JOB_TYPE.DESTROY_PROFANE_LANDMARK, CHARACTER_STATE.MOVE_OUT, null);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoDestroyProfaneJob);
        jobQueue.AddJobInQueue(job);

        //expires at end of day
        GameDate expiryDate = GameManager.Instance.Today();
        expiryDate.SetTicks(GameManager.ticksPerDay);
        SchedulingManager.Instance.AddEntry(expiryDate, () => CheckIfJobWillExpire(job), this);
    }
    #endregion

    #region Holy Incantation
    private void CreateHolyIncantationJob() {
        CharacterStateJob job = new CharacterStateJob(JOB_TYPE.PERFORM_HOLY_INCANTATION, CHARACTER_STATE.MOVE_OUT, null);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoHolyIncantationJob);
        jobQueue.AddJobInQueue(job);

        //expires at end of day
        GameDate expiryDate = GameManager.Instance.Today();
        expiryDate.SetTicks(GameManager.ticksPerDay);
        SchedulingManager.Instance.AddEntry(expiryDate, () => CheckIfJobWillExpire(job), this);
    }
    #endregion

    #region Sabotage Faction
    public void CreateSabotageFactionnJob() {
        CharacterStateJob job = new CharacterStateJob(JOB_TYPE.SABOTAGE_FACTION, CHARACTER_STATE.MOVE_OUT);
        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoSabotageFactionJob);
        jobQueue.AddJobInQueue(job);
    }
    #endregion

    #region Utilities
    private void CheckIfJobWillExpire(JobQueueItem item) {
        if (item.assignedCharacter == null && item.jobQueueParent != null && item.jobQueueParent.jobsInQueue.Contains(item)) {
            Debug.Log(GameManager.Instance.TodayLogString() + item.jobType.ToString() + " expired.");
            item.jobQueueParent.RemoveJobInQueue(item);
        }
    }
    #endregion
}
