using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ONLY HAS ONE INSTANCE IN THE WORLD, DO NOT PUT ANYTHING THAT WILL HAVE DIFFERENT VALUES IN DIFFERENT INSTANCES
[System.Serializable]
public class Trait {
    public virtual string nameInUI {
        get { return name; }
    }
    public virtual bool isNotSavable {
        get { return false; }
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
    public bool isHidden;
    public string[] mutuallyExclusive; //list of traits that this trait cannot be with.
    public bool canBeTriggered;

    public Dictionary<ITraitable, string> expiryTickets { get; private set; } //this is the key for the scheduled removal of this trait for each object
    public GoapAction gainedFromDoing { get; private set; } //what action was this poi involved in that gave it this trait.
    public bool isDisabled { get; private set; }
    public GameDate dateEstablished { get; protected set; }

    public virtual bool broadcastDuplicates { get { return false; } }
    public virtual bool isPersistent { get { return false; } } //should this trait persist through all a character's alter egos
    public virtual bool isRemovedOnSwitchAlterEgo { get { return false; } }

    //private Character _responsibleCharacter;

    private System.Action onRemoveAction;

    #region Virtuals
    public virtual void OnAddTrait(ITraitable addedTo) {
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
        Messenger.Broadcast(Signals.TRAITABLE_GAINED_TRAIT, addedTo, this);
    }
    public virtual void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        if (onRemoveAction != null) {
            onRemoveAction();
        }
        if (type == TRAIT_TYPE.CRIMINAL && removedFrom is Character) {
            Character character = removedFrom as Character;
            if (!character.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                character.CancelAllJobsTargettingThisCharacter(JOB_TYPE.APPREHEND);
            }
        }
        Messenger.Broadcast(Signals.TRAITABLE_LOST_TRAIT, removedFrom, this, removedBy);
    }
    public virtual string GetToolTipText() { return string.Empty; }
    public virtual bool IsUnique() { return true; }
    /// <summary>
    /// Only used for characters, since traits aren't removed when a character dies.
    /// This function will be called to ensure that any unneeded resources in traits can be freed up when a character dies.
    /// <see cref="Character.Death(string)"/>
    /// </summary>
    public virtual void OnDeath(Character character) { }
    /// <summary>
    /// Used to return necessary actions when a character with this trait
    /// returns to life.
    /// </summary>
    /// <param name="character">The character that returned to life.</param>
    public virtual void OnReturnToLife(Character character) { }
    public virtual string GetTestingData() { return string.Empty; }
    public virtual bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) { return false; } //What jobs a character can create based on the target's traits?
    public virtual bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) { return false; } //What jobs a character can create based on the his/her own traits, considering the target?
    public virtual void OnSeePOI(IPointOfInterest targetPOI, Character character) { }
    protected virtual void OnChangeLevel() { }
    public virtual void OnOwnerInitiallyPlaced(Character owner) { }
    public virtual bool IsTangible() { return false; } //is this trait tangible? Only used for traits on tiles, so that the tile's tile object will be activated when it has a tangible trait
    public virtual bool PerTickOwnerMovement() { return false; } //returns true or false if it created a job/action, once a job/action is created must not check others anymore to avoid conflicts
    public virtual bool OnStartPerformGoapAction(GoapAction action, ref bool willStillContinueAction) { return false; } //returns true or false if it created a job/action, once a job/action is created must not check others anymore to avoid conflicts
    public virtual void TriggerFlaw(Character character) {
        int manaCost = GetTriggerFlawManaCost(character); ;
        PlayerManager.Instance.player.AdjustMana(-manaCost);
        if (character.trapStructure.structure != null) {
            //clear all trap structures when triggering flaw
            character.trapStructure.SetStructureAndDuration(null, 0);
        }
    }
    /// <summary>
    /// This checks if this flaw can be triggered. This checks both the requirements of the individual traits,
    /// and the mana cost. This is responsible for enabling/disabling the trigger flaw buttton.
    /// </summary>
    /// <param name="character">The character whose flaw will be triggered</param>
    /// <returns>true or false</returns>
    public virtual bool CanFlawBeTriggered(Character character) {
        //return true;
        int manaCost = GetTriggerFlawManaCost(character);
       
        return PlayerManager.Instance.player.mana >= manaCost 
            && character.GetTraitOf(TRAIT_TYPE.DISABLER) == null //disabled characters cannot be triggered
            && character.GetNormalTrait("Blessed") == null
            && !character.currentParty.icon.isTravellingOutside; //characters travelling outside cannot be triggered
    }
    public virtual string GetRequirementDescription(Character character) {
        return "Mana cost of triggering this flaw's negative effect depends on the character's mood. The darker the mood, the cheaper the cost.";
    }
    public virtual List<string> GetCannotTriggerFlawReasons(Character character) {
        List<string> reasons = new List<string>();
        if (PlayerManager.Instance.player.mana < GetTriggerFlawManaCost(character)) {
            reasons.Add("You do not have enough mana.");
        }
        if (character.GetNormalTrait("Blessed") != null) {
            reasons.Add("Blessed characters cannot be targeted by Trigger Flaw.");
        }
        if (character.GetTraitOf(TRAIT_TYPE.DISABLER) != null) {
            reasons.Add("Inactive characters cannot be targeted by Trigger Flaw.");
        }
        return reasons;

    }
    #endregion

    #region Utilities
    public int GetTriggerFlawManaCost(Character character) {
        //Triggering while in a bad mood costs more Mana (100) than triggering while in a dark mood (50). Great and good mood costs 200 mana.
        if (character.currentMoodType == CHARACTER_MOOD.BAD) {
            return 25;
        } else if (character.currentMoodType == CHARACTER_MOOD.DARK) {
            return 10;
        } else {
            return 50; //great or good
        }
    }
    public string GetTriggerFlawEffectDescription(Character character) {
        if (LocalizationManager.Instance.HasLocalizedValue("Trait", this.GetType().ToString(), "flaw_effect")) {
            Log log = new Log(GameManager.Instance.Today(), "Trait", this.GetType().ToString(), "flaw_effect");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            return Utilities.LogReplacer(log);
        }
        return string.Empty;
    }
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
        if (responsibleCharacters == null) {
            responsibleCharacters = new List<Character>();
        }
        if (character != null && !responsibleCharacters.Contains(character)) {
            responsibleCharacters.Add(character);
        }
    }
    public bool IsResponsibleForTrait(Character character) {
        if (responsibleCharacter == character) {
            return true;
        } else if (responsibleCharacters != null) {
            return responsibleCharacters.Contains(character);
        }
        return false;
    }
    public void SetExpiryTicket(ITraitable obj, string expiryTicket) {
        if (expiryTickets == null) {
            expiryTickets = new Dictionary<ITraitable, string>();
        }
        if (!expiryTickets.ContainsKey(obj)) {
            expiryTickets.Add(obj, expiryTicket);
        } else {
            expiryTickets[obj] = expiryTicket;
        }
    }
    public void RemoveExpiryTicket(ITraitable traitable) {
        if (expiryTickets != null) {
            expiryTickets.Remove(traitable);
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
    /// <summary>
    /// Is this trait mutually exclusive with the given trait? (Can they both exist in one object)
    /// </summary>
    /// <param name="otherTrait">The trait to compare with</param>
    /// <returns>True or false</returns>
    public bool IsMutuallyExclusiveWith(string otherTrait) {
        for (int i = 0; i < mutuallyExclusive.Length; i++) {
            if (mutuallyExclusive[i].ToLower() == otherTrait.ToLower()) {
                return true;
            }
        }
        return false;
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