using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourcePile : TileObject {

	public RESOURCE providedResource { get; protected set; }
    public int resourceInPile { get; protected set; }

    public ResourcePile(RESOURCE providedResource) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.OBTAIN_RESOURCE, INTERACTION_TYPE.CARRY, INTERACTION_TYPE.DROP_RESOURCE /*, INTERACTION_TYPE.DESTROY_RESOURCE*/ };
        this.providedResource = providedResource;
    }

    #region Virtuals
    public virtual void SetResourceInPile(int amount) {
        resourceInPile = amount;
        resourceInPile = Mathf.Max(0, resourceInPile);
    }
    public virtual void AdjustResourceInPile(int adjustment) {
        resourceInPile += adjustment;
        resourceInPile = Mathf.Max(0, resourceInPile);
    }
    public virtual bool HasResource() {
        return resourceInPile > 0;
    }
    #endregion

    protected bool IsDepositResourcePileStillApplicable() {
        return gridTileLocation != null && gridTileLocation.structure != gridTileLocation.structure.location.mainStorage;
    }
    protected void CreateHaulJob() {
        if (!structureLocation.location.HasJob(JOB_TYPE.HAUL, this)) {
            ResourcePile chosenPileToBeDeposited = structureLocation.location.mainStorage.GetResourcePileObjectWithLowestCount(tileObjectType);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), this, structureLocation.location);
            if (chosenPileToBeDeposited != null) {
                job.AddOtherData(INTERACTION_TYPE.DROP_RESOURCE, new object[] { chosenPileToBeDeposited });
            }
            job.SetStillApplicableChecker(IsDepositResourcePileStillApplicable);
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
            structureLocation.location.AddToAvailableJobs(job);
        }
    }
}
