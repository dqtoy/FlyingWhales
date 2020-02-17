using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Burning : Trait {

        public ITraitable owner { get; private set; }
        public override bool isPersistent { get { return true; } }
        private GameObject burningEffect;

        public BurningSource sourceOfBurning { get; private set; }

        public Burning() {
            name = "Burning";
            description = "This character is on fire!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            owner = addedTo;
            burningEffect = GameManager.Instance.CreateBurningEffectAt(addedTo);
            if (addedTo is IPointOfInterest poi) {
                if (poi is Character character) {
                    character.AdjustDoNotRecoverHP(1);
                    if(character.canMove && character.canWitness && character.canPerform) {
                        CreateJobsOnEnterVisionBasedOnTrait(character, character);
                    }
                } else {
                    IPointOfInterest obj = poi;
                    obj.SetPOIState(POI_STATE.INACTIVE);
                }
                Messenger.Broadcast(Signals.REPROCESS_POI, poi);
            }
            if (sourceOfBurning != null && !sourceOfBurning.objectsOnFire.Contains(owner)) {
                SetSourceOfBurning(sourceOfBurning, owner);
            }
            Messenger.AddListener(Signals.TICK_ENDED, PerTickEnded);
            
            base.OnAddTrait(addedTo);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            Messenger.RemoveListener(Signals.TICK_ENDED, PerTickEnded);
            ObjectPoolManager.Instance.DestroyObject(burningEffect);
            if (removedFrom is IPointOfInterest) {
                if (removedFrom is Character) {
                    Character character = removedFrom as Character;
                    character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.DOUSE_FIRE);
                    character.AdjustDoNotRecoverHP(-1);
                }
            } 
            sourceOfBurning.RemoveObjectOnFire(owner);
        }
        public override bool OnDeath(Character character) {
            //base.OnDeath(character);
            return character.traitContainer.RemoveTrait(character, this);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner.gridTileLocation != null 
                && traitOwner.gridTileLocation.IsPartOfSettlement(characterThatWillDoJob.homeSettlement)) {

                characterThatWillDoJob.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
            }
            
            //pyrophobic handling
            Pyrophobic pyrophobic = characterThatWillDoJob.traitContainer.GetNormalTrait<Pyrophobic>("Pyrophobic");
            if (pyrophobic != null) {
                //pyrophobic
                //When he sees a fire source for the first time, reduce Happiness by 2000. Do not create Douse Fire job. It should always Flee from fire. Add log showing reason for fleeing is Pyrophobia
                pyrophobic.AddKnownBurningSource(sourceOfBurning, traitOwner);
                //It will trigger one of the following:
                //if (!characterThatWillDoJob.marker.hasFleePath &&
                //    characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Catatonic") == null) {
                //    //if not already fleeing or catatonic
                //    //50% gain Shellshocked and Flee from fire. Log "[Actor Name] saw a fire and fled from it."
                //    if (UnityEngine.Random.Range(0, 100) < 50) {
                //        pyrophobic.BeShellshocked(sourceOfBurning, characterThatWillDoJob);
                //    }
                //    //50% gain Catatonic. Log "[Actor Name] saw a fire and became Catatonic."
                //    else {
                //        pyrophobic.BeCatatonic(sourceOfBurning, characterThatWillDoJob);
                //    }
                //}
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        public override bool IsTangible() {
            return true;
        }
        public override string GetTestingData() {
            return sourceOfBurning.ToString();
        }
        // public override void OnTickEnded() {
        //     base.OnTickEnded();
        //     PerTickEnded();
        // }
        #endregion

        public void LoadSourceOfBurning(BurningSource source) {
            sourceOfBurning = source;
        }
        public void SetSourceOfBurning(BurningSource source, ITraitable obj) {
            sourceOfBurning = source;
            if (obj is IPointOfInterest) {
                var poiOnFire = obj as IPointOfInterest;
                source.AddObjectOnFire(poiOnFire);
            }
        }
        private void PerTickEnded() {
            //Every tick, a Burning tile, object or character has a 15% chance to spread to an adjacent flammable tile, flammable character, 
            //flammable object or the object in the same tile.
            if(PlayerManager.Instance.player.seizeComponent.seizedPOI == owner) {
                //Temporary fix only, if the burning object is seized, spreading of fire should not trigger
                return;
            }
            if(owner.gridTileLocation == null) {
                //Messenger.RemoveListener(Signals.TICK_ENDED, PerTickEnded);
                //Temporary fix only, if the burning object has no longer have a tile location (presumably destroyed), spreading of fire should not trigger, and remove listener for per tick
                return;
            }
            if (Random.Range(0, 100) < 10) { //5
                List<ITraitable> choices = new List<ITraitable>();
                LocationGridTile origin = owner.gridTileLocation;
                choices.AddRange(origin.GetTraitablesOnTileWithTrait("Flammable"));
                List<LocationGridTile> neighbours = origin.FourNeighbours();
                for (int i = 0; i < neighbours.Count; i++) {
                    choices.AddRange(neighbours[i].GetTraitablesOnTileWithTrait("Flammable"));
                }
                choices = choices.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
                if (choices.Count > 0) {
                    ITraitable chosen = choices[Random.Range(0, choices.Count)];
                    Burning burning = new Burning();
                    burning.SetSourceOfBurning(sourceOfBurning, chosen);
                    chosen.traitContainer.AddTrait(chosen, burning);
                }
            }

            owner.AdjustHP(-(int)(owner.maxHP * 0.02f), true, this);
            if (owner is Character) {
                //Burning characters reduce their current hp by 2% of maxhp every tick. 
                //They also have a 6% chance to remove Burning effect but will not gain a Burnt trait afterwards. 
                //If a character dies and becomes a corpse, it may still continue to burn.
                if (Random.Range(0, 100) < 6) {
                    owner.traitContainer.RemoveTrait(owner, this);
                }
            } else {
                if (owner.currentHP == 0) {
                    owner.traitContainer.RemoveTrait(owner, this);
                    // owner.traitContainer.AddTrait(owner, "Burnt");
                } else {
                    //Every tick, a Burning tile or object also has a 3% chance to remove Burning effect. 
                    //Afterwards, it will have a Burnt trait, which disables its Flammable trait (meaning it can no longer gain a Burning status).
                    if (Random.Range(0, 100) < 3) {
                        owner.traitContainer.RemoveTrait(owner, this);
                        owner.traitContainer.AddTrait(owner, "Burnt");
                    }
                }
                
            }
        }

    }

    public class SaveDataBurning : SaveDataTrait {
        public int burningSourceID;

        public override void Save(Trait trait) {
            base.Save(trait);
            Burning derivedTrait = trait as Burning;
            burningSourceID = derivedTrait.sourceOfBurning.id;
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Burning derivedTrait = trait as Burning;
            derivedTrait.LoadSourceOfBurning(LandmarkManager.Instance.GetBurningSourceByID(burningSourceID));
            return trait;
        }
    }
}
