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
        InitializeFacilities();
    }

    #region Residents
    public void AddResident(Character character) {
        if (!residents.Contains(character)) {
            residents.Add(character);
            character.SetHomeStructure(this);
            List<TileObject> objs = GetTileObjects();
            for (int i = 0; i < objs.Count; i++) {
                TileObject obj = objs[i];
                obj.UpdateOwners();
            }
        }
    }
    public void RemoveResident(Character character) {
        if (residents.Remove(character)) {
            character.SetHomeStructure(null);
            List<TileObject> objs = GetTileObjects();
            for (int i = 0; i < objs.Count; i++) {
                TileObject obj = objs[i];
                obj.UpdateOwners();
            }
        }
    }
    public bool IsResident(Character character) {
        return residents.Contains(character);
        //for (int i = 0; i < residents.Count; i++) {
        //    if(residents[i].id == character.id) {
        //        return true;
        //    }
        //}
        //return false;
    }
    public override bool IsOccupied() {
        return residents.Count > 0;
    }
    public bool CanBeResidentHere(Character character) {
        //if (this.IsFull()) {
        //    return false;
        //}
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
    public bool HasPositiveRelationshipWithAnyResident(Character character) {
        if (residents.Contains(character)) {
            return true; //if the provided character is a resident of this dwelling, then yes, consider relationship as positive
        }
        for (int i = 0; i < residents.Count; i++) {
            Character currResident = residents[i];
            RELATIONSHIP_EFFECT effect = character.GetRelationshipEffectWith(currResident);
            if (effect == RELATIONSHIP_EFFECT.POSITIVE) {
                return true;
            }
        }
        return false;
    }
    public bool HasEnemyOrNoRelationshipWithAnyResident(Character character) {
        //if (residents.Contains(character)) {
        //    return true; //if the provided character is a resident of this dwelling, then yes, consider relationship as positive
        //}
        for (int i = 0; i < residents.Count; i++) {
            Character currResident = residents[i];
            RELATIONSHIP_EFFECT effect = character.GetRelationshipEffectWith(currResident);
            if (effect == RELATIONSHIP_EFFECT.NEGATIVE || effect == RELATIONSHIP_EFFECT.NONE) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Misc
    public override string GetNameRelativeTo(Character character) {
        if (character.homeStructure == this) {
            //- Dwelling where Actor Resides: "at [his/her] home"
            return Utilities.GetPronounString(character.gender, PRONOUN_TYPE.POSSESSIVE, false) + " home";
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
                return residentSummary + "' home";
            }
            return residentSummary + "'s home";
        } else {
            //- Dwelling where no one resides: "at an empty house"
            return "an empty house";
        }
    }
    #endregion

    #region Facilities
    protected override void InitializeFacilities() {
        facilities = new Dictionary<FACILITY_TYPE, int>();
        FACILITY_TYPE[] facilityTypes = Utilities.GetEnumValues<FACILITY_TYPE>();
        for (int i = 0; i < facilityTypes.Length; i++) {
            if (facilityTypes[i] != FACILITY_TYPE.NONE) {
                facilities.Add(facilityTypes[i], 0);
            }
        }
    }
    #endregion

}
