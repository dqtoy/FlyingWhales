using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Token {
    protected bool _isObtained;

    #region getters/setters
    public bool isObtained {
        get { return _isObtained; }
    }
    #endregion

    public Token() {
        _isObtained = false;
    }
    public void SetObtainedState(bool state) {
        _isObtained = state;
    }
    public void ConsumeToken() {
        SetObtainedState(false);
        Messenger.Broadcast(Signals.TOKEN_CONSUMED, this);
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

public class FactionToken : Token{
    public Faction faction;

    public FactionToken(Faction faction) : base() {
        this.faction = faction;
    }

    public override string ToString() {
        return faction.name + "'s Token";
    }
}

public class LocationToken : Token {
    public Area location;

    public LocationToken(Area location) : base() {
        this.location = location;
    }

    public override string ToString() {
        return location.name + " Token";
    }
}

public class CharacterToken : Token {
    public Character character;

    public CharacterToken(Character character) : base() {
        this.character = character;
    }
    public override string ToString() {
        return character.name + "'s Token";
    }
}

[System.Serializable]
/*
 NOTE: There is only one instance of SpecialToken class per special token. (See TokenManager)
     */
public class SpecialToken : Token { 
    public string name;
    public int quantity;
    public SpecialToken(string name) : base() {
        this.name = name;
    }
    public void AdjustQuantity(int amount) {
        quantity += amount;
        if (quantity <= 0) {
            Messenger.Broadcast(Signals.SPECIAL_TOKEN_RAN_OUT, this);
        }
    }
    public override string ToString() {
        return name + " Token";
    }
}

public class DefenderToken : Token {
    public Area owner;

    public DefenderToken(Area owner) : base() {
        this.owner = owner;
    }
    public override string ToString() {
        return owner.name + "'s Defenders";
    }
}