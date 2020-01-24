using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Pyrophobic : Trait {

        private Character owner;
        private List<BurningSource> seenBurningSources;

        public Pyrophobic() {
            name = "Pyrophobic";
            description = "Pyrophobics are afraid of fires.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            seenBurningSources = new List<BurningSource>();
        }

        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
            }
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            Burning burning = targetPOI.traitContainer.GetNormalTrait<Burning>("Burning");
            if (burning != null) {
                AddKnownBurningSource(burning.sourceOfBurning, targetPOI);
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public bool AddKnownBurningSource(BurningSource burningSource, IPointOfInterest burningPOI) {
            if (!seenBurningSources.Contains(burningSource)) {
                seenBurningSources.Add(burningSource);
                burningSource.AddOnBurningExtinguishedAction(RemoveKnownBurningSource);
                burningSource.AddOnBurningObjectAddedAction(OnObjectStartedBurning);
                burningSource.AddOnBurningObjectRemovedAction(OnObjectStoppedBurning);
                TriggerReactionToFireOnFirstTimeSeeing(burningPOI);
                return true;
            }
            return false;
        }
        public void RemoveKnownBurningSource(BurningSource burningSource) {
            if (seenBurningSources.Remove(burningSource)) {
                burningSource.RemoveOnBurningExtinguishedAction(RemoveKnownBurningSource);
                burningSource.RemoveOnBurningObjectAddedAction(OnObjectStartedBurning);
                burningSource.RemoveOnBurningObjectRemovedAction(OnObjectStoppedBurning);
            }
        }
        private void TriggerReactionToFireOnFirstTimeSeeing(IPointOfInterest burningPOI) {
            string debugLog = owner.name + " saw a fire for the first time, reduce Happiness by 20, add Anxious status";
            owner.needsComponent.AdjustHappiness(-20f);
            owner.traitContainer.AddTrait(owner, "Anxious");
            int chance = UnityEngine.Random.Range(0, 2);
            if(chance == 0) {
                debugLog += "\n-Character decided to flee";
                Log log = new Log(GameManager.Instance.Today(), "Trait", name, "flee");
                log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                owner.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                owner.marker.AddAvoidInRange(burningPOI);
            } else {
                debugLog += "\n-Character decided to trigger Cowering interrupt";
                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, owner);
            }
            owner.logComponent.PrintLogIfActive(debugLog);
        }
        public void BeShellshocked(BurningSource source, Character character) {
            string summary = GameManager.Instance.TodayLogString() + character.name + " saw burning source " + source.ToString();
            //TODO:
            //if (character.marker.AddAvoidsInRange(source.objectsOnFire)) {
            //    summary += "\nStarted fleeing";
            //    //TODO:character.CancelAllJobsAndPlans();
            //    Log log = new Log(GameManager.Instance.Today(), "Trait", name, "flee");
            //    log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            //    log.AddLogToInvolvedObjects();
            //    PlayerManager.Instance.player.ShowNotificationFrom(character, log);
            //    character.traitContainer.AddTrait(character, "Shellshocked");
            //} else {
            //    summary += "\nDid not flee because already fleeing.";
            //}
            Debug.Log(summary);
        }
        public void BeCatatonic(BurningSource source, Character character) {
            Log log = new Log(GameManager.Instance.Today(), "Trait", name, "catatonic");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(character, log);
            character.traitContainer.AddTrait(character, "Catatonic");
        }

        private void OnObjectStartedBurning(ITraitable poi) {
            //owner.marker.AddTerrifyingObject(poi);
        }
        private void OnObjectStoppedBurning(ITraitable poi) {
            //owner.marker.RemoveTerrifyingObject(poi);
        }
    }
}

