using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourcePile : TileObject {

	public RESOURCE providedResource { get; protected set; }
    public int resourceInPile { get; protected set; }

    public ResourcePile(RESOURCE providedResource) {
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TAKE_RESOURCE, INTERACTION_TYPE.CARRY, INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE /*, INTERACTION_TYPE.DESTROY_RESOURCE*/ };
        this.providedResource = providedResource;
    }

    #region Virtuals
    public virtual void SetResourceInPile(int amount) {
        resourceInPile = amount;
        resourceInPile = Mathf.Max(0, resourceInPile);
        if(resourceInPile <= 0 && gridTileLocation != null && isBeingCarriedBy == null) {
            gridTileLocation.structure.RemovePOI(this);
        }
    }
    public virtual void AdjustResourceInPile(int adjustment) {
        resourceInPile += adjustment;
        resourceInPile = Mathf.Max(0, resourceInPile);
        if (resourceInPile <= 0) {
            if(gridTileLocation != null && isBeingCarriedBy == null) {
                gridTileLocation.structure.RemovePOI(this);
            } else if (isBeingCarriedBy != null) {
                //If amount in pile was reduced to zero and is still being carried, remove from being carried and destroy it
                if (isBeingCarriedBy.ownParty.IsPOICarried(this)) {
                    isBeingCarriedBy.ownParty.RemoveCarriedPOI(false);
                }
            }
        }
    }
    public virtual bool HasResource() {
        return resourceInPile > 0;
    }
    #endregion

    #region Overrides
    public override void OnPlacePOI() {
        base.OnPlacePOI();
        if(gridTileLocation != null && structureLocation != structureLocation.location.mainStorage) {
            CreateHaulJob();
        }
        Messenger.AddListener<Region>(Signals.REGION_CHANGE_STORAGE, OnRegionChangeStorage);
    }
    public override void OnDestroyPOI() {
        base.OnDestroyPOI();
        Messenger.RemoveListener<Region>(Signals.REGION_CHANGE_STORAGE, OnRegionChangeStorage);
    }
    #endregion

    protected bool IsDepositResourcePileStillApplicable() {
        return gridTileLocation != null && gridTileLocation.structure != gridTileLocation.structure.location.mainStorage;
    }
    protected void OnRegionChangeStorage(Region regionOfMainStorage) {
        if(gridTileLocation != null && structureLocation.location.IsSameCoreLocationAs(regionOfMainStorage) && structureLocation != structureLocation.location.mainStorage) {
            CreateHaulJob();
        }
    }
    protected void CreateHaulJob() {
        if (!structureLocation.areaLocation.HasJob(JOB_TYPE.HAUL, this)) {
            ResourcePile chosenPileToBeDeposited = structureLocation.location.mainStorage.GetResourcePileObjectWithLowestCount(tileObjectType);
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty, false, GOAP_EFFECT_TARGET.TARGET), this, structureLocation.areaLocation);
            if (chosenPileToBeDeposited != null) {
                job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new object[] { chosenPileToBeDeposited });
            }
            job.SetStillApplicableChecker(IsDepositResourcePileStillApplicable);
            job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
            structureLocation.areaLocation.AddToAvailableJobs(job);
        }
    }
    protected void ForceCancelNotAssignedProduceJob(JOB_TYPE jobType) {
        JobQueueItem job = structureLocation.areaLocation.GetJob(jobType);
        if (job != null && job.assignedCharacter == null) {
            structureLocation.areaLocation.ForceCancelJob(job);
        }
    }
}
