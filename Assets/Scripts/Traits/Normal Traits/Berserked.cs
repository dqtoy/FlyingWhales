using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Berserked : Trait {

        public override bool isNotSavable {
            get { return true; }
        }

        private List<CharacterBehaviourComponent> _behaviourComponentsBeforeBerserked;
        
        public Berserked() {
            name = "Berserked";
            description = "This character will attack anyone at random and may destroy objects.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 72;
            hindersWitness = true;
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
                _behaviourComponentsBeforeBerserked = new List<CharacterBehaviourComponent>(character.behaviourComponent.currentBehaviourComponents);
                character.behaviourComponent.ReplaceBehaviourComponent(new List<CharacterBehaviourComponent>()
                    {CharacterManager.Instance.GetCharacterBehaviourComponent(typeof(BerserkBehaviour))});
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
                
                //check hostiles in range, remove any poi's that are not hostile with the character 
                List<IPointOfInterest> hostilesToRemove = new List<IPointOfInterest>();
                for (int i = 0; i < character.marker.hostilesInRange.Count; i++) {
                    IPointOfInterest poi = character.marker.hostilesInRange[i];
                    if (poi is Character) {
                        //poi is a character, check for hostilities
                        Character otherCharacter = poi as Character;
                        if (character.IsHostileWith(otherCharacter) == false) {
                            hostilesToRemove.Add(otherCharacter);
                        }    
                    } else {
                        //poi is not a character, remove
                        hostilesToRemove.Add(poi);
                    }
                }

                //remove all non hostiles from hostile in range
                for (int i = 0; i < hostilesToRemove.Count; i++) {
                    IPointOfInterest hostile = hostilesToRemove[i];
                    character.marker.RemoveHostileInRange(hostile);
                }
                character.behaviourComponent.ReplaceBehaviourComponent(_behaviourComponentsBeforeBerserked);
                _behaviourComponentsBeforeBerserked.Clear();
            }
        }
        public override void OnSeePOIEvenCannotWitness(IPointOfInterest targetPOI, Character character) {
            base.OnSeePOIEvenCannotWitness(targetPOI, character);
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (!targetCharacter.isDead) {
                    if (character.faction.isPlayerFaction) {
                        character.marker.AddHostileInRange(targetCharacter, isLethal: true); //check hostility if from player faction, so as not to attack other characters that are also from the same faction.
                    } else {
                        character.marker.AddHostileInRange(targetCharacter, checkHostility: false, isLethal: false);
                    }
                }
            } else if (targetPOI is TileObject || targetPOI is SpecialToken) {
                if (Random.Range(0, 100) < 35) {
                    //character.jobComponent.TriggerDestroy(targetPOI);
                    character.marker.AddHostileInRange(targetPOI, checkHostility: false, isLethal: false);
                }
            }
        }
        //public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        //    if (targetPOI is Character) {
        //        Character targetCharacter = targetPOI as Character;
        //        if (!targetCharacter.isDead) {
        //            if (characterThatWillDoJob.faction.isPlayerFaction) {
        //                return characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, isLethal: true); //check hostility if from player faction, so as not to attack other characters that are also from the same faction.
        //            } else {
        //                return characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, checkHostility: false, isLethal: false);
        //            }
        //        }
        //    }
        //    else if (targetPOI is TileObject || targetPOI is SpecialToken) {
        //        return characterThatWillDoJob.marker.AddHostileInRange(targetPOI, checkHostility: false, isLethal: false);
        //    } 
        //    return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        //}
        #endregion
    }
}

