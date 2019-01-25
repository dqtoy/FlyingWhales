using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dwelling : LocationStructure {

    public List<Character> residents { get; private set; }

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

    public override bool IsOccupied() {
        return residents.Count > 0;
    }

    public bool CanBeResidentHere(Character character) {
        //TODO: Add all relationship checking here
        if (residents.Count == 0) {
            return true;
        }
        return false;
    }
}
