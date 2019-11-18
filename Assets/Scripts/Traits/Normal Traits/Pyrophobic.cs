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
            
            
            
            daysDuration = 0;
            seenBurningSources = new List<BurningSource>();
        }

        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
            }
        }


        public bool AddKnownBurningSource(BurningSource burningSource) {
            if (!seenBurningSources.Contains(burningSource)) {
                seenBurningSources.Add(burningSource);
                burningSource.AddOnBurningExtinguishedAction(RemoveKnownBurningSource);
                burningSource.AddOnBurningObjectAddedAction(OnObjectStartedBurning);
                burningSource.AddOnBurningObjectRemovedAction(OnObjectStoppedBurning);
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

        public void BeShellshocked(BurningSource source, Character character) {
            string summary = GameManager.Instance.TodayLogString() + character.name + " saw burning source " + source.ToString();
            if (character.marker.AddAvoidsInRange(source.objectsOnFire)) {
                summary += "\nStarted fleeing";
                //TODO:character.CancelAllJobsAndPlans();
                Log log = new Log(GameManager.Instance.Today(), "Trait", this.GetType().ToString(), "flee");
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToInvolvedObjects();
                PlayerManager.Instance.player.ShowNotificationFrom(character, log);
                character.traitContainer.AddTrait(character, "Shellshocked");
            } else {
                summary += "\nDid not flee because already fleeing.";
            }
            Debug.Log(summary);
        }
        public void BeCatatonic(BurningSource source, Character character) {
            Log log = new Log(GameManager.Instance.Today(), "Trait", this.GetType().ToString(), "catatonic");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(character, log);
            character.traitContainer.AddTrait(character, "Catatonic");
        }

        private void OnObjectStartedBurning(IPointOfInterest poi) {
            //owner.marker.AddTerrifyingObject(poi);
        }
        private void OnObjectStoppedBurning(IPointOfInterest poi) {
            //owner.marker.RemoveTerrifyingObject(poi);
        }
    }
}

