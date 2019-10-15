using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Burning : Trait {

    public IPointOfInterest owner { get; private set; }
    public override bool isPersistent { get { return true; } }
    private GameObject burningEffect;

    public BurningSource sourceOfBurning { get; private set; }

    public Burning() {
        name = "Burning";
        description = "This character is on fire!";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        if (addedTo is LocationGridTile) {
            LocationGridTile tile = addedTo as LocationGridTile;
            burningEffect = GameManager.Instance.CreateBurningEffectAt(tile);
            if (tile.genericTileObject == null) {
                throw new System.Exception("Generic Tile Object of " + tile.ToString() + " is null!");
            }
            tile.genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
            owner = tile.genericTileObject;
        } else if (addedTo is IPointOfInterest) {
            owner = addedTo as IPointOfInterest;
            if (addedTo is TileObject) {
                TileObject obj = addedTo as TileObject;
                burningEffect = GameManager.Instance.CreateBurningEffectAt(obj);
                obj.SetPOIState(POI_STATE.INACTIVE);
                obj.AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
            } else if (addedTo is SpecialToken) {
                SpecialToken token = addedTo as SpecialToken;
                burningEffect = GameManager.Instance.CreateBurningEffectAt(token);
                token.SetPOIState(POI_STATE.INACTIVE);
                token.AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
            } else if (addedTo is Character) {
                Character character = addedTo as Character;
                burningEffect = GameManager.Instance.CreateBurningEffectAt(character);
                character.AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
            }
        }

        if (sourceOfBurning != null && !sourceOfBurning.objectsOnFire.Contains(owner)) {
            SetSourceOfBurning(sourceOfBurning, owner);
        }
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        base.OnAddTrait(addedTo);
    }
    public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        base.OnRemoveTrait(removedFrom, removedBy);
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        ObjectPoolManager.Instance.DestroyObject(burningEffect);
        if (removedFrom is IPointOfInterest) {
            if (removedFrom is Character) {
                (removedFrom as Character).CancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_FIRE);
            }
            (removedFrom as IPointOfInterest).RemoveAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
        } else if (removedFrom is LocationGridTile) {
            (removedFrom as LocationGridTile).genericTileObject.RemoveAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
        }
        sourceOfBurning.RemoveObjectOnFire(owner);

    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.REMOVE_FIRE) && (characterThatWillDoJob.stateComponent.currentState == null || characterThatWillDoJob.stateComponent.currentState.characterState != CHARACTER_STATE.DOUSE_FIRE)) {
            string summary = GameManager.Instance.TodayLogString() + characterThatWillDoJob.name + " saw a fire from source " + sourceOfBurning.ToString();
            if(!TryToCreateDouseFireJob(traitOwner, characterThatWillDoJob)) {
                Pyrophobic pyrophobic = characterThatWillDoJob.GetNormalTrait("Pyrophobic") as Pyrophobic;
                if (pyrophobic != null) {
                    //pyrophobic
                    summary += "\nDid not create douse fire job because character is pyrophobic!";
                    //When he sees a fire source for the first time, reduce Happiness by 2000. Do not create Douse Fire job. It should always Flee from fire. Add log showing reason for fleeing is Pyrophobia
                    if (pyrophobic.AddKnownBurningSource(sourceOfBurning)) {
                        characterThatWillDoJob.AdjustHappiness(-2000);
                    }
                    //It will trigger one of the following:
                    if (!characterThatWillDoJob.marker.hasFleePath && characterThatWillDoJob.GetNormalTrait("Catatonic") == null) { //if not already fleeing or catatonic
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
                    int currentPriorityValue = characterThatWillDoJob.GetCurrentPriorityValue();
                    if ((characterThatWillDoJob.currentAction == null || characterThatWillDoJob.currentAction.goapType != INTERACTION_TYPE.WATCH)
                        && currentPriorityValue > InteractionManager.Instance.GetInitialPriority(JOB_TYPE.WATCH)) {
                        summary += "\nWill watch because current priority value is  " + currentPriorityValue.ToString();
                        Character nearestDouser = sourceOfBurning.GetNearestDouserFrom(characterThatWillDoJob);
                        if (nearestDouser != null && nearestDouser.stateComponent.currentState is DouseFireState) {
                            characterThatWillDoJob.CreateWatchEvent(null, nearestDouser.stateComponent.currentState, nearestDouser);
                            summary += "\nCreated watch event targetting " + nearestDouser.name;
                        } else {
                            summary += "\nDid not watch because nearest douser is null or nearest douser is not in douse fire state. Nearest douser is: " + (nearestDouser?.name ?? "None");
                        }
                    } else {
                        summary += "\nDid not watch because current priority value is " + currentPriorityValue.ToString() + " or is already doing watch.";
                    }
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
        if (sourceOfBurning.dousers.Count < 3 && characterThatWillDoJob.GetNormalTrait("Pyrophobic") == null) {  //3
            bool willCreateDouseFireJob = false;
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                //When a character sees someone burning, it must create a Remove Fire job targetting that character (if they are not enemies).
                if (targetCharacter.isPartOfHomeFaction && characterThatWillDoJob.isPartOfHomeFaction 
                    && (targetCharacter == characterThatWillDoJob || !characterThatWillDoJob.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY))) {
                    willCreateDouseFireJob = true;
                }
            } else {
                //if anything else other than a character always create douse fire job.
                willCreateDouseFireJob = true;
            }

            if (willCreateDouseFireJob) {
                CharacterStateJob job = new CharacterStateJob(JOB_TYPE.REMOVE_FIRE, CHARACTER_STATE.DOUSE_FIRE);
                if (CanTakeRemoveFireJob(characterThatWillDoJob, traitOwner)) {
                    sourceOfBurning.AddCharactersDousingFire(characterThatWillDoJob); //adjust the number of characters dousing the fire source. NOTE: Make sure to reduce that number if a character decides to quit the job for any reason.
                    job.AddOnUnassignAction(sourceOfBurning.RemoveCharactersDousingFire); //This is the action responsible for reducing the number of characters dousing the fire when a character decides to quit the job.
                    //characterThatWillDoJob.CancelAllJobsAndPlansExcept(JOB_TYPE.REMOVE_FIRE); //cancel all other plans except douse fire.
                    characterThatWillDoJob.CancelAllPlans(); //cancel all other plans except douse fire.
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
                return character.GetNormalTrait("Burning") == null && !character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY);
            }
        } else {
            //make sure that the character that will do the job is not burning.
            return character.GetNormalTrait("Burning") == null;
        }
    }
    private void PerTick() {
        //Burning characters reduce their current hp by 2% of maxhp every tick. 
        //They also have a 6% chance to remove Burning effect but will not gain a Burnt trait afterwards. 
        //If a character dies and becomes a corpse, it may still continue to burn.
        if (owner is Character) {
            Character character = owner as Character;
            if (!character.isDead) {
                character.AdjustHP(-(int)(character.maxHP * 0.02f), true, this);
            }
            if (Random.Range(0, 100) < 6) {
                owner.RemoveTrait(this);
            }
        } else {
            //Every tick, a Burning tile or object also has a 3% chance to remove Burning effect. 
            //Afterwards, it will have a Burnt trait, which disables its Flammable trait (meaning it can no longer gain a Burning status).
            if (!(owner is GenericTileObject)) {
                owner.AdjustHP(-(int)(owner.maxHP * 0.02f), true, this);
            }
            if (owner.gridTileLocation != null && Random.Range(0, 100) < 3) {
                owner.RemoveTrait(this);
                owner.AddTrait("Burnt");
                return; //do not execute other effecs.
            }
        }

        //Every tick, a Burning tile, object or character has a 15% chance to spread to an adjacent flammable tile, flammable character, 
        //flammable object or the object in the same tile. 
        if (Random.Range(0, 100) < 15) {
            List<ITraitable> choices = new List<ITraitable>();
            LocationGridTile origin = owner.gridTileLocation;
            choices.AddRange(origin.GetAllTraitablesOnTileWithTrait("Flammable"));
            List<LocationGridTile> neighbours = origin.FourNeighbours();
            for (int i = 0; i < neighbours.Count; i++) {
                choices.AddRange(neighbours[i].GetAllTraitablesOnTileWithTrait("Flammable"));
            }
            choices = choices.Where(x => x.GetNormalTrait("Burning", "Burnt", "Wet", "Fireproof") == null).ToList();
            if (choices.Count > 0) {
                ITraitable chosen = choices[Random.Range(0, choices.Count)];
                Burning burning = new Burning();
                burning.SetSourceOfBurning(sourceOfBurning, chosen);
                chosen.AddTrait(burning);
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