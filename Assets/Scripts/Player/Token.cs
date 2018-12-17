using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Token {
    protected bool _isObtainedByPlayer;
    protected TOKEN_TYPE _tokenType;

    #region getters/setters
    public virtual string tokenName {
        get { return string.Empty; }
    }
    public string nameInBold {
        get { return "<b>" + tokenName + "</b>"; }
    }
    public bool isObtainedByPlayer {
        get { return _isObtainedByPlayer; }
    }
    public TOKEN_TYPE tokenType {
        get { return _tokenType; }
    }
    #endregion

    public Token() {
        _isObtainedByPlayer = false;
    }
    public void SetObtainedState(bool state) {
        _isObtainedByPlayer = state;
    }
    /*
     NOTE: Only use this when the player consumes this token.
     If a character consumes this token, use ConsumeToken(Token) in that characters instance.
         */
    public void PlayerConsumeToken() {
        SetObtainedState(false);
        Messenger.Broadcast(Signals.TOKEN_CONSUMED, this);
    }
    #region Virtuals
    public virtual void CreateJointInteractionStates(Interaction interaction) { }
    public virtual bool CanBeUsed() { return true; }
    #endregion
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

    #region getters/setters
    public override string tokenName {
        get { return faction.name; }
    }
    #endregion

    public FactionToken(Faction faction) : base() {
        _tokenType = TOKEN_TYPE.FACTION;
        this.faction = faction;
    }

    public override string ToString() {
        return faction.name + "'s Token";
    }
}

public class LocationToken : Token {
    public Area location;

    #region getters/setters
    public override string tokenName {
        get { return location.name; }
    }
    #endregion

    public LocationToken(Area location) : base() {
        _tokenType = TOKEN_TYPE.LOCATION;
        this.location = location;
    }

    public override string ToString() {
        return location.name + " Token";
    }
}

public class CharacterToken : Token {
    public Character character;

    #region getters/setters
    public override string tokenName {
        get { return character.name; }
    }
    #endregion

    public CharacterToken(Character character) : base() {
        _tokenType = TOKEN_TYPE.CHARACTER;
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
    public SPECIAL_TOKEN specialTokenType;
    public string name;
    public int quantity;

    #region getters/setters
    public override string tokenName {
        get { return name; }
    }
    #endregion

    public SpecialToken(SPECIAL_TOKEN specialTokenType) : base() {
        _tokenType = TOKEN_TYPE.SPECIAL;
        this.specialTokenType = specialTokenType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.specialTokenType.ToString());
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