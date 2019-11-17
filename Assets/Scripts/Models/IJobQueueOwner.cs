using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IJobQueueOwner {
    JOB_QUEUE_OWNER ownerType { get; }
    string name { get; }

    void OnAddJob(JobQueueItem job);
    void OnRemoveJob(JobQueueItem job);
}
