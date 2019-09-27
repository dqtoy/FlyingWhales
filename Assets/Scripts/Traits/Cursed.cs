using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursed : Trait {
    public ITraitable sourcePOI { get; private set; }
    public List<CursedInteraction> cursedInteractions { get; private set; }

    public Cursed() {
        name = "Cursed";
        description = "This character has been afflicted by a magical curse.";
        type = TRAIT_TYPE.ENCHANTMENT;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = GameManager.ticksPerDay;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DISPEL_MAGIC, };
        cursedInteractions = new List<CursedInteraction>();
        //effects = new List<TraitEffect>();
    }

    public void SetCursedInteractions(List<CursedInteraction> cursedInteractions) {
        this.cursedInteractions = cursedInteractions;
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        sourcePOI = sourceCharacter;
        if (sourcePOI is Character) {
            Character character = sourcePOI as Character;
            //_sourceCharacter.CreateRemoveTraitJob(name);
            character.AddTraitNeededToBeRemoved(this);
            character.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
        }else if(sourcePOI is TileObject) {
            Messenger.AddListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedInteraction);
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        if(sourcePOI is Character) {
            Character character = sourcePOI as Character;
            character.CancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
            character.RemoveTraitNeededToBeRemoved(this);
            character.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        } else if (sourceCharacter is TileObject) {
            Messenger.RemoveListener<Character, GoapAction, string>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedInteraction);
        }
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM_GOAP, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
                job.SetCanBeDoneInLocation(true);
                if (InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, job)) {
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                } else {
                    if (!IsResponsibleForTrait(characterThatWillDoJob)) {
                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveTraitJob);
                        characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    }
                    return false;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    
    #endregion

    public void Interact(Character characterThatInteracted, GoapAction actionDone) {
        string result = string.Empty;
        int chance = UnityEngine.Random.Range(0, 100);
        if (level == 1) {
            if(chance < 80) {
                result = "injury";
            } else {
                result = "death";
            }
        } else if (level == 2) {
            if (chance < 50) {
                result = "injury";
            } else if (chance < 30) {
                result = "death";
            } else {
                result = "flaw";
            }
        } else if (level == 3) {
            if (chance < 30) {
                result = "injury";
            } else if (chance < 30) {
                result = "death";
            } else if (chance < 20) {
                result = "flaw";
            } else {
                result = "losebuff";
            }
        }
        cursedInteractions.Add(new CursedInteraction() { characterThatInteracted = characterThatInteracted, actionDone = actionDone.goapType, result = result });
    }
    private void OnCharacterFinishedInteraction(Character character, GoapAction action, string result) {
        for (int i = 0; i < cursedInteractions.Count; i++) {
            CursedInteraction interaction = cursedInteractions[i];
            if(interaction.characterThatInteracted == character && interaction.actionDone == action.goapType) {
                InteractionEffectApplication(character, interaction.result);
                cursedInteractions.RemoveAt(i);
                break;
            }
        }
    }
    private void InteractionEffectApplication(Character character, string result) {
        if(result == "injury") {
            InjureCharacter(character);
        } else if (result == "death") {
            DeathCharacter(character);
        } else if (result == "flaw") {
            FlawCharacter(character);
        } else if (result == "losebuff") {
            LoseBuffCharacter(character);
        }
    }
    private void InjureCharacter(Character character) {
        character.AddTrait("Injured");
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cursed_injury");
        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(sourcePOI, sourcePOI.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //log.AddLogToInvolvedObjects();
        character.RegisterLogAndShowNotifToThisCharacterOnly(log, null, false);
    }
    private void DeathCharacter(Character character) {
        character.Death();
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cursed_death");
        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(sourcePOI, sourcePOI.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //log.AddLogToInvolvedObjects();
        character.RegisterLogAndShowNotifToThisCharacterOnly(log, null, false);
    }
    private void FlawCharacter(Character character) {
        List<Trait> flawTraits = AttributeManager.Instance.GetAllTraitsOfType(TRAIT_TYPE.FLAW);
        string chosenFlawTraitName = string.Empty;
        if (flawTraits.Count > 0) {
            chosenFlawTraitName = flawTraits[UnityEngine.Random.Range(0, flawTraits.Count)].name;
        }
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cursed_flaw");
        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(sourcePOI, sourcePOI.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddToFillers(null, chosenFlawTraitName, LOG_IDENTIFIER.STRING_1);
        //log.AddLogToInvolvedObjects();
        character.RegisterLogAndShowNotifToThisCharacterOnly(log, null, false);

        character.AddTrait(chosenFlawTraitName);
    }
    private void LoseBuffCharacter(Character character) {
        List<Trait> characterBuffTraits = character.GetTraitsOf(TRAIT_TYPE.BUFF);
        if(characterBuffTraits.Count > 0) {
            Trait chosenBuffTrait = characterBuffTraits[UnityEngine.Random.Range(0, characterBuffTraits.Count)];

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cursed_losebuff");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(sourcePOI, sourcePOI.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(null, chosenBuffTrait.name, LOG_IDENTIFIER.STRING_1);
            //log.AddLogToInvolvedObjects();
            character.RegisterLogAndShowNotifToThisCharacterOnly(log, null, false);

            character.RemoveTrait(chosenBuffTrait);
        } else {
            //If there is no buff trait to lose, injure character instead
            InjureCharacter(character);
        }
    }
}

public class SaveDataCursed : SaveDataTrait {
    public List<CursedInteraction> cursedInteractions;

    public override void Save(Trait trait) {
        base.Save(trait);
        Cursed derivedTrait = trait as Cursed;
        cursedInteractions = derivedTrait.cursedInteractions;
    }

    public override Trait Load(ref Character responsibleCharacter) {
        Trait trait = base.Load(ref responsibleCharacter);
        Cursed derivedTrait = trait as Cursed;
        derivedTrait.SetCursedInteractions(cursedInteractions);
        return trait;
    }
}

[System.Serializable]
public struct CursedInteraction {
    public Character characterThatInteracted;
    public INTERACTION_TYPE actionDone;
    public string result;
}
