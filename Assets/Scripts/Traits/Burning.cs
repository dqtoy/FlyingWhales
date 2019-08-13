using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Burning : Trait {

    private ITraitable owner;
    public override bool isPersistent { get { return true; } }

    private GameObject burningEffect;

    public Burning() {
        name = "Burning";
        description = "This is burning.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        owner = addedTo;
        if (addedTo is LocationGridTile) {
            LocationGridTile tile = addedTo as LocationGridTile;
            burningEffect = GameManager.Instance.CreateBurningEffectAt(tile);
            tile.genericTileObject.AddAdvertisedAction(INTERACTION_TYPE.DOUSE_FIRE);
        } else if (addedTo is TileObject) {
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
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
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
        
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            //When a character sees someone burning, it must create a Remove Fire job targetting that character (if they are not enemies).
            if (!targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_FIRE) 
                && (targetCharacter == characterThatWillDoJob || !characterThatWillDoJob.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY))) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_FIRE, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Burning", targetPOI = traitOwner });
                if (CanTakeRemoveFireJob(characterThatWillDoJob, targetCharacter)) {
                    //job.SetCanTakeThisJobChecker(CanTakeRemoveFireJob);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    characterThatWillDoJob.CancelAllJobsAndPlansExcept(JOB_TYPE.REMOVE_FIRE);
                    //if (characterThatWillDoJob.allGoapPlans.Count == 0 || characterThatWillDoJob.allGoapPlans.First().job == null || characterThatWillDoJob.allGoapPlans.First().job.priority > job.priority) {
                        //characterThatWillDoJob.jobQueue.ProcessFirstJobInQueue(characterThatWillDoJob);
                    //}
                    return true;
                } 
                //else {
                //    job.SetCanTakeThisJobChecker(CanTakeRemoveFireJob);
                //    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                //    return false;
                //}
            }
        } else {
            if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.REMOVE_FIRE, traitOwner)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_FIRE, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Burning", targetPOI = traitOwner });
                if (CanTakeRemoveFireJob(characterThatWillDoJob, traitOwner)) {
                    //job.SetCanTakeThisJobChecker(CanTakeRemoveFireJob);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    if (characterThatWillDoJob.allGoapPlans.Count == 0 || characterThatWillDoJob.allGoapPlans.First().job == null || characterThatWillDoJob.allGoapPlans.First().job.priority > job.priority) {
                        characterThatWillDoJob.jobQueue.ProcessFirstJobInQueue(characterThatWillDoJob);
                    }
                    return true;
                }
            }
            
            //else {
            //    job.SetCanTakeThisJobChecker(CanTakeRemoveFireJob);
            //    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
            //    return false;
            //}
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    public override bool IsTangible() {
        return true;
    }
    #endregion

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
            if (Random.Range(0, 100) < 3) {
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
            choices = choices.Where(x => x.GetNormalTrait("Burning", "Burnt", "Wet") == null).ToList();
            if (choices.Count > 0) {
                ITraitable chosen = choices[Random.Range(0, choices.Count)];
                chosen.AddTrait("Burning");
            }
        }
    }
}
