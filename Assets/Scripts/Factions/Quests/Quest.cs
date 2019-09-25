using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest {
    public string name { get; protected set; }
    public string description { get; protected set; }
    public Faction factionOwner { get; protected set; }
    public Region region { get; protected set; }
    public JobQueue jobQueue { get; protected set; }
    public bool isActivated { get; protected set; }

    public Quest(Faction factionOwner, Region region) {
        this.factionOwner = factionOwner;
        this.region = region;
        name = "Quest";
        jobQueue = new JobQueue(null);
    }
    public Quest(SaveDataQuest data) {
        name = data.name;
        description = data.description;
        region = GridMap.Instance.GetRegionByID(data.regionID);
        factionOwner = FactionManager.Instance.GetFactionBasedOnID(data.factionOwnerID);
        jobQueue = new JobQueue(null);
    }

    #region Virtuals
    //On its own, when quest is made, it is still not active, this must be called to activate quest
    public virtual void ActivateQuest() {
        isActivated = true;
    }
    public virtual void FinishQuest() {
        isActivated = false;
    }
    #endregion
}

[System.Serializable]
public class SaveDataQuest {
    public string name;
    public string description;
    public int factionOwnerID;
    public int regionID;
    public bool isActivated;
    public List<SaveDataJobQueueItem> jobs;

    public virtual void Save(Quest quest) {
        name = quest.name;
        description = quest.description;
        factionOwnerID = quest.factionOwner.id;
        regionID = quest.region.id;
        isActivated = quest.isActivated;


        jobs = new List<SaveDataJobQueueItem>();
        for (int i = 0; i < quest.jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = quest.jobQueue.jobsInQueue[i];
            if (job.isNotSavable) {
                continue;
            }
            //SaveDataJobQueueItem data = System.Activator.CreateInstance(System.Type.GetType("SaveData" + job.GetType().ToString())) as SaveDataJobQueueItem;
            SaveDataJobQueueItem data = null;
            if (job is GoapPlanJob) {
                data = new SaveDataGoapPlanJob();
            } else if (job is CharacterStateJob) {
                data = new SaveDataCharacterStateJob();
            }
            data.Save(job);
            jobs.Add(data);
        }
    }

    public virtual Quest Load() {
        string noSpacesName = Utilities.RemoveAllWhiteSpace(name);
        Quest quest = System.Activator.CreateInstance(System.Type.GetType(noSpacesName), this) as Quest;
        for (int i = 0; i < jobs.Count; i++) {
            JobQueueItem job = jobs[i].Load();
            quest.jobQueue.AddJobInQueue(job, false);
        }
        return quest;
    }
}
