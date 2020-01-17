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
            if (addedTo is IPointOfInterest) {
                if (addedTo is Character) {
                    Character character = addedTo as Character;
                    character.AdjustDoNotRecoverHP(1);
                    if(character.canMove && character.canWitness && character.canPerform) {
                        CreateJobsOnEnterVisionBasedOnTrait(character, character);
                    }
                } else {
                    IPointOfInterest obj = addedTo as IPointOfInterest;
                    obj.SetPOIState(POI_STATE.INACTIVE);
                }
            }
            if (sourceOfBurning != null && !sourceOfBurning.objectsOnFire.Contains(owner)) {
                SetSourceOfBurning(sourceOfBurning, owner);
            }
            //Messenger.AddListener(Signals.TICK_ENDED, PerTickEnded);
            base.OnAddTrait(addedTo);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            //Messenger.RemoveListener(Signals.TICK_ENDED, PerTickEnded);
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
            if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE) && (characterThatWillDoJob.stateComponent.currentState == null || characterThatWillDoJob.stateComponent.currentState.characterState != CHARACTER_STATE.DOUSE_FIRE)) {
                string summary = GameManager.Instance.TodayLogString() + characterThatWillDoJob.name + " saw a fire from source " + sourceOfBurning.ToString();
                if (!TryToCreateDouseFireJob(traitOwner, characterThatWillDoJob)) {
                    Pyrophobic pyrophobic = characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Pyrophobic") as Pyrophobic;
                    if (pyrophobic != null) {
                        //pyrophobic
                        summary += "\nDid not create douse fire job because character is pyrophobic!";
                        //When he sees a fire source for the first time, reduce Happiness by 2000. Do not create Douse Fire job. It should always Flee from fire. Add log showing reason for fleeing is Pyrophobia
                        if (pyrophobic.AddKnownBurningSource(sourceOfBurning)) {
                            characterThatWillDoJob.needsComponent.AdjustHappiness(-20f);
                        }
                        //It will trigger one of the following:
                        if (!characterThatWillDoJob.marker.hasFleePath && characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Catatonic") == null) { //if not already fleeing or catatonic
                            //50% gain Shellshocked and Flee from fire. Log "[Actor Name] saw a fire and fled from it."
                            if (UnityEngine.Random.Range(0, 100) < 50) {
                                pyrophobic.BeShellshocked(sourceOfBurning, characterThatWillDoJob);
                            }
                            //50% gain Catatonic. Log "[Actor Name] saw a fire and became Catatonic."
                            else {
                                pyrophobic.BeCatatonic(sourceOfBurning, characterThatWillDoJob);
                            }
                        }
                    } else {
                        summary += "\nDid not create douse fire job because maximum dousers has been reached!";
                        //if the character did not create a douse fire job. Check if he/she will watch instead. (Characters will just watch if their current actions are lower priority than Watch) NOTE: Lower priority value is considered higher priority
                        //also make sure that character is not already watching.
                        //int currentPriorityValue = characterThatWillDoJob.GetCurrentPriorityValue();
                        //if (characterThatWillDoJob != traitOwner
                        //    && (characterThatWillDoJob.currentActionNode == null || characterThatWillDoJob.currentActionNode.action.goapType != INTERACTION_TYPE.WATCH)
                        //    && currentPriorityValue > JOB_TYPE.WATCH.GetJobTypePriority()) {
                        //    summary += "\nWill watch because current priority value is  " + currentPriorityValue.ToString();
                        //    Character nearestDouser = sourceOfBurning.GetNearestDouserFrom(characterThatWillDoJob);
                        //    if (nearestDouser != null && nearestDouser.stateComponent.currentState is DouseFireState) {
                        //        characterThatWillDoJob.CreateWatchEvent(null, nearestDouser.stateComponent.currentState, nearestDouser);
                        //        summary += "\nCreated watch event targetting " + nearestDouser.name;
                        //    } else {
                        //        summary += "\nDid not watch because nearest douser is null or nearest douser is not in douse fire state. Nearest douser is: " + (nearestDouser?.name ?? "None");
                        //    }
                        //} else {
                        //    summary += "\nDid not watch because current priority value is " + currentPriorityValue.ToString() + " or is already doing watch.";
                        //}
                    }
                    Debug.Log(summary);
                    return false;
                }
                summary += "\nCreated douse fire job!";
                Debug.Log(summary);
                return true;
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        private bool TryToCreateDouseFireJob(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            //only create a remove fire job if the characters dousing the fire of a specific source is less than the required amount
            if (sourceOfBurning.dousers.Count < 3 && characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Pyrophobic") == null) {  //3
                bool willCreateDouseFireJob = false;
                if (traitOwner is Character) {
                    Character targetCharacter = traitOwner as Character;
                    //When a character sees someone burning, it must create a Remove Fire job targetting that character (if they are not enemies).
                    if (targetCharacter.isPartOfHomeFaction && characterThatWillDoJob.isPartOfHomeFaction
                        && (targetCharacter == characterThatWillDoJob || !characterThatWillDoJob.opinionComponent.IsEnemiesWith(targetCharacter))) {
                        willCreateDouseFireJob = true;
                    }
                } else {
                    //if anything else other than a character always create douse fire job.
                    willCreateDouseFireJob = true;
                }

                if (willCreateDouseFireJob) {
                    CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.DOUSE_FIRE, CHARACTER_STATE.DOUSE_FIRE, characterThatWillDoJob);
                    if (CanTakeRemoveFireJob(characterThatWillDoJob, traitOwner)) {
                        sourceOfBurning.AddCharactersDousingFire(characterThatWillDoJob); //adjust the number of characters dousing the fire source. NOTE: Make sure to reduce that number if a character decides to quit the job for any reason.
                        job.AddOnUnassignAction(sourceOfBurning.RemoveCharactersDousingFire); //This is the action responsible for reducing the number of characters dousing the fire when a character decides to quit the job.
                        //TODO: characterThatWillDoJob.CancelAllJobsAndPlans(); //cancel all other plans except douse fire.
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    }
                }
            }
            return false;
        }
        public override bool IsTangible() {
            return true;
        }
        public override string GetTestingData() {
            return sourceOfBurning.ToString();
        }
        public override void OnTickEnded() {
            base.OnTickEnded();
            PerTickEnded();
        }
        #endregion

        public void LoadSourceOfBurning(BurningSource source) {
            sourceOfBurning = source;
        }
        public void SetSourceOfBurning(BurningSource source, ITraitable obj) {
            sourceOfBurning = source;
            IPointOfInterest poiOnFire;
            if (obj is LocationGridTile) {
                poiOnFire = (obj as LocationGridTile).genericTileObject;
            } else {
                poiOnFire = obj as IPointOfInterest; //assuming that everything else is POI other than LocationGridTile
            }
            source.AddObjectOnFire(poiOnFire);
        }

        private bool CanTakeRemoveFireJob(Character character, JobQueueItem item) {
            if (item is GoapPlanJob) {
                GoapPlanJob job = item as GoapPlanJob;
                return CanTakeRemoveFireJob(character, job.targetPOI);
            }
            return false;
        }
        private bool CanTakeRemoveFireJob(Character character, IPointOfInterest target) {
            if (target is Character) {
                Character targetCharacter = target as Character;
                if (character == target) {
                    //the burning character is himself
                    return true;
                } else {
                    //if burning character is other character, make sure that the character that will do the job is not burning.
                    return character.traitContainer.GetNormalTrait<Trait>("Burning") == null && !character.opinionComponent.IsEnemiesWith(targetCharacter);
                }
            } else {
                //make sure that the character that will do the job is not burning.
                return character.traitContainer.GetNormalTrait<Trait>("Burning") == null;
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
            if (Random.Range(0, 100) < 25) { //5
                List<ITraitable> choices = new List<ITraitable>();
                LocationGridTile origin = owner.gridTileLocation;
                choices.AddRange(origin.GetTraitablesOnTileWithTrait("Flammable"));
                List<LocationGridTile> neighbours = origin.FourNeighbours();
                for (int i = 0; i < neighbours.Count; i++) {
                    choices.AddRange(neighbours[i].GetTraitablesOnTileWithTrait("Flammable"));
                }
                choices = choices.Where(x => x.traitContainer.GetNormalTrait<Trait>("Burning", "Burnt", "Wet", "Fireproof") == null).ToList();
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
                    owner.traitContainer.AddTrait(owner, "Burnt");
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
