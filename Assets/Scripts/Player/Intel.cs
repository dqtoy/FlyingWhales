using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Intel {
    //public int id;
    //public string name;
    //public string description;

    //#region getters/setters
    //public string thisName {
    //    get { return name; }
    //}
    //#endregion
    //public void SetData(IntelComponent intelComponent) {
    //    id = intelComponent.id;
    //    name = intelComponent.thisName;
    //    description = intelComponent.description;
    //}

    //public override string ToString() {
    //    return "[" + id.ToString() + "] " + name + " - " + description; 
    //}
}

public class FactionIntel : Intel{
    public Faction faction;

    public FactionIntel(Faction faction) {
        this.faction = faction;
    }
}

public class LocationIntel : Intel {
    public Area location;

    public LocationIntel(Area location) {
        this.location = location;
    }
}

public class CharacterIntel : Intel {
    public ICharacter character;

    public CharacterIntel(ICharacter character) {
        this.character = character;
    }
}