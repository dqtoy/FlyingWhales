using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zapped : Trait {

    public Zapped() {
        name = "Zapped";
        description = "This character cannot move.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 3;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            if (character.currentParty.icon.isTravelling) {
                if (character.currentParty.icon.travelLine == null) {
                    character.marker.StopMovement();
                } else {
                    character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                }
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.currentState.OnExitThisState();
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.currentState.OnExitThisState();
                }
            }
            character.CancelAllJobsAndPlans();
            //else if(character.currentAction != null) {
            //    character.currentAction.StopAction();
            //} else if (character.currentParty.icon.isTravelling) {
            //    if (character.currentParty.icon.travelLine == null) {
            //        character.marker.StopMovement();
            //    } else {
            //        character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
            //    }
            //}
            character.AdjustDoNotDisturb(1);
        }
        base.OnAddTrait(sourcePOI);
    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.AdjustDoNotDisturb(-1);

#if TRAILER_BUILD
            if (character.name == "Audrey") {
                Character fiona = CharacterManager.Instance.GetCharacterByName("Fiona");
                Character jamie = CharacterManager.Instance.GetCharacterByName("Jamie");

                if (fiona.currentStructure == fiona.homeStructure) {
                    fiona.PlanIdleStrollOutside(fiona.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                }
                if (jamie.currentStructure == fiona.homeStructure) {
                    jamie.PlanIdleStrollOutside(jamie.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                }

                IPointOfInterest targetTable = fiona.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.TABLE)[0];
                GoapPlanJob job = new GoapPlanJob("Poison Table", INTERACTION_TYPE.TABLE_POISON, targetTable);
                job.SetCannotOverrideJob(true);
                job.SetCannotCancelJob(true);
                job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                character.jobQueue.AddJobInQueue(job, true);
                character.jobQueue.ProcessFirstJobInQueue(character);
            }
#endif
        }
        base.OnRemoveTrait(sourcePOI);
    }
    #endregion
}