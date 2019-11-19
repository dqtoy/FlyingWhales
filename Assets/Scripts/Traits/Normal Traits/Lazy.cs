using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Lazy : Trait {
        public Character owner { get; private set; }

        public Lazy() {
            name = "Lazy";
            description = "Lazy characters often daydream and are less likely to take on settlement tasks.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            
            daysDuration = 0;
            canBeTriggered = true;
            mutuallyExclusive = new string[] { "Hardworking" };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
            }
        }
        public override string TriggerFlaw(Character character) {
            //Will drop current action and will perform Happiness Recovery.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                if (character.currentActionNode.action != null) {
                    character.StopCurrentActionNode(false);
                }
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.currentState.OnExitThisState();
                }

                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait("Heartbroken") as Heartbroken;
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);
                    }
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.ACTOR }, character, character);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        #endregion

        public bool TriggerLazy() {
            if (!owner.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                //JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
                //if (owner.isForlorn) {
                //    jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
                //}
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = owner.traitContainer.GetNormalTrait("Heartbroken") as Heartbroken;
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerBrokenhearted) {
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.ACTOR }, owner, owner);
                    owner.jobQueue.AddJobInQueue(job);

                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "trigger_lazy");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
                return true;
            }
            return false;
        }
    }
}

