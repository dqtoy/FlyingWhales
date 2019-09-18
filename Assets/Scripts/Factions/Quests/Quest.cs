using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest {
    public string name { get; protected set; }
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
