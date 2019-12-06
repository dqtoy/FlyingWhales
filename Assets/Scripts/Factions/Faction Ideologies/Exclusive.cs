using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exclusive : FactionIdeology {
    private const string GENDER = "Gender";
    private const string RACE = "Race";
    private const string TRAIT = "Trait";

    public string identifier { get; private set; }
    public RACE raceRequirement { get; private set; }
    public GENDER genderRequirement { get; private set; }
    public string traitRequirement { get; private set; }

    public Exclusive() : base(FACTION_IDEOLOGY.EXCLUSIVE) {

    }

    #region Overrides
    public override void SetRequirements(Faction faction) {
        int chance = UnityEngine.Random.Range(0, 3);
        //identifier = TRAIT;
        //if(chance == 0) {
        //    identifier = GENDER;
        //    genderRequirement = faction.leader.gender;
        //} else if (chance == 1) {
        //    identifier = RACE;
        //    raceRequirement = faction.leader.race;
        //} else {
            identifier = TRAIT;
            traitRequirement = GetRandomTraitRequirement();
        //}
    }
    public override bool DoesCharacterFitIdeology(Character character) {
        if(identifier == GENDER) {
            return character.gender == genderRequirement;
        } else if (identifier == RACE) {
            return character.race == raceRequirement;
        }
        return character.traitContainer.GetNormalTrait(traitRequirement) != null;
    }
    public override string GetRequirementsForJoiningAsString() {
        return identifier + ": " + GetRequirementAsString();
    }

    #endregion

    private string GetRequirementAsString() {
        if (identifier == GENDER) {
            return genderRequirement.ToString();
        } else if (identifier == RACE) {
            return raceRequirement.ToString();
        }
        return traitRequirement;
    }
    private string GetRandomTraitRequirement() {
        int chance = UnityEngine.Random.Range(0, 3);
        if (chance == 0) {
            return "Worker";
        } else if (chance == 1) {
            return "Combatant";
        }
        return "Royalty";
    }
}
