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
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
            noItemCharacters = new List<Character>();
            canBeTriggered = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            //(sourceCharacter as Character).RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, "Kleptomania");
            owner = sourceCharacter as Character;
            owner.needsComponent.AdjustHappinessDecreaseRate(_happinessDecreaseRate);
            base.OnAddTrait(sourceCharacter);
            //owner.AddInteractionType(INTERACTION_TYPE.STEAL);
            Messenger.AddListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            base.OnRemoveTrait(sourceCharacter, removedBy);
            //owner.RemoveInteractionType(INTERACTION_TYPE.STEAL);
            owner.needsComponent.AdjustHappinessDecreaseRate(-_happinessDecreaseRate);
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
                if ((token.characterOwner == null || token.characterOwner != characterThatWillDoJob) && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetPOI) && token.mapObjectState == MAP_OBJECT_STATE.BUILT) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL, INTERACTION_TYPE.STEAL, targetPOI, characterThatWillDoJob);
                    job.SetIsStealth(true);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                }
            } else if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (characterThatWillDoJob.opinionComponent.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetCharacter) && targetCharacter.items.Count > 0) {
                    SpecialToken item = targetCharacter.items[Random.Range(0, targetCharacter.items.Count)];
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.STEAL, INTERACTION_TYPE.STEAL, item, characterThatWillDoJob);
                    job.SetIsStealth(true);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        public override string TriggerFlaw(Character character) {
            //The character will begin Hunt for Blood.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);
                    }

                    //This is just a quick fix, need to figure out a way to ensure that the character will steal.
                    List<SpecialToken> choices = new List<SpecialToken>();
                    for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                        Character otherCharacter = character.currentRegion.charactersAtLocation[i];
                        if (otherCharacter == character && character.opinionComponent.GetRelationshipEffectWith(otherCharacter) == RELATIONSHIP_EFFECT.POSITIVE) {
                            continue; //skip
                        }
                        for (int j = 0; j < otherCharacter.items.Count; j++) {
                            SpecialToken currItem = otherCharacter.items[j];
                            choices.Add(currItem);
                        }
                    }
                    if (choices.Count > 0) {
                        IPointOfInterest target = Utilities.GetRandomElement(choices);
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEAL, target, character);
                        character.jobQueue.AddJobInQueue(job);
                    }
                    
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) {
            if (action == INTERACTION_TYPE.STEAL) {
                cost = 0;//Utilities.rng.Next(5, 10);//5,46
            } else if (action == INTERACTION_TYPE.PICK_UP) {
                cost = 10000;//Utilities.rng.Next(5, 10);//5,46
            }
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
            base.ExecuteActionAfterEffects(action, goapNode);
            if (action == INTERACTION_TYPE.STEAL) {
                owner.needsComponent.AdjustHappiness(6000);
            }
        }
        public override void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref List<GoapEffect> effects) {
            if (action == INTERACTION_TYPE.STEAL) {
                effects.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
            }
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

