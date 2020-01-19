using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class SettlementJobTriggerComponent : JobTriggerComponent {

	private readonly Settlement _owner;

	private const int MinimumFood = 100;
	private const int MinimumMetal = 100;
	private const int MinimumStone = 100;
	private const int MinimumWood = 100;
	
	public SettlementJobTriggerComponent(Settlement owner) {
		_owner = owner;
	}
	
	#region Listeners
	public void SubscribeToListeners() {
		Messenger.AddListener<ResourcePile>(Signals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChanged);
		Messenger.AddListener<IPointOfInterest>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
		Messenger.AddListener<IPointOfInterest>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
		Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
	}
	public void UnsubscribeListeners() {
		Messenger.RemoveListener<ResourcePile>(Signals.RESOURCE_IN_PILE_CHANGED, OnResourceInPileChanged);
		Messenger.RemoveListener<IPointOfInterest>(Signals.OBJECT_DAMAGED, OnObjectDamaged);
		Messenger.RemoveListener<IPointOfInterest>(Signals.OBJECT_REPAIRED, OnObjectRepaired);
		Messenger.RemoveListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
		Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
		Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
	}
	private void OnResourceInPileChanged(ResourcePile resourcePile) {
		if (resourcePile.gridTileLocation != null && resourcePile.structureLocation == _owner.mainStorage) {
			CheckResource(resourcePile.tileObjectType, resourcePile.providedResource);
		}
	}
	private void OnObjectDamaged(IPointOfInterest poi) {
		Assert.IsTrue(poi is TileObject || poi is SpecialToken);
		if (poi.gridTileLocation != null && poi.gridTileLocation.IsPartOfSettlement(_owner)) {
			TryCreateRepairJob(poi);
		}
	}
	private void OnObjectRepaired(IPointOfInterest poi) {
		Assert.IsTrue(poi is TileObject || poi is SpecialToken);
		if (poi.gridTileLocation != null && poi.gridTileLocation.IsPartOfSettlement(_owner)) {
			//cancel existing repair job
			Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.REPAIR, poi);
		}
	}
	private void OnTileObjectPlaced(TileObject tileObject, LocationGridTile tile) {
		if (tileObject is ResourcePile) {
			if (tile.IsPartOfSettlement()) {
				Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.HAUL, tileObject as IPointOfInterest);
			} else {
				TryCreateHaulJob(tileObject as ResourcePile);	
			}
			
		}
	}
	private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
		if (structure.settlementLocation == _owner) {
			if (structure.structureType == STRUCTURE_TYPE.PRISON) {
				TryCreateJudgePrisoner(character);
			}
		}
	}
	private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
		if (traitable is Character) {
			Character target = traitable as Character;
			if (trait is Restrained) {
				TryCreateJudgePrisoner(target);
			}
		}
	}
	#endregion

	#region Resources
	private int GetTotalResource(RESOURCE resourceType) {
		int resource = 0;
		List<ResourcePile> piles = _owner.mainStorage.GetTileObjectsOfType<ResourcePile>();
		for (int i = 0; i < piles.Count; i++) {
			ResourcePile resourcePile = piles[i];
			if (resourcePile.providedResource == resourceType) {
				resource += piles[i].resourceInPile;	
			}
		}
		return resource;
	}
	private int GetMinimumResource(RESOURCE resource) {
		switch (resource) {
			case RESOURCE.FOOD:
				return MinimumFood;
			case RESOURCE.WOOD:
				return MinimumWood;
			case RESOURCE.METAL:
				return MinimumMetal;
			case RESOURCE.STONE:
				return MinimumStone;
		}
		throw new Exception($"There is no minimum resource for {resource.ToString()}");
	}
	private JOB_TYPE GetProduceResourceJobType(RESOURCE resource) {
		switch (resource) {
			case RESOURCE.FOOD:
				return JOB_TYPE.PRODUCE_FOOD;
			case RESOURCE.WOOD:
				return JOB_TYPE.PRODUCE_WOOD;
			case RESOURCE.METAL:
				return JOB_TYPE.PRODUCE_METAL;
			case RESOURCE.STONE:
				return JOB_TYPE.PRODUCE_STONE;
		}
		throw new Exception($"There is no produce resource job type for {resource.ToString()}");
	}
	private GOAP_EFFECT_CONDITION GetProduceResourceGoapEffect(RESOURCE resource) {
		switch (resource) {
			case RESOURCE.FOOD:
				return GOAP_EFFECT_CONDITION.PRODUCE_FOOD;
			case RESOURCE.WOOD:
				return GOAP_EFFECT_CONDITION.PRODUCE_WOOD;
			case RESOURCE.METAL:
				return GOAP_EFFECT_CONDITION.PRODUCE_METAL;
			case RESOURCE.STONE:
				return GOAP_EFFECT_CONDITION.PRODUCE_STONE;
		}
		throw new Exception($"There is no produce resource goap effect type for {resource.ToString()}");
	}
	
	private void CheckResource(TILE_OBJECT_TYPE resourcePile, RESOURCE resource) {
		int totalResource = GetTotalResource(resource);
		int minimumResource = GetMinimumResource(resource);
		JOB_TYPE jobType = GetProduceResourceJobType(resource);
		if (totalResource < minimumResource) {
			TriggerProduceResource(resource, resourcePile, jobType);
		} else {
			ResourcePile pile = _owner.mainStorage.GetTileObjectOfType<ResourcePile>(resourcePile);
			Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, jobType, pile as IPointOfInterest);
		}
	}
	private void TriggerProduceResource(RESOURCE resourceType, TILE_OBJECT_TYPE resourcePile, JOB_TYPE jobType) {
		if (_owner.HasJob(jobType) == false) {
			ResourcePile foodPile = _owner.mainStorage.GetTileObjectOfType<ResourcePile>(resourcePile);
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, new GoapEffect(
				GetProduceResourceGoapEffect(resourceType), string.Empty, 
				false, GOAP_EFFECT_TARGET.ACTOR), foodPile, _owner);
			job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
			job.SetStillApplicableChecker(() => IsProduceResourceJobStillValid(resourceType));
			_owner.AddToAvailableJobs(job);	
		}
	}
	private bool IsProduceResourceJobStillValid(RESOURCE resource) {
		return GetTotalResource(resource) < GetMinimumResource(resource);
	}
	#endregion

	#region Repair
	private void TryCreateRepairJob(IPointOfInterest target) {
		if (_owner.HasJob(JOB_TYPE.REPAIR, target) == false) {
			GoapPlanJob job =
				JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR, target, _owner);
			job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRepairJob);
			job.SetStillApplicableChecker(() => IsRepairJobStillValid(target));
			// job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE,
			// 	new object[] {(int) (TileObjectDB.GetTileObjectData(target.tileObjectType).constructionCost * 0.5f)});
			_owner.AddToAvailableJobs(job);
		}
	}
	private bool IsRepairJobStillValid(IPointOfInterest target) {
		return target.currentHP < target.maxHP && target.gridTileLocation != null 
		                                       && target.gridTileLocation.IsPartOfSettlement(_owner);
	}
	#endregion

	#region Haul
	private void TryCreateHaulJob(ResourcePile target) {
		if (target.gridTileLocation.IsPartOfSettlement(_owner) == false 
		    && _owner.HasJob(JOB_TYPE.HAUL, target) == false) {
			ResourcePile chosenPileToBeDeposited = _owner.mainStorage.GetResourcePileObjectWithLowestCount(target.tileObjectType);
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAUL, 
					new GoapEffect(GOAP_EFFECT_CONDITION.DEPOSIT_RESOURCE, string.Empty,
						false, GOAP_EFFECT_TARGET.TARGET), target, _owner);
				if (chosenPileToBeDeposited != null) {
					job.AddOtherData(INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, 
						new object[] { chosenPileToBeDeposited });
				}
				job.SetStillApplicableChecker(() => IsDepositResourcePileStillApplicable(target));
				job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoObtainSupplyJob);
				_owner.AddToAvailableJobs(job);
		}
	}
	private bool IsDepositResourcePileStillApplicable(ResourcePile resourcePile) {
		return resourcePile.gridTileLocation != null 
		       && resourcePile.gridTileLocation.structure != resourcePile.gridTileLocation.structure.location.mainStorage;
	}
	#endregion

	#region Judge Prisoner
	private void TryCreateJudgePrisoner(Character target) {
		if (target.traitContainer.GetNormalTrait<Trait>("Restrained") != null 
		    && target.currentStructure.structureType == STRUCTURE_TYPE.PRISON
		    && target.currentStructure.settlementLocation == _owner) {
			if (!target.HasJobTargetingThis(JOB_TYPE.JUDGE_PRISONER)) {
				GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.JUDGE_PRISONER, INTERACTION_TYPE.JUDGE_CHARACTER, target, _owner);
				job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoJudgementJob);
				job.SetStillApplicableChecker(() => InteractionManager.Instance.IsJudgementJobStillApplicable(target));
				_owner.AddToAvailableJobs(job);
			}
		}
	}
	#endregion
}
