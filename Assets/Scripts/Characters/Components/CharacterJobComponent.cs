using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;

public class CharacterJobComponent : JobTriggerComponent {

	private Character _owner;
	
	private bool hasStartedScreamCheck;

	private string[] removeStatusTraits = new[] {
		nameof(Unconscious), nameof(Injured), nameof(Sick), nameof(Plagued),
		nameof(Infected), nameof(Cursed)
	};
	private string[] specialIllnessTraits = new[] {
		nameof(Sick), nameof(Plagued), nameof(Infected)
	};
	
	public CharacterJobComponent(Character owner) {
		_owner = owner;
	}

	#region Listeners
	public void SubscribeToListeners() {
		Messenger.AddListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		Messenger.AddListener<Character>(Signals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
		Messenger.AddListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
		Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.AddListener<Settlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
	}
	public void UnsubscribeListeners() {
		Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
		Messenger.RemoveListener<Character>(Signals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
		Messenger.RemoveListener<Character, GoapPlanJob>(Signals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedJob);
		Messenger.RemoveListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
		Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, OnTraitableLostTrait);
		Messenger.RemoveListener<Settlement, bool>(Signals.SETTLEMENT_UNDER_SIEGE_STATE_CHANGED, OnSettlementUnderSiegeChanged);
		Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
		Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
	}
	private void OnCharacterCanNoLongerMove(Character character) {
		if (character == _owner) {
			TryStartScreamCheck();
			TryTriggerRestrain();
		}
	}
	private void OnCharacterCanMoveAgain(Character character) {
		if (character == _owner) {
			Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RESTRAIN, _owner as IPointOfInterest);
			TryStopScreamCheck();
		}
	}
	private void OnCharacterFinishedJob(Character character, GoapPlanJob job) {
		if (character == _owner && job.jobType == JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM) {
			TriggerBurySerialKillerVictim(job);
		}
	}
	private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
		if (traitable == _owner) {
			if (removeStatusTraits.Contains(trait.name)) {
				TryCreateRemoveStatusJob(trait);
			}
			TryStartScreamCheck();
		}
	}
	private void OnTraitableLostTrait(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner) {
			TryStopScreamCheck();
			if (removeStatusTraits.Contains(nameof(trait))) {
				_owner.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_STATUS, trait.name, removedBy); //so that the character that cured him will not cancel his job.
			}
		}
	}
	private void OnSettlementUnderSiegeChanged(Settlement settlement, bool siegeState) {
		if (settlement == _owner.currentSettlement && siegeState) {
			//characters current settlement is under siege
			_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Stopped, _owner);
			Messenger.AddListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfStopInterruptFinished);
		}
	}
	private void OnCharacterEnteredHexTile(Character character, HexTile tile) {
		if (character == _owner) {
			TryCreateRemoveStatusJob();
		}
	}
	private void OnCharacterExitedHexTile(Character character, HexTile tile) {
		if (character == _owner) {
			Messenger.Broadcast(Signals.CHECK_JOB_APPLICABILITY, JOB_TYPE.RESTRAIN, _owner as IPointOfInterest);
		}
	}
	#endregion

	#region Job Triggers
	private void TriggerScreamJob() {
		if (_owner.jobQueue.HasJob(JOB_TYPE.SCREAM) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SCREAM, INTERACTION_TYPE.SCREAM_FOR_HELP, _owner, _owner);
			_owner.jobQueue.AddJobInQueue(job);
		}
	}
	private void TriggerBurySerialKillerVictim(GoapPlanJob huntJob) {
		IPointOfInterest target = huntJob.targetPOI;
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.BURY_SERIAL_KILLER_VICTIM,
			INTERACTION_TYPE.BURY_CHARACTER, target, _owner);
		job.AddOtherData(INTERACTION_TYPE.BURY_CHARACTER, new object[] {
			_owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS)
		});
		_owner.jobQueue.AddJobInQueue(job);
	}
	public void TriggerFleeHome(JOB_TYPE jobType = JOB_TYPE.FLEE_TO_HOME) {
		ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], _owner, _owner, null, 0);
		GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, _owner);
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.RETURN_HOME, _owner, _owner);
		goapPlan.SetDoNotRecalculate(true);
		job.SetCannotBePushedBack(true);
		job.SetAssignedPlan(goapPlan);
		_owner.jobQueue.AddJobInQueue(job);
	}
	public bool TriggerDestroy(IPointOfInterest target) {
		if (!_owner.jobQueue.HasJob(JOB_TYPE.DESTROY, target)) {
			GoapPlanJob destroyJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.RESOLVE_COMBAT, target, _owner);
			destroyJob.SetStillApplicableChecker(() => IsDestroyJobApplicable(target));
			_owner.jobQueue.AddJobInQueue(destroyJob);
			return true;
		}
		return false;
	}
	private void TriggerRemoveStatus(Trait trait) {
		GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = trait.name, target = GOAP_EFFECT_TARGET.TARGET };
		if (_owner.homeSettlement.HasJob(goapEffect, _owner) == false) {
			GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_STATUS, goapEffect, _owner, _owner.homeSettlement);
			job.SetCanTakeThisJobChecker((Character character, JobQueueItem jqi) => CanTakeRemoveStatus(character, job, trait));
			job.SetStillApplicableChecker(() => IsRemoveStatusJobStillApplicable(_owner, job));
			job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION });
			job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.HEALING_POTION].craftCost });
			_owner.homeSettlement.AddToAvailableJobs(job);
		}
	}
	private void TriggerFeed(Character target) {
		GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, target = GOAP_EFFECT_TARGET.TARGET };
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FEED, goapEffect, target, _owner);
		job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { 20 });
		_owner.jobQueue.AddJobInQueue(job);
	}
	private void TriggerRestrain(Settlement settlement) {
		GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESTRAIN, INTERACTION_TYPE.RESTRAIN_CHARACTER, _owner, settlement);
		job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRestrainJob);
		job.SetStillApplicableChecker(() => IsRestrainApplicable(_owner, settlement));
		settlement.AddToAvailableJobs(job);
	}
	#endregion

	#region Applicability Checkers
	private bool IsDestroyJobApplicable(IPointOfInterest target) {
		return target.gridTileLocation != null;
	}
	private bool IsRemoveStatusJobStillApplicable(Character target, GoapPlanJob job) {
		if (target.gridTileLocation == null || target.isDead) {
			return false;
		}
		if (target.gridTileLocation.IsNextToOrPartOfSettlement(job.originalOwner as Settlement) == false) {
			return false;
		}
		if (target.isCriminal) {
			return false;
		}
		return true;
	}
	private bool IsRestrainApplicable(Character target, Settlement settlement) {
		return target.canMove == false && target.gridTileLocation != null &&
		       target.gridTileLocation.IsNextToOrPartOfSettlement(settlement);
	}
	#endregion

	#region Job Checkers
	private bool CanTakeRemoveStatus(Character character, JobQueueItem job, Trait trait) {
		if (job is GoapPlanJob) {
			GoapPlanJob goapPlanJob = job as GoapPlanJob;
			Character targetCharacter = goapPlanJob.targetPOI as Character;
			if (character != targetCharacter) {
				bool isHostile = character.IsHostileWith(targetCharacter, false);
				bool isResponsibleForTrait = trait.IsResponsibleForTrait(character);

				//if special illness, check if character is healer
				if (specialIllnessTraits.Contains(nameof(trait))) {
					return isHostile == false &&
					       character.opinionComponent.HasOpinionLabelWithCharacter(targetCharacter,
						       OpinionComponent.Rival, OpinionComponent.Enemy) == false 
					       && isResponsibleForTrait == false 
						&& character.traitContainer.GetNormalTrait<Trait>("Healer") != null;	
				}
				
				return isHostile == false &&
				       character.opinionComponent.HasOpinionLabelWithCharacter(targetCharacter,
					       OpinionComponent.Rival, OpinionComponent.Enemy) == false 
				       && isResponsibleForTrait == false;
			}
		}
		return false;
	}
	#endregion
	
	#region Scream
	private void TryStartScreamCheck() {
		if (hasStartedScreamCheck) {
			return;
		}
		if ((_owner.canMove == false && 
		     _owner.traitContainer.GetNormalTrait<Trait>("Exhausted", "Starving", "Depressed") != null)
		    || (_owner.traitContainer.GetNormalTrait<Trait>("Restrained") != null && _owner.currentStructure.structureType != STRUCTURE_TYPE.PRISON)) {
			hasStartedScreamCheck = true;
			Messenger.AddListener(Signals.HOUR_STARTED, HourlyScreamCheck);
		}
	}
	private void TryStopScreamCheck() {
		if (hasStartedScreamCheck == false) {
			return;
		}
		if (_owner.canMove || 
		    (_owner.traitContainer.GetNormalTrait<Trait>("Restrained") != null &&
		     _owner.currentStructure.structureType == STRUCTURE_TYPE.PRISON)) {
			hasStartedScreamCheck = false;
			Messenger.RemoveListener(Signals.HOUR_STARTED, HourlyScreamCheck);
		}
	}
	private void HourlyScreamCheck() {
		int chance = 50;
		if (_owner.canMove == false && 
		    _owner.traitContainer.GetNormalTrait<Trait>("Exhausted", "Starving", "Depressed") != null) {
			chance = 75;
		}
		if (Random.Range(0,100) < chance) {
			TriggerScreamJob();
		}
	}
	#endregion

	#region Flee to home
	private void CheckIfStopInterruptFinished(INTERRUPT interrupt, Character character) {
		if (character == _owner && interrupt == INTERRUPT.Stopped) {
			Messenger.RemoveListener<INTERRUPT, Character>(Signals.INTERRUPT_FINISHED, CheckIfStopInterruptFinished);
			TriggerFleeHome();
		}
	}
	#endregion

	#region Remove Status
	private void TryCreateRemoveStatusJob(Trait trait) {
		if (_owner.homeSettlement != null && _owner.homeSettlement == _owner.currentSettlement 
		    && _owner.isCriminal == false) {
			TriggerRemoveStatus(trait);
		}
	}
	private void TryCreateRemoveStatusJob() {
		if (_owner.homeSettlement != null && _owner.homeSettlement == _owner.currentSettlement
		    && _owner.isCriminal == false) {
			List<Trait> statusTraits = _owner.traitContainer.GetNormalTraits<Trait>(this.removeStatusTraits);
			for (int i = 0; i < statusTraits.Count; i++) {
				Trait trait = statusTraits[i];
				TryCreateRemoveStatusJob(trait);
			}
		}
	}
	#endregion

	#region Feed
	public void TryTriggerFeed(Character targetCharacter) {
		if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Hungry", "Starving") != null
		    && (_owner.homeSettlement == targetCharacter.homeSettlement || _owner.faction == targetCharacter.faction)
		    && _owner.IsHostileWith(targetCharacter, false) == false 
		    && _owner.opinionComponent.HasOpinionLabelWithCharacter(targetCharacter,
			    OpinionComponent.Rival, OpinionComponent.Enemy) == false
		    && _owner.jobQueue.HasJob(JOB_TYPE.FEED, targetCharacter) == false) {
			TriggerFeed(targetCharacter);
		}
	}
	#endregion

	#region Restrain
	private void TryTriggerRestrain() {
		Settlement nearSettlement;
		if (_owner.gridTileLocation.IsPartOfSettlement(out nearSettlement) 
		    || _owner.gridTileLocation.IsNextToSettlement(out nearSettlement)) {
			if (_owner.faction != nearSettlement.owner) {
				bool isHostileWithFaction =
					_owner.faction.GetRelationshipWith(nearSettlement.owner).relationshipStatus ==
					FACTION_RELATIONSHIP_STATUS.HOSTILE;
				if (isHostileWithFaction) {
					TriggerRestrain(nearSettlement);
				}
			}	
		}
		
	}
	#endregion
}
