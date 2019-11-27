using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour {
    public static JobManager Instance;

    private List<GoapPlanJob> goapJobPool;

    void Awake() {
        Instance = this;
    }

    public void Initialize() {
        ConstructInitialGoapJobPool();
    }

    private void ConstructInitialGoapJobPool() {
        goapJobPool = new List<GoapPlanJob>();
        for (int i = 0; i < 20; i++) {
            goapJobPool.Add(new GoapPlanJob());
        }
    }

    #region Goap Plan Job
    public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, IJobOwner owner) {
        GoapPlanJob job = GetGoapPlanJobFromPool();
        job.Initialize(jobType, goal, targetPOI, owner);
        return job;
    }
    //public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, GoapEffect goal, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData, IJobOwner owner) {
    //    GoapPlanJob job = new GoapPlanJob(jobType, goal, targetPOI, otherData, owner);
    //    job.Initialize(jobType, goal, targetPOI, owner);
    //    return job;
    //}
    public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, IJobOwner owner) {
        GoapPlanJob job = GetGoapPlanJobFromPool();
        job.Initialize(jobType, targetInteractionType, targetPOI, owner);
        return job;
    }
    //public GoapPlanJob CreateNewGoapPlanJob(JOB_TYPE jobType, INTERACTION_TYPE targetInteractionType, IPointOfInterest targetPOI, Dictionary<INTERACTION_TYPE, object[]> otherData, IJobOwner owner) {
    //    GoapPlanJob job = new GoapPlanJob(jobType, targetInteractionType, targetPOI, otherData, owner);
    //    job.Initialize(jobType, goal, targetPOI, owner);
    //    return job;
    //}
    public GoapPlanJob CreateNewGoapPlanJob(SaveDataGoapPlanJob data) {
        GoapPlanJob job = GetGoapPlanJobFromPool();
        job.Initialize(data);
        return job;
    }
    public void OnFinishGoapPlanJob(JobQueueItem job) {
        if(job is GoapPlanJob) {
            ReturnGoapPlanJobToPool(job as GoapPlanJob);
        }
    }
    private GoapPlanJob GetGoapPlanJobFromPool() {
        if(goapJobPool.Count > 0) {
            GoapPlanJob job = goapJobPool[0];
            goapJobPool.RemoveAt(0);
            return job;
        }
        return new GoapPlanJob();
    }
    private void ReturnGoapPlanJobToPool(GoapPlanJob job) {
        job.Reset();
        goapJobPool.Add(job);
    }
    #endregion
}
