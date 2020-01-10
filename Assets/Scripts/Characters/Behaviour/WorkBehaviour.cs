using UnityEngine;
using Traits;

public class WorkBehaviour : CharacterBehaviourComponent {
    public WorkBehaviour() {
        attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.INSIDE_SETTLEMENT_ONLY };
    }
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} is going to work or will do needs recovery...";
        if (!PlanJobQueueFirst(character)) {
            if (!character.needsComponent.PlanFullnessRecoveryActions(character)) {
                if (!character.needsComponent.PlanTirednessRecoveryActions(character)) {
                    if (!character.needsComponent.PlanHappinessRecoveryActions(character)) {
                        if (!PlanWorkActions(character)) {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }
    
    private bool PlanJobQueueFirst(Character character) {
        if (!character.needsComponent.isStarving && !character.needsComponent.isExhausted && !character.needsComponent.isForlorn) {
            return PlanWorkActions(character);
        }
        return false;
    }
    private bool PlanWorkActions(Character character) {
        if (character.isAtHomeRegion && character.homeSettlement != null && character.isPartOfHomeFaction) { //&& this.faction.id != FactionManager.Instance.neutralFaction.id
            bool triggerLazy = false;
            Lazy lazy = character.traitContainer.GetNormalTrait<Trait>("Lazy") as Lazy;
            if (lazy != null) {
                triggerLazy = Random.Range(0, 100) < 35;
            }
            if (triggerLazy) {
                if (lazy.TriggerLazy()) {
                    return true;
                } else {
                    character.PrintLogIfActive($"{GameManager.Instance.TodayLogString()}Triggered LAZY happiness recovery but {character.name} already has that job type in queue and will not do it anymore!");
                }
            }
            //check settlement job queue, if it has any jobs that target an object that is in view of the character
            bool assignedFromVision = character.homeSettlement.AssignCharacterToJobBasedOnVision(character);
            if (assignedFromVision == false) {
                //if there are none, check the characters faction job queue under the same conditions.
                if (character.faction?.activeQuest != null) {
                    assignedFromVision = character.faction.activeQuest.AssignCharacterToJobBasedOnVision(character);
                }
            }
            if (assignedFromVision) {
                //took job based from vision
                return true;
            } else {
                //if none of the jobs targets can be seen by the character, try and get a job from the settlement or faction
                //regardless of vision instead.
                if (!character.homeSettlement.AddFirstUnassignedJobToCharacterJob(character)) {
                    if (character.faction?.activeQuest != null) {
                        return character.faction.activeQuest.AddFirstUnassignedJobToCharacterJob(character);
                    }
                    return false;
                } else {
                    return true;
                }    
            }
        } else {
            return false;
        }
    }
}
