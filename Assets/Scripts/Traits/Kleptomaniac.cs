using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Kleptomaniac : Trait {
        public List<Character> noItemCharacters { get; private set; }
        private Character owner;

        private int _happinessDecreaseRate;
        public Kleptomaniac() {
            name = "Kleptomaniac";
            description = "Kleptomaniacs enjoy stealing.";
            thoughtText = "[Character] has irresistible urge to steal.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
            associatedInteraction = INTERACTION_TYPE.NONE;
            crimeSeverity = CRIME_CATEGORY.NONE;
            daysDuration = 0;
            //effects = new List<TraitEffect>();
            noItemCharacters = new List<Character>();
            canBeTriggered = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            //(sourceCharacter as Character).RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, "Kleptomania");
            owner = sourceCharacter as Character;
            owner.AdjustHappinessDecreaseRate(_happinessDecreaseRate);
            base.OnAddTrait(sourceCharacter);
            owner.AddInteractionType(INTERACTION_TYPE.STEAL_FROM_CHARACTER);
            owner.AddInteractionType(INTERACTION_TYPE.STEAL);
            Messenger.AddListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            base.OnRemoveTrait(sourceCharacter, removedBy);
            owner.RemoveInteractionType(INTERACTION_TYPE.STEAL_FROM_CHARACTER);
            owner.RemoveInteractionType(INTERACTION_TYPE.STEAL);
            owner.AdjustHappinessDecreaseRate(-_happinessDecreaseRate);
            Messenger.RemoveListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
        }
        public override void OnDeath(Character character) {
            base.OnDeath(character);
            Messenger.RemoveListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
        }
        public override void OnReturnToLife(Character character) {
            base.OnReturnToLife(character);
            Messenger.AddListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
        }
        public override string GetTestingData() {
            string testingData = string.Empty;
            testingData += "Known character'S with no items: \n";
            for (int i = 0; i < noItemCharacters.Count; i++) {
                testingData += noItemCharacters[i].name + ", ";
            }
            return testingData;
        }
        protected override void OnChangeLevel() {
            base.OnChangeLevel();
            if (level == 1) {
                _happinessDecreaseRate = 10;
            } else if (level == 2) {
                _happinessDecreaseRate = 15;
            } else if (level == 3) {
                _happinessDecreaseRate = 20;
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is SpecialToken) {
                SpecialToken token = targetPOI as SpecialToken;
                if (characterThatWillDoJob.currentAction != null && characterThatWillDoJob.currentAction.goapType == INTERACTION_TYPE.ROAMING_TO_STEAL && !characterThatWillDoJob.currentAction.isDone) {
                    if ((token.characterOwner == null || token.characterOwner != characterThatWillDoJob) && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetPOI)) {
                        GoapPlanJob job = new GoapPlanJob(characterThatWillDoJob.currentAction.parentPlan.job.jobType, INTERACTION_TYPE.STEAL, targetPOI);
                        job.SetIsStealth(true);
                        characterThatWillDoJob.currentAction.parentPlan.job.jobQueueParent.CancelJob(characterThatWillDoJob.currentAction.parentPlan.job);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job, false);
                        characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(job, characterThatWillDoJob);
                        return true;
                    }
                }
            } else if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (characterThatWillDoJob.currentAction != null && characterThatWillDoJob.currentAction.goapType == INTERACTION_TYPE.ROAMING_TO_STEAL && !characterThatWillDoJob.currentAction.isDone) {
                    if (characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.POSITIVE && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetCharacter)) {
                        GoapPlanJob job = new GoapPlanJob(characterThatWillDoJob.currentAction.parentPlan.job.jobType, INTERACTION_TYPE.STEAL_FROM_CHARACTER, targetCharacter);
                        job.SetIsStealth(true);
                        characterThatWillDoJob.currentAction.parentPlan.job.jobQueueParent.CancelJob(characterThatWillDoJob.currentAction.parentPlan.job);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job, false);
                        characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(job, characterThatWillDoJob);
                        return true;
                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        public override string TriggerFlaw(Character character) {
            //The character will begin Hunt for Blood.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait("Heartbroken") as Heartbroken;
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);
                    }
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = character });
                    job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = character }, INTERACTION_TYPE.ROAMING_TO_STEAL);
                    job.SetCancelOnFail(true);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        #endregion

        public void AddNoItemCharacter(Character character) {
            noItemCharacters.Add(character);
        }
        public void RemoveNoItemCharacter(Character character) {
            noItemCharacters.Remove(character);
        }

        private void ClearNoItemsList() {
            noItemCharacters.Clear();
            //Debug.Log(GameManager.Instance.TodayLogString() + "Cleared " + owner.name + "'s Kleptomaniac list of character's with no items.");
        }

        private void CheckForClearNoItemsList() {
            //Store the character into the Kleptomaniac trait if it does not have any items. 
            //Exclude all characters listed in Kleptomaniac trait from Steal actions. Clear out the list at the start of every even day.
            if (Utilities.IsEven(GameManager.days)) {
                ClearNoItemsList();
            }
        }
    }

    public class SaveDataKleptomaniac : SaveDataTrait {
        public List<int> noItemCharacterIDs;

        public override void Save(Trait trait) {
            base.Save(trait);
            Kleptomaniac derivedTrait = trait as Kleptomaniac;
            for (int i = 0; i < derivedTrait.noItemCharacters.Count; i++) {
                noItemCharacterIDs.Add(derivedTrait.noItemCharacters[i].id);
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Kleptomaniac derivedTrait = trait as Kleptomaniac;
            for (int i = 0; i < noItemCharacterIDs.Count; i++) {
                derivedTrait.AddNoItemCharacter(CharacterManager.Instance.GetCharacterByID(noItemCharacterIDs[i]));
            }
            return trait;
        }
    }
}

