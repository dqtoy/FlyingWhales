using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Intel {
    protected bool _isObtained;

    #region getters/setters
    public bool isObtained {
        get { return _isObtained; }
    }
    #endregion

    public Intel() {
        _isObtained = false;
    }
    public void SetObtainedState(bool state) {
        _isObtained = state;
    }
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

    public FactionIntel(Faction faction) : base() {
        this.faction = faction;
    }

    public override string ToString() {
        return faction.name + " Intel";
    }
}

public class LocationIntel : Intel {
    public Area location;

    public LocationIntel(Area location) : base() {
        this.location = location;
    }

    public override string ToString() {
        return location.name + " Intel";
    }
}

public class CharacterIntel : Intel {
    public ICharacter character;

    public CharacterIntel(ICharacter character) : base() {
        this.character = character;
    }
    public override string ToString() {
        return character.name + " Intel";
    }
}

public class DefenderIntel : Intel {
    public Party defenderParty;

    public DefenderIntel(Party defenderParty) : base() {
        this.defenderParty = defenderParty;
    }
    public override string ToString() {
        return defenderParty.name + " Intel";
    }
}