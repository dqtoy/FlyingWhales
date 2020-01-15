using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Agoraphobic : Trait {

        public Agoraphobic() {
            name = "Agoraphobic";
            description = "Agoraphobics avoid crowds.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            ticksDuration = 0;
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        //protected override void OnChangeLevel() {
        //    base.OnChangeLevel();
        //if(level == 1) {
        //    daysDuration = 50;
        //} else if (level == 2) {
        //    daysDuration = 70;
        //} else if (level == 3) {
        //    daysDuration = 90;
        //}
        //}
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                ApplyAgoraphobicEffect(character);
                //if (character.marker.inVisionCharacters.Count >= 3) {
                //    ApplyAgoraphobicEffect(character, true);
                //}
            }
        }
        public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
            base.OnSeePOI(targetPOI, character);
            if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //Character targetCharacter = targetPOI as Character;
                if (character.traitContainer.GetNormalTrait<Trait>("Berserked") != null) {
                    return;
                }
                ApplyAgoraphobicEffect(character);
                //if (!character.isInCombat) {
                //    if (character.marker.inVisionCharacters.Count >= 3) {
                //        ApplyAgoraphobicEffect(character, true);
                //    }
                //} else {
                //    CombatState combatState = character.stateComponent.currentState as CombatState;
                //    if (combatState.isAttacking) {
                //        if (character.marker.inVisionCharacters.Count >= 3) {
                //            ApplyAgoraphobicEffect(character, false);
                //            Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, character, "agoraphobia");
                //        }
                //    }
                //}
            }
        }
        public override string TriggerFlaw(Character character) {
            //If outside and the character lives in a house, the character will flee and go back home.
            //string successLogKey = base.TriggerFlaw(character);
            //if (character.homeStructure != null) {
            //    if (character.currentStructure != character.homeStructure) {
            //        if (character.currentActionNode != null) {
            //            character.StopCurrentActionNode(false);
            //        }
            //        if (character.stateComponent.currentState != null) {
            //            character.stateComponent.ExitCurrentState();
            //        }
            //        ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], character, character, null, 0);
            //        GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, character);
            //        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.RETURN_HOME, character, character);
            //        goapPlan.SetDoNotRecalculate(true);
            //        job.SetCannotBePushedBack(true);
            //        job.SetAssignedPlan(goapPlan);
            //        character.jobQueue.AddJobInQueue(job);
            //        return successLogKey;
            //    } else {
            //        return "fail_at_home";
            //    }
            //} else {
            //    return "fail_no_home";
            //}
            ApplyAgoraphobicEffect(character);
            return base.TriggerFlaw(character);

        }
        #endregion

        private void ApplyAgoraphobicEffect(Character character/*, bool processCombat*/) {
            if (!character.canPerform || !character.canWitness) {
                return;
            }
            if(character.marker.inVisionCharacters.Count < 3) {
                return;
            }
            character.traitContainer.AddTrait(character, "Anxious");
            if(character.homeStructure != null && character.currentStructure != character.homeStructure) {
                ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.RETURN_HOME], character, character, null, 0);
                GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, character);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.RETURN_HOME, character, character);
                goapPlan.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                job.SetAssignedPlan(goapPlan);
                character.jobQueue.AddJobInQueue(job);
            } else {
                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character);
            }
            //character.marker.AddAvoidsInRange(character.marker.inVisionCharacters, processCombat, "agoraphobia");
            //character.needsComponent.AdjustHappiness(-50);
            //character.needsComponent.AdjustTiredness(-150);
        }
    }
}

