using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ONLY HAS ONE INSTANCE IN THE WORLD, DO NOT PUT ANYTHING THAT WILL HAVE DIFFERENT VALUES IN DIFFERENT INSTANCES
[System.Serializable]
public class Trait {
    public virtual string nameInUI {
        get { return name; }
    }
    public virtual Character responsibleCharacter {
        get { return null; }
    }
    public virtual List<Character> responsibleCharacters {
        get { return null; }
    }
    public string name;
    public string description;
    public string thoughtText;
    public TRAIT_TYPE type;
    public TRAIT_EFFECT effect;
    public TRAIT_TRIGGER trigger;
    public INTERACTION_TYPE associatedInteraction;
    public List<INTERACTION_TYPE> advertisedInteractions;
    public CRIME_CATEGORY crimeSeverity;
    public int daysDuration; //Zero (0) means Permanent
    public List<TraitEffect> effects;
    public GoapAction gainedFromDoing { get; private set; } //what action was this poi involved in that gave it this trait.
    public bool isDisabled { get; private set; }
    public virtual bool broadcastDuplicates { get { return false; } }
    public virtual bool isPersistent { get { return false; } } //should this trait persist through all the character's alter egos
    public GameDate dateEstablished { get; protected set; }
    //private Character _responsibleCharacter;

    private System.Action onRemoveAction;

    #region Virtuals
    public virtual void OnAddTrait(IPointOfInterest sourceCharacter) {
        //if(type == TRAIT_TYPE.CRIMINAL && sourceCharacter is Character) {
        //    Character character = sourceCharacter as Character;
        //    character.CreateApprehendJob();
        //}
#if !WORLD_CREATION_TOOL
         dateEstablished = GameManager.Instance.Today();
#endif
    }
    public virtual void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if (onRemoveAction != null) {
            onRemoveAction();
        }
        if (type == TRAIT_TYPE.CRIMINAL && sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            if (!character.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                character.CancelAllJobsTargettingThisCharacter(JOB_TYPE.APPREHEND);
            }
        }
    }
    public virtual void SetCharacterResponsibleForTrait(Character character) {
        //_responsibleCharacter = character;
    }
    public virtual void AddCharacterResponsibleForTrait(Character character) {
    }
    public virtual bool IsResponsibleForTrait(Character character) {
        return false;
        //return _responsibleCharacter == character;
    }
    public virtual string GetToolTipText() { return string.Empty; }
    public virtual bool IsUnique() { return true; }
    /// <summary>
    /// Only used for characters, since traits aren't removed when a character dies.
    /// This function will be called to ensure that any unneeded resources in traits can be freed up when a character dies.
    /// <see cref="Character.Death(string)"/>
    /// </summary>
    public virtual void OnDeath() { }
    public virtual string GetTestingData() { return string.Empty; }
    public virtual bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) { return false; }
    #endregion

    public void SetOnRemoveAction(System.Action onRemoveAction) {
        this.onRemoveAction = onRemoveAction;
    }
    public void SetGainedFromDoing(GoapAction action) {
        gainedFromDoing = action;
    }
    public void SetIsDisabled(bool state) {
        isDisabled = state;
    }

    #region Jobs
    protected bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter, JobQueueItem job) {
        if (character != targetCharacter && character.faction.id == targetCharacter.faction.id) {
            if (responsibleCharacter != null && responsibleCharacter == character) {
                return false;
            }
            if (responsibleCharacters != null && responsibleCharacters.Contains(character)) {
                return false;
            }
            if (character.faction.id == FactionManager.Instance.neutralFaction.id) {
                return character.race == targetCharacter.race && character.homeArea == targetCharacter.homeArea && !targetCharacter.HasRelationshipOfTypeWith(character, RELATIONSHIP_TRAIT.ENEMY);
            }
            return !character.HasRelationshipOfTypeWith(targetCharacter, RELATIONSHIP_TRAIT.ENEMY);
        }
        return false;
    }
    protected bool CanTakeBuryJob(Character character, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN;
    }
    protected bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.SOLDIER && character.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE;
    }
    protected bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter, JobQueueItem job) {
        return (character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN) && character.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE; // || character.role.roleType == CHARACTER_ROLE.ADVENTURER
    }
    #endregion
}

[System.Serializable]
public class TraitEffect {
    public STAT stat;
    public float amount;
    public bool isPercentage;
    public TRAIT_REQUIREMENT_CHECKER checker;
    public TRAIT_REQUIREMENT_TARGET target;
    public DAMAGE_IDENTIFIER damageIdentifier; //Only used during combat
    public string description;

    public bool hasRequirement;
    public bool isNot;
    public TRAIT_REQUIREMENT requirementType;
    public TRAIT_REQUIREMENT_SEPARATOR requirementSeparator;
    public List<string> requirements;
}