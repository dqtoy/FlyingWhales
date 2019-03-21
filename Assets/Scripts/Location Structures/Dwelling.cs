using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dwelling : LocationStructure {

    public List<Character> residents { get; private set; }

    public Character owner {
        get { return residents.ElementAtOrDefault(0); }
    }

    public Dwelling(Area location, bool isInside) 
        : base(STRUCTURE_TYPE.DWELLING, location, isInside) {
        residents = new List<Character>();
    }

    public void AddResident(Character character) {
        if (!residents.Contains(character)) {
            residents.Add(character);
            character.SetHomeStructure(this);
        }
    }
    public void RemoveResident(Character character) {
        if (residents.Remove(character)) {
            character.SetHomeStructure(null);
        }
    }
    public bool IsResident(Character character) {
        for (int i = 0; i < residents.Count; i++) {
            if(residents[i].id == character.id) {
                return true;
            }
        }
        return false;
    }

    public override bool IsOccupied() {
        return residents.Count > 0;
    }

    public bool CanBeResidentHere(Character character) {
        if (this.IsFull()) {
            return false;
        }
        if (residents.Count == 0) {
            return true;
        } else {
            for (int i = 0; i < residents.Count; i++) {
                Character currResident = residents[i];
                List<RELATIONSHIP_TRAIT> rels = currResident.GetAllRelationshipTraitTypesWith(character);
                if (rels != null && rels.Contains(RELATIONSHIP_TRAIT.LOVER)) {
                    return true;
                }
            }
        }
        return false;
    }

    public override string GetNameRelativeTo(Character character) {
        if (character.homeStructure == this) {
            //- Dwelling where Actor Resides: "at [his/her] home"
            return "at " + Utilities.GetPronounString(character.gender, PRONOUN_TYPE.POSSESSIVE, false) + " home";
        } else if (residents.Count > 0) {
            //- Dwelling where Someone else Resides: "at [Resident Name]'s home"
            string residentSummary = residents[0].name;
            for (int i = 1; i < residents.Count; i++) {
                if (i + 1 == residents.Count) {
                    residentSummary += " and ";
                } else {
                    residentSummary += ", ";
                }
                residentSummary += residents[i].name;
            }
            if (residentSummary.Last() == 's') {
                return "at " + residentSummary + "' home";
            }
            return "at " + residentSummary + "'s home";
        } else {
            //- Dwelling where no one resides: "at an empty house"
            return "at an empty house";
        }
    }
}
