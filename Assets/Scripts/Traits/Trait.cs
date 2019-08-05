using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ONLY HAS ONE INSTANCE IN THE WORLD, DO NOT PUT ANYTHING THAT WILL HAVE DIFFERENT VALUES IN DIFFERENT INSTANCES
[System.Serializable]
public class Trait {
    public virtual string nameInUI {
        get { return name; }
    }
    public Character responsibleCharacter { get; protected set; }
    public List<Character> responsibleCharacters { get; protected set; }
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
    public int level;
    public List<TraitEffect> effects;
    public Dictionary<IPointOfInterest, string> expiryTickets { get; private set; } //this is the key for the scheduled removal of this trait for each object
    public GoapAction gainedFromDoing { get; private set; } //what action was this poi involved in that gave it this trait.
    public bool isDisabled { get; private set; }
    public virtual bool broadcastDuplicates { get { return false; } }
    public virtual bool isPersistent { get { return false; } } //should this trait persist through all a character's alter egos
    public GameDate dateEstablished { get; protected set; }
    //private Character _responsibleCharacter;

    private System.Action onRemoveAction;

    #region Virtuals
    public virtual void OnAddTrait(IPointOfInterest sourceCharacter) {
        //if(type == TRAIT_TYPE.CRIMINAL && sourceCharacter is Character) {
        //    Character character = sourceCharacter as Character;
        //    character.CreateApprehendJob();
        //}
        if(level == 0) {
            SetLevel(1);
        }
#if !WORLD_CREATION_TOOL
        SetDateEstablished(GameManager.Instance.Today());
#endif
    }
    public virtual void OnRemoveTrait(IPointOfInterest sourceCharacter, Character removedBy) {
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
    public virtual string GetToolTipText() { return string.Empty; }
    public virtual bool IsUnique() { return true; }
    /// <summary>
    /// Only used for characters, since traits aren't removed when a character dies.
    /// This function will be called to ensure that any unneeded resources in traits can be freed up when a character dies.
    /// <see cref="Character.Death(string)"/>
    /// </summary>
    public virtual void OnDeath(Character character) { }
    public virtual string GetTestingData() { return string.Empty; }
    public virtual bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) { return false; }
    protected virtual void OnChangeLevel() { }
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
    public void OverrideDuration(int newDuration) {
        daysDuration = newDuration;
    }
    public void SetCharacterResponsibleForTrait(Character character) {
        responsibleCharacter = character;
    }
    public void AddCharacterResponsibleForTrait(Character character) {
        if(responsibleCharacters == null) {
            responsibleCharacters = new List<Character>();
        }
        if (character != null && !responsibleCharacters.Contains(character)) {
            responsibleCharacters.Add(character);
        }
    }
    public bool IsResponsibleForTrait(Character character) {
        if(responsibleCharacter == character) {
            return true;
        }else if(responsibleCharacters != null) {
            return responsibleCharacters.Contains(character);
        }
        return false;
    }
    public void SetExpiryTicket(IPointOfInterest poi, string expiryTicket) {
        if (expiryTickets == null) {
            expiryTickets = new Dictionary<IPointOfInterest, string>();
        }
        if (!expiryTickets.ContainsKey(poi)) {
            expiryTickets.Add(poi, expiryTicket);
        } else {
            expiryTickets[poi] = expiryTicket;
        }
    }
    public void RemoveExpiryTicket(IPointOfInterest poi) {
        if (expiryTickets != null) {
            expiryTickets.Remove(poi);
        }
    }
    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY);
        OnChangeLevel();
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY);
        OnChangeLevel();
    }
    public void SetDateEstablished(GameDate date) {
        dateEstablished = date;
    }
    public void SetTraitEffects(List<TraitEffect> effects) {
        this.effects = effects;
    }

    #region Jobs
    protected bool CanCharacterTakeRemoveTraitJob(Character character, Character targetCharacter, JobQueueItem job) {
        if (character != targetCharacter && character.faction == targetCharacter.faction && character.isAtHomeArea) {
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
        if(!character.HasTraitOf(TRAIT_TYPE.CRIMINAL) && character.isAtHomeArea
                && character.role.roleType != CHARACTER_ROLE.BEAST) {
            return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN;
        }
        return false;
    }
    protected bool CanCharacterTakeApprehendJob(Character character, Character targetCharacter, JobQueueItem job) {
        if(character.isAtHomeArea && !character.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
            return character.role.roleType == CHARACTER_ROLE.SOLDIER && character.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE;
        }
        return false;
    }
    protected bool CanCharacterTakeRestrainJob(Character character, Character targetCharacter, JobQueueItem job) {
        return targetCharacter.faction != character.faction && character.isAtHomeArea && character.faction == character.homeArea.owner 
            && (character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN || character.role.roleType == CHARACTER_ROLE.ADVENTURER)
            && character.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE && !character.HasTraitOf(TRAIT_TYPE.CRIMINAL);
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