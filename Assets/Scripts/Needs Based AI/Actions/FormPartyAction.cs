using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormPartyAction : CharacterAction {

    protected List<ICharacter> joiningCharacters;
    protected CharacterParty party;
    protected int minimumDuration;

    private enum TileType {
        S_Tile_Of_Faction,
        S_Tile_Of_Non_Hostile_Faction,
        Deadend_Tile,
        Non_Deadend,
        Tile_Not_Owned_By_Hostile,
        Tile_Owned_By_Hostile,
    }

    public FormPartyAction() : base(ACTION_TYPE.FORM_PARTY) {}

    public override CharacterAction Clone() {
        FormPartyAction action = new FormPartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }

    public override void Initialize() {
        base.Initialize();
    }

    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        joiningCharacters = new List<ICharacter>();
        party = iparty as CharacterParty;
        minimumDuration = 12;
        //When a Squad Leader starts performing a Forming Party action, it will loop through all other party members:
        for (int i = 0; i < iparty.icharacters.Count; i++) {
            ICharacter character = iparty.icharacters[i];
            if (character is ECS.Character) {
                ECS.Character currCharacter = character as ECS.Character;
                if (!currCharacter.IsSquadLeader()) {
                    //If below Happiness, Mental or Physical Point thresholds
                    if (currCharacter.role.happiness < CharacterManager.Instance.HAPPINESS_THRESHOLD
                        && currCharacter.mentalPoints < CharacterManager.Instance.MENTAL_THRESHOLD && currCharacter.mentalPoints < CharacterManager.Instance.PHYSICAL_THRESHOLD) {
                        //end their In Party action and put them out of the party
                        iparty.RemoveCharacter(currCharacter);
                        //currCharacter.party.actionData.EndAction();
                    }
                    //Otherwise, maintain existing In Party action
                }
            }
        }
        //Then, the character will select a safe spot within 3 tile radius of his current location. To determine this, this is the order of priority:
        //check first if the party's current location is already fine
        if (party.specificLocation.tileLocation.areaOfTile == null || party.specificLocation.tileLocation.areaOfTile.owner == null) {
            InviteSquadMembers(iparty.mainCharacter);
            return;
        } else if(party.specificLocation.tileLocation.areaOfTile.owner.id == party.faction.id) {
            InviteSquadMembers(iparty.mainCharacter);
            return;
        }

        ILocation targetLocation = null;
        Dictionary<TileType, List<HexTile>> locationChoices = new Dictionary<TileType, List<HexTile>>() {
            {TileType.S_Tile_Of_Faction, new List<HexTile>()},
            {TileType.S_Tile_Of_Non_Hostile_Faction, new List<HexTile>()},
            {TileType.Deadend_Tile, new List<HexTile>()},
            {TileType.Non_Deadend, new List<HexTile>()},
            {TileType.Tile_Not_Owned_By_Hostile, new List<HexTile>()},
            {TileType.Tile_Owned_By_Hostile, new List<HexTile>()},
        };

        List<HexTile> tilesInRange = iparty.specificLocation.tileLocation.GetTilesInRange(3);
        Faction factionOfParty = iparty.mainCharacter.faction;
        for (int i = 0; i < tilesInRange.Count; i++) {
            HexTile currTile = tilesInRange[i];
            if (!currTile.isPassable) {
                continue; //skip
            }
            if (currTile.areaOfTile != null) {
                Faction factionOfTile = currTile.areaOfTile.owner;
                if (factionOfTile != null && factionOfParty != null) {
                    if (factionOfTile.id == factionOfParty.id) {
                        locationChoices[TileType.S_Tile_Of_Faction].Add(currTile); //Settlement Tiles owned by own Faction
                    } else {
                        FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(factionOfParty, factionOfTile);
                        if (rel != null) {
                            if (rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.NON_HOSTILE) {
                                locationChoices[TileType.S_Tile_Of_Non_Hostile_Faction].Add(currTile); //Settlement Tiles owned by non-hostile Faction
                            } else {
                                locationChoices[TileType.Tile_Owned_By_Hostile].Add(currTile); //Tiles owned by hostile Faction
                            }
                        }
                    }
                }
                if (factionOfTile == null) {
                    locationChoices[TileType.Tile_Not_Owned_By_Hostile].Add(currTile); //Tiles not owned by hostile Faction
                }
            }
            if (currTile.landmarkOnTile == null) {
                if (currTile.passableType == PASSABLE_TYPE.MAJOR_DEADEND || currTile.passableType == PASSABLE_TYPE.MINOR_DEADEND) {
                    locationChoices[TileType.Deadend_Tile].Add(currTile); //Deadend Tiles with no structure
                } else {
                    locationChoices[TileType.Non_Deadend].Add(currTile); //Non-deadend Tiles with no structure
                }
            }
        }

        foreach (KeyValuePair<TileType, List<HexTile>> kvp in locationChoices) {
            if (kvp.Value.Count > 0) {
                HexTile chosenTile = kvp.Value[Random.Range(0, kvp.Value.Count)];
                if (chosenTile.landmarkOnTile != null) {
                    targetLocation = chosenTile.landmarkOnTile;
                } else {
                    targetLocation = chosenTile;
                }
                break;
            }
        }

        if (targetLocation == null) {
            throw new System.Exception("Target location for form party of " + iparty.mainCharacter.name + " is null!");
        }

        iparty.GoToLocation(targetLocation, PATHFINDING_MODE.PASSABLE, () => InviteSquadMembers(iparty.mainCharacter)); //The character will move to the target tile and perform the action for Minimum Duration
        base.OnChooseAction(iparty, targetObject);
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        GiveAllReward(party);
        minimumDuration -= 1;
        if (minimumDuration == 0) {
            if (joiningCharacters.Count == 0) { //if not waiting for anyone, end, else wait for other characters
                EndAction(party, targetObject);
            }
        }
    }
    //Give all provided needs to the character regardless of the amount
    public override void GiveAllReward(CharacterParty party) {
        for (int i = 0; i < party.icharacters.Count; i++) {
            ICharacter icharacter = party.icharacters[i];
            icharacter.role.AdjustFullness(_actionData.providedFullness);
            icharacter.role.AdjustEnergy(_actionData.providedEnergy);
            icharacter.role.AdjustPrestige(_actionData.providedPrestige);
            if (party.icharacters.Count >= 2) { //only if there are at least 2 members in the party
                icharacter.role.AdjustSanity(_actionData.providedSanity);
                icharacter.role.AdjustFun(_actionData.providedFun);
            }
            icharacter.role.AdjustSafety(_actionData.providedSafety);
            if (_actionData.hpRecoveredPercentage != 0f && icharacter.currentHP < icharacter.maxHP) {
                float hpRecovery = (_actionData.hpRecoveredPercentage / 100f) * (float)icharacter.maxHP;
                icharacter.AdjustHP((int)hpRecovery);
            }
        }

    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
        base.EndAction(party, targetObject);
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_JOINED_PARTY)) {
            Messenger.RemoveListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
        }
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_DEATH)) {
            Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
    }
    public override bool ShouldGoToTargetObjectOnChoose() {
        return false;
    }

    private void InviteSquadMembers(ICharacter squadLeader) {
        for (int i = 0; i < squadLeader.squad.squadFollowers.Count; i++) {
            ICharacter follower = squadLeader.squad.squadFollowers[i];
            string text = follower.name + " is invited to join " + squadLeader.name + "'s party";
            if (follower.InviteToParty(squadLeader)) {
                joiningCharacters.Add(follower);
                text += "\n" + follower.name + " accepted the invitation!";
            } else {
                text += "\n" + follower.name + " declined the invitation!";
            }
            Debug.Log(text);
        }
        if (joiningCharacters.Count > 0) {
            Messenger.AddListener<ICharacter, NewParty>(Signals.CHARACTER_JOINED_PARTY, OnCharacterJoinedParty);
            Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
    }

    private void OnCharacterJoinedParty(ICharacter character, NewParty affectedParty) {
        if (this.party.id == affectedParty.id) {
            if (joiningCharacters.Contains(character)) {
                joiningCharacters.Remove(character);
                CheckForEnd();
            }
        }
    }
    private void OnCharacterDied(ECS.Character character) {
        if (joiningCharacters.Contains(character)) {
            joiningCharacters.Remove(character);
            CheckForEnd();
        }
    }
    private void CheckForEnd() {
        if (minimumDuration <= 0 && joiningCharacters.Count == 0) {
            EndAction(party, party.icharacterObject);
        }
    }
}
