using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public virtual void CreateJointInteractionStates(Interaction interaction, Character user, object target) { }
    public virtual bool CanBeUsedBy(Character character) { return false; }
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

public class SpecialToken : Token, IPointOfInterest {
    public string name { get; private set; }
    public SPECIAL_TOKEN specialTokenType;
    public INTERACTION_TYPE npcAssociatedInteractionType;
    //public int quantity;
    public int weight;
    public Faction owner;
    public LocationStructure structureLocation { get; private set; }
    public InteractionAttributes interactionAttributes { get; protected set; }
    public List<INTERACTION_TYPE> poiGoapActions { get; private set; }
    public int supplyValue { get; private set; }
    public int craftCost { get; private set; }
    public int purchaseCost { get; private set; }

    private LocationGridTile tile;
    private POI_STATE _state;

    #region getters/setters
    public override string tokenName {
        get { return name; }
    }
    public virtual string Item_Used {
        get { return "Item Used"; }
    }
    public virtual string Stop_Fail {
        get { return "Stop Fail"; }
    }
    public string ownerName {
        get {
            if (owner == null) {
                return "no one";
            } else {
                return owner.name;
            }
        }
    }
    public POINT_OF_INTEREST_TYPE poiType {
        get { return POINT_OF_INTEREST_TYPE.ITEM; }
    }
    public LocationGridTile gridTileLocation {
        get { return tile; }
    }
    public POI_STATE state {
        get { return _state; }
    }
    #endregion

    public SpecialToken(SPECIAL_TOKEN specialTokenType, int appearanceRate) : base() {
        _tokenType = TOKEN_TYPE.SPECIAL;
        this.specialTokenType = specialTokenType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.specialTokenType.ToString());
        weight = appearanceRate;
        npcAssociatedInteractionType = INTERACTION_TYPE.NONE;
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.PICK_ITEM };

        if(specialTokenType == SPECIAL_TOKEN.TOOL) {
            supplyValue = 15;
            craftCost = 25;
            purchaseCost = 35;
        } else {
            supplyValue = 0;
            craftCost = 0;
            purchaseCost = 0;
        }
    }
    //public void AdjustQuantity(int amount) {
    //    quantity += amount;
    //    if (quantity <= 0) {
    //        Messenger.Broadcast(Signals.SPECIAL_TOKEN_RAN_OUT, this);
    //    }
    //}

    #region Virtuals
    public virtual Character GetTargetCharacterFor(Character sourceCharacter) {
        return null;
    }
    public virtual bool CanBeUsedForTarget(Character sourceCharacter, Character targetCharacter) { return false; }
    public virtual void OnObtainToken(Character character) { }
    public virtual void OnUnobtainToken(Character character) { }
    public virtual void OnConsumeToken(Character character) { }
    public virtual void StartTokenInteractionState(Character user, Character target) {
        user.MoveToAnotherStructure(target.currentStructure, target.GetNearestUnoccupiedTileFromThis());
    }
    #endregion

    public void SetOwner(Faction owner) {
        this.owner = owner;
    }
    public void SetStructureLocation(LocationStructure structureLocation) {
        this.structureLocation = structureLocation;
    }
    public override string ToString() {
        return name;
    }

    #region Area Map
    public void SetGridTileLocation(LocationGridTile tile) {
        this.tile = tile;
    }
    //public LocationGridTile GetNearestUnoccupiedTileFromThis() {
    //    if (gridTileLocation != null && structureLocation == structure) {
    //        List<LocationGridTile> choices = structureLocation.unoccupiedTiles.Where(x => x != gridTileLocation).OrderBy(x => Vector2.Distance(gridTileLocation.localLocation, x.localLocation)).ToList();
    //        if (choices.Count > 0) {
    //            LocationGridTile nearestTile = choices[0];
    //            if (otherCharacter.currentStructure == structure && otherCharacter.gridTileLocation != null) {
    //                float ogDistance = Vector2.Distance(this.gridTileLocation.localLocation, otherCharacter.gridTileLocation.localLocation);
    //                float newDistance = Vector2.Distance(this.gridTileLocation.localLocation, nearestTile.localLocation);
    //                if (newDistance > ogDistance) {
    //                    return otherCharacter.gridTileLocation; //keep the other character's current tile
    //                }
    //            }
    //            return nearestTile;
    //        }
    //    }
    //    return null;
    //}
    public LocationGridTile GetNearestUnoccupiedTileFromThis() {
        if (gridTileLocation != null) {
            List<LocationGridTile> unoccupiedNeighbours = gridTileLocation.UnoccupiedNeighbours;
            if (unoccupiedNeighbours.Count == 0) {
                return null;
            } else {
                return unoccupiedNeighbours[Random.Range(0, unoccupiedNeighbours.Count)];
            }
        }
        return null;
    }
    #endregion

    #region Point Of Interest
    public List<GoapAction> AdvertiseActionsToActor(Character actor, List<INTERACTION_TYPE> actorAllowedInteractions) {
        if (poiGoapActions != null && poiGoapActions.Count > 0) {
            List<GoapAction> usableActions = new List<GoapAction>();
            for (int i = 0; i < poiGoapActions.Count; i++) {
                if (actorAllowedInteractions.Contains(poiGoapActions[i])) {
                    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(poiGoapActions[i], actor, this);
                    if (goapAction.CanSatisfyRequirements()) {
                        usableActions.Add(goapAction);
                    }
                }
            }
            return usableActions;
        }
        return null;
    }
    public void SetPOIState(POI_STATE state) {
        _state = state;
    }
    #endregion
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