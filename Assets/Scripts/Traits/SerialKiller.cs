using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialKiller : Trait {

    public SerialVictim victim1 { get; private set; }
    public SerialVictim victim2 { get; private set; }
    public Character character { get; private set; }

    public SerialKiller() {
        name = "Serial Killer";
        description = "This character is a serial killer.";
        thoughtText = "[Character] is a serial killer.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            character = sourceCharacter as Character;
            GenerateSerialVictims();
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        if (character != null) {
            
        }
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion

    private void GenerateSerialVictims() {
        victim1 = new SerialVictim(RandomizeVictimType(true), RandomizeVictimType(false));
        victim2 = new SerialVictim(RandomizeVictimType(true), RandomizeVictimType(false));

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "became_serial_killer");
        log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, victim1.text, LOG_IDENTIFIER.STRING_1);
        log.AddToFillers(null, victim2.text, LOG_IDENTIFIER.STRING_2);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }

    private SERIAL_VICTIM_TYPE RandomizeVictimType(bool isPrefix) {
        int chance = UnityEngine.Random.Range(0, 2);
        if (isPrefix) {
            if(chance == 0) {
                return SERIAL_VICTIM_TYPE.GENDER;
            }
            return SERIAL_VICTIM_TYPE.ROLE;
        } else {
            if (chance == 0) {
                return SERIAL_VICTIM_TYPE.TRAIT;
            }
            return SERIAL_VICTIM_TYPE.STATUS;
        }
    }
}

public class SerialVictim {
    public SERIAL_VICTIM_TYPE victimFirstType { get; private set; }
    public SERIAL_VICTIM_TYPE victimSecondType { get; private set; }
    public string victimFirstDescription { get; private set; }
    public string victimSecondDescription { get; private set; }

    public string text { get; private set; }

    public SerialVictim(SERIAL_VICTIM_TYPE victimFirstType, SERIAL_VICTIM_TYPE victimSecondType) {
        this.victimFirstType = victimFirstType;
        this.victimSecondType = victimSecondType;
        GenerateVictim();
    }

    private void GenerateVictim() {
        victimFirstDescription = GenerateVictimDescription(victimFirstType);
        victimSecondDescription = GenerateVictimDescription(victimSecondType);
        GenerateText();
    }
    private string GenerateVictimDescription(SERIAL_VICTIM_TYPE victimType) {
        if (victimType == SERIAL_VICTIM_TYPE.GENDER) {
            GENDER gender = Utilities.GetRandomEnumValue<GENDER>();
            return gender.ToString();
        } else if (victimType == SERIAL_VICTIM_TYPE.ROLE) {
            CHARACTER_ROLE[] roles = new CHARACTER_ROLE[] { CHARACTER_ROLE.CIVILIAN, CHARACTER_ROLE.SOLDIER, CHARACTER_ROLE.ADVENTURER };
            return roles[UnityEngine.Random.Range(0, roles.Length)].ToString();
        } else if (victimType == SERIAL_VICTIM_TYPE.STATUS) {
            string[] traits = new string[] { "Craftsman", "Criminal", "Drunk", "Sick", "Lazy", "Hardworking", "Curious" };
            return traits[UnityEngine.Random.Range(0, traits.Length)];
        } else if (victimType == SERIAL_VICTIM_TYPE.TRAIT) {
            string[] statuses = new string[] { "Hungry", "Tired", "Lonely" };
            return statuses[UnityEngine.Random.Range(0, statuses.Length)];
        }
        return string.Empty;
    }
    private void GenerateText() {
        string firstText = string.Empty;
        string secondText = string.Empty;
        if (victimSecondDescription != "Craftsman" && victimSecondDescription != "Criminal") {
            firstText = victimSecondDescription;
            secondText = PluralizeText(Utilities.NormalizeStringUpperCaseFirstLetters(victimFirstDescription));
        } else {
            firstText = Utilities.NormalizeStringUpperCaseFirstLetters(victimFirstDescription);
            secondText = PluralizeText(victimSecondDescription);
        }
        this.text = firstText + " " + secondText;
    }

    private string PluralizeText(string text) {
        string newText = text + "s";
        if (text.EndsWith("man")) {
            newText = text.Replace("man", "men");
        }
        return newText;
    }
}
