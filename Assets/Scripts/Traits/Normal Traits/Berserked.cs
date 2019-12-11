using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Berserked : Trait {

        public override bool isNotSavable {
            get { return true; }
        }

        public Berserked() {
            name = "Berserked";
            description = "This character will attack anyone at random and may destroy objects.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 24;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                if (character.marker != null) {
                    character.marker.BerserkedMarker();
                }
                character.CancelAllJobs();
                //character.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                if (character.marker != null) {
                    character.marker.UnberserkedMarker();
                }
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (!targetCharacter.isDead) {
                    if (characterThatWillDoJob.faction.isPlayerFaction) {
                        return characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, isLethal: true); //check hostility if from player faction, so as not to attack other characters that are also from the same faction.
                    } else {
                        return characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, checkHostility: false, isLethal: false);
                    }
                }
            }
            else if (targetPOI is TileObject || targetPOI is SpecialToken) {
                return characterThatWillDoJob.marker.AddHostileInRange(targetPOI, checkHostility: false, isLethal: false);
            } 
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}

