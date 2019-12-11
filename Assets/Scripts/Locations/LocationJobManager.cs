using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationJobManager {
    public Area location { get; private set; }

    private int createJobsTriggerTick;
    private int currentExistingJobsCount;
    private List<string> jobNames;

    public LocationJobManager(Area location) {
        this.location = location;
        jobNames = new List<string>();
        createJobsTriggerTick = GameManager.Instance.GetTicksBasedOnHour(9); //150
        Messenger.AddListener(Signals.TICK_STARTED, ProcessJobs);
    }


    private void ProcessJobs() {
        if(GameManager.Instance.tick == createJobsTriggerTick && currentExistingJobsCount < 2) {
            if (!JobsPart1()) {
                //if (!JobsPart2()) {
                //    JobsPart3();
                //}
            }
        }else if (GameManager.Instance.IsEndOfDay()) {
            OnEndOfDay();
        }
    }
    private void OnEndOfDay() {
        int chance = UnityEngine.Random.Range(0, 2);
        if(currentExistingJobsCount > 0) {
            for (int i = 0; i < location.availableJobs.Count; i++) {
                JobQueueItem job = location.availableJobs[i];
                if(job.assignedCharacter == null && chance == 0 && IsJobAnOutsideJob(job)) {
                    job.ForceCancelJob(false);
                }
            }
        }

    }
    public void OnAddToAvailableJobs(JobQueueItem addedJob) {
        if(IsJobAnOutsideJob(addedJob)) {
            currentExistingJobsCount++;
        }
    }
    public void OnRemoveFromAvailableJobs(JobQueueItem removedJob) {
        if (IsJobAnOutsideJob(removedJob)) {
            currentExistingJobsCount--;
        }
    }
    private bool IsJobAnOutsideJob(JobQueueItem job) {
        if (job.jobType == JOB_TYPE.CLEANSE_REGION || job.jobType == JOB_TYPE.CLAIM_REGION || job.jobType == JOB_TYPE.INVADE_REGION
            || job.jobType == JOB_TYPE.ATTACK_NON_DEMONIC_REGION || job.jobType == JOB_TYPE.ATTACK_DEMONIC_REGION) {
            return true;
        }
        return false;
    }

    #region Part 1
    private bool JobsPart1() {
        jobNames.Clear();
        jobNames.Add("Cleanse");
        jobNames.Add("Claim");
        jobNames.Add("Invade");

        bool hasCreateJob = false;
        string chosenJobName = string.Empty;
        int index = 0;
        while (!hasCreateJob && jobNames.Count > 0) {
            index = UnityEngine.Random.Range(0, jobNames.Count);
            chosenJobName = jobNames[index];
            if(chosenJobName == "Cleanse") {
                hasCreateJob = CreateCleanseRegionJob();
            }else if (chosenJobName == "Claim") {
                hasCreateJob = CreateClaimRegionJob();
            } else if (chosenJobName == "Invade") {
                hasCreateJob = CreateInvadeRegionJob();
            }
            jobNames.RemoveAt(index);
        }
        return hasCreateJob;
    }
    private bool CreateCleanseRegionJob() {
        if (UnityEngine.Random.Range(0, 2) <= 0) {
            Region region;
            if (TryGetCorruptedRegionWithoutLandmark(out region)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLEANSE_REGION, INTERACTION_TYPE.CLEANSE_REGION, region.regionTileObject, location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoCleanseRegionJob);
                location.AddToAvailableJobs(job);
                return true;
            }
        }
        return false;
    }
    private bool CreateClaimRegionJob() {
        if (UnityEngine.Random.Range(0, 2) == 0) {
            Region region;
            if (TryGetAdjacentRegionWithoutFactionOwner(out region)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.CLAIM_REGION, INTERACTION_TYPE.CLAIM_REGION, region.regionTileObject, location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoClaimRegionJob);
                location.AddToAvailableJobs(job);
                return true;
            }
        }
        return false;
    }
    private bool CreateInvadeRegionJob() {
        if (UnityEngine.Random.Range(0, 2) == 0) {
            Region region;
            if (TryGetAdjacentRegionAtWarWithThisLocation(out region)) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INVADE_REGION, INTERACTION_TYPE.INVADE_REGION, region.regionTileObject, location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoInvadeRegionJob);
                location.AddToAvailableJobs(job);
                return true;
            }
        }
        return false;
    }
    private bool TryGetCorruptedRegionWithoutLandmark(out Region validRegion) {
        List<Region> regions = PlayerManager.Instance.player.playerFaction.ownedRegions;
        for (int i = 0; i < regions.Count; i++) {
            Region region = regions[i];
            if(region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
                validRegion = region;
                return true;
            }
        }
        validRegion = null;
        return false;
    }
    private bool TryGetAdjacentRegionWithoutFactionOwner(out Region validRegion) {
        List<Region> connectedRegions = location.region.connections;
        for (int i = 0; i < connectedRegions.Count; i++) {
            Region region = connectedRegions[i];
            if (!region.coreTile.isCorrupted && region.owner == null) {
                validRegion = region;
                return true;
            }
        }
        validRegion = null;
        return false;
    }
    private bool TryGetAdjacentRegionAtWarWithThisLocation(out Region validRegion) {
        List<Region> connectedRegions = location.region.connections;
        for (int i = 0; i < connectedRegions.Count; i++) {
            Region region = connectedRegions[i];
            if (!region.coreTile.isCorrupted && region.area == null && region.owner != null
                && region.residents.Count <= 0 && region.owner.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.HOSTILE, location.region.owner)) {
                validRegion = region;
                return true;
            }
        }
        validRegion = null;
        return false;
    }
    #endregion

    #region Part 2
    private bool JobsPart2() {
        jobNames.Clear();
        jobNames.Add("AttackNonDemonic");
        jobNames.Add("AttackDemonic");

        bool hasCreateJob = false;
        string chosenJobName = string.Empty;
        int index = 0;
        while (!hasCreateJob && jobNames.Count > 0) {
            index = UnityEngine.Random.Range(0, jobNames.Count);
            chosenJobName = jobNames[index];
            if (chosenJobName == "AttackNonDemonic") {
                hasCreateJob = CreateAttackNonDemonicRegionJobPart2();
            } else if (chosenJobName == "AttackDemonic") {
                hasCreateJob = CreateAttackDemonicRegionJobPart2();
            }
            jobNames.RemoveAt(index);
        }
        return hasCreateJob;
    }
    private bool CreateAttackNonDemonicRegionJobPart2() {
        if (UnityEngine.Random.Range(0, 2) == 0) {
            if (HasOccupiedNonSettlementTileAtWarWithThisLocation()) {
                CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.ATTACK_NON_DEMONIC_REGION, CHARACTER_STATE.MOVE_OUT, location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoAttackNonDemonicRegionJob);
                location.AddToAvailableJobs(job);
                return true;
            }
        }
        return false;
    }
    private bool CreateAttackDemonicRegionJobPart2() {
        if (UnityEngine.Random.Range(0, 2) == 0) {
//            if (!TryGetCorruptedRegionWithoutLandmark()) {
//                CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.ATTACK_DEMONIC_REGION, CHARACTER_STATE.MOVE_OUT, location);
//                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoAttackDemonicRegionJob);
//                location.AddToAvailableJobs(job);
//                return true;
//            }
        }
        return false;
    }
    private bool HasOccupiedNonSettlementTileAtWarWithThisLocation() {
        Region[] regions = GridMap.Instance.allRegions;
        for (int i = 0; i < regions.Length; i++) {
            Region region = regions[i];
            if (region.residents.Count > 0 && region.area == null && region.owner.HasRelationshipStatusWith(FACTION_RELATIONSHIP_STATUS.HOSTILE, location.region.owner)) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Part 3
    private bool JobsPart3() {
        jobNames.Clear();
        jobNames.Add("AttackNonDemonic");
        jobNames.Add("AttackDemonic");

        bool hasCreateJob = false;
        string chosenJobName = string.Empty;
        int index = 0;
        while (!hasCreateJob && jobNames.Count > 0) {
            index = UnityEngine.Random.Range(0, jobNames.Count);
            chosenJobName = jobNames[index];
            if (chosenJobName == "AttackNonDemonic") {
                hasCreateJob = CreateAttackNonDemonicRegionJobPart3();
            } else if (chosenJobName == "AttackDemonic") {
                hasCreateJob = CreateAttackDemonicRegionJobPart3();
            }
            jobNames.RemoveAt(index);
        }
        return hasCreateJob;
    }
    private bool CreateAttackNonDemonicRegionJobPart3() {
        if (UnityEngine.Random.Range(0, 4) == 0) {
            if (IsAtWarWithFactionThatSettlementNotBlockedByGarrison()) {
                CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.ATTACK_NON_DEMONIC_REGION, CHARACTER_STATE.MOVE_OUT, location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoAttackNonDemonicRegionJob);
                location.AddToAvailableJobs(job);
                return true;
            }
        }
        return false;
    }
    private bool CreateAttackDemonicRegionJobPart3() {
        if (UnityEngine.Random.Range(0, 2) == 0) {
            if (IsPlayerOnlyRemainingStructureIsPortal()) {
                CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.ATTACK_DEMONIC_REGION, CHARACTER_STATE.MOVE_OUT, location);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoAttackDemonicRegionJob);
                location.AddToAvailableJobs(job);
                return true;
            }
        }
        return false;
    }
    private bool IsAtWarWithFactionThatSettlementNotBlockedByGarrison() {
        //TODO: Is blocked by garrison checker
        foreach (KeyValuePair<Faction, FactionRelationship> kvp in location.region.owner.relationships) {
            if (kvp.Key.isPlayerFaction) {
                continue; //exclude player faction
            }
            if (kvp.Value.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE && kvp.Key.HasOwnedRegionWithSettlement()) {
                return true;
            }
        }
        return false;
    }
    private bool IsPlayerOnlyRemainingStructureIsPortal() {
        List<Region> regions = PlayerManager.Instance.player.playerFaction.ownedRegions;
        for (int i = 0; i < regions.Count; i++) {
            Region region = regions[i];
            if (region.mainLandmark != null && region.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_PORTAL) {
                return false;
            }
        }
        return false;
    }
    #endregion
}
