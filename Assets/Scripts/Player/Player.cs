﻿using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Player : ILeader{

    private int _corruption;
    public Faction playerFaction { get; private set; }
    public Area playerArea { get; private set; }
    public int snatchCredits { get; private set; }

    private int _threatLevel;
    private int _redMagic;
    private int _blueMagic;
    private int _greenMagic;
    private Intel _currentlySelectedIntel;
    private List<CharacterAction> _actions;
    private List<Character> _snatchedCharacters;
    private List<Intel> _intels;

    //#region getters/setters
    //public Area playerArea {
    //    get { return _playerArea; }
    //}
    //#endregion

    #region getters/setters
    public int id {
        get { return -645; }
    }
    public string name {
        get { return "Player"; }
    }
    public int redMagic {
        get { return _redMagic; }
    }
    public int blueMagic {
        get { return _blueMagic; }
    }
    public int greenMagic {
        get { return _greenMagic; }
    }
    public int threatLevel {
        get { return _threatLevel; }
    }
    public Intel currentlySelectedIntel {
        get { return _currentlySelectedIntel; }
    }
    public List<CharacterAction> actions {
        get { return _actions; }
    }
    public List<Intel> intels {
        get { return _intels; }
    }
    #endregion

    public Player() {
        _corruption = 0;
        playerArea = null;
        snatchCredits = 0;
        SetRedMagic(50);
        SetBlueMagic(50);
        SetGreenMagic(50);
        SetThreatLevel(20);
        _snatchedCharacters = new List<ECS.Character>();
        _intels = new List<Intel>();
        //Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_ADDED, OnTileAddedToPlayerArea);
        Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_REMOVED, OnTileRemovedFromPlayerArea);
        Messenger.AddListener<Character>(Signals.CHARACTER_RELEASED, OnCharacterReleased);
        //ConstructPlayerActions();
    }

    #region Corruption
    public void AdjustCorruption(int adjustment) {
        _corruption += adjustment;
    }
    #endregion

    #region Area
    public void CreatePlayerArea(HexTile chosenCoreTile) {
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION);
        BaseLandmark demonicPortal = LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        chosenCoreTile.SetCorruption(true);
        SetPlayerArea(playerArea);
        ActivateMagicTransferToPlayer();
        //OnTileAddedToPlayerArea(playerArea, chosenCoreTile);
    }
    private void SetPlayerArea(Area area) {
        playerArea = area;
    }
    //private void OnTileAddedToPlayerArea(Area affectedArea, HexTile addedTile) {
    //    if (playerArea != null && affectedArea.id == playerArea.id) {
    //        addedTile.SetBaseSprite(PlayerManager.Instance.playerAreaFloorSprites[Random.Range(0, PlayerManager.Instance.playerAreaFloorSprites.Length)]);
    //    }
    //}
    private void OnTileRemovedFromPlayerArea(Area affectedArea, HexTile removedTile) {
        if (playerArea != null && affectedArea.id == playerArea.id) {
            Biomes.Instance.UpdateTileVisuals(removedTile);
        }
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction playerFaction = FactionManager.Instance.CreateNewFaction(true);
        playerFaction.SetLeader(this);
        SetPlayerFaction(playerFaction);
    }
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    #endregion

    #region Snatch
    public void AdjustSnatchCredits(int adjustment) {
        snatchCredits += adjustment;
        snatchCredits = Mathf.Max(0, snatchCredits);
    }
    public void SnatchCharacter(ECS.Character character) {
        AdjustSnatchCredits(-1);
        //check for success
        if (IsSnatchSuccess(character)) {
            if (!_snatchedCharacters.Contains(character)) {
                _snatchedCharacters.Add(character);
                character.OnThisCharacterSnatched();
                //ReleaseCharacterQuest rcq = new ReleaseCharacterQuest(character); //create quest
                //QuestManager.Instance.CreateQuest(QUEST_TYPE.RELEASE_CHARACTER, character);
                Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Successfully snatched " + character.name, MESSAGE_BOX_MODE.MESSAGE_ONLY, true);
                Debug.Log("Snatched " + character.name);
                Messenger.Broadcast(Signals.CHARACTER_SNATCHED, character);
            }
        } else {
            Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Failed to snatch " + character.name, MESSAGE_BOX_MODE.MESSAGE_ONLY, true);
            Debug.Log("Failed to snatch " + character.name);
        }
        
    }
    private bool IsSnatchSuccess(ECS.Character character) {
        return true;
        if (character.role == null) {
            return true;
        }
        if (character.role is Civilian) {
            return true; //100%
        } else if (character.role.roleType == CHARACTER_ROLE.HERO) {
            int chance = 50; //low happiness
            if (character.role.happiness >= 100f) {
                chance = 25; //high happiness
            }
            if (Random.Range(0, 100) < chance) {
                return true;
            }
        } else if (character.role.roleType == CHARACTER_ROLE.KING) {
            int chance = 25; //low happiness
            if (character.role.happiness >= 100f) {
                chance = 10; //high happiness
            }
            if (Random.Range(0, 100) < chance) {
                return true;
            }
        }
        return false;
    }
    public bool IsCharacterSnatched(ECS.Character character) {
        return _snatchedCharacters.Contains(character);
    }
    public BaseLandmark GetAvailableSnatcherLair() {
        for (int i = 0; i < playerArea.landmarks.Count; i++) {
            BaseLandmark currLandmark = playerArea.landmarks[i];
            if (currLandmark.specificLandmarkType == LANDMARK_TYPE.SNATCHER_DEMONS_LAIR) {
                if (!HasSnatchedCharacter(currLandmark)) {
                    return currLandmark;
                }
            }
        }
        return null;
    }
    public bool HasSnatchedCharacter(BaseLandmark landmark) {
        for (int i = 0; i < _snatchedCharacters.Count; i++) {
            ECS.Character currCharacter = _snatchedCharacters[i];
            if (landmark.charactersAtLocation.Contains(currCharacter.party)) {
                return true;
            }
        }
        return false;
    }
    private void OnCharacterReleased(Character releasedCharacter) {
        _snatchedCharacters.Remove(releasedCharacter);
    }
    #endregion

    #region Actions
    private void ConstructPlayerActions() {
        _actions = new List<CharacterAction>();
        _actions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.MOVE));
        for (int i = 0; i < _actions.Count; i++) {
            _actions[i].Initialize();
            GameObject go = GameObject.Instantiate(UIManager.Instance.playerActionsUI.playerActionsBtnPrefab, UIManager.Instance.playerActionsUI.playerActionsContentTransform);
            go.GetComponent<PlayerActionBtn>().SetAction(_actions[i]);
        }
    }
    #endregion

    #region Magic
    private void ActivateMagicTransferToPlayer() {
        Messenger.AddListener(Signals.HOUR_STARTED, TransferMagicToPlayer);
    }
    private void TransferMagicToPlayer() {
        AdjustRedMagic(1);
        AdjustBlueMagic(1);
        AdjustGreenMagic(1);
    }
    public void SetRedMagic(int amount) {
        _redMagic = amount;
    }
    public void SetBlueMagic(int amount) {
        _blueMagic = amount;
    }
    public void SetGreenMagic(int amount) {
        _greenMagic = amount;
    }
    public void AdjustRedMagic(int amount) {
        _redMagic += amount;
        _redMagic = Mathf.Clamp(_redMagic, 0, 100);
    }
    public void AdjustBlueMagic(int amount) {
        _blueMagic += amount;
        _blueMagic = Mathf.Clamp(_blueMagic, 0, 100);
    }
    public void AdjustGreenMagic(int amount) {
        _greenMagic += amount;
        _greenMagic = Mathf.Clamp(_greenMagic, 0, 100);
    }
    #endregion

    #region Threat
    public void SetThreatLevel(int amount) {
        _threatLevel = amount;
    }
    public void AdjustThreatLevel(int amount) {
        _threatLevel += amount;
        _threatLevel = Mathf.Clamp(_threatLevel, 0, 100);
    }
    #endregion

    #region Intel
    public void AddIntel(Intel intel) {
        if (!_intels.Contains(intel)) {
            _intels.Add(intel);
        }
    }
    public bool RemoveIntel(Intel intel) {
        return _intels.Remove(intel);
    }
    public void PickIntelToGiveToCharacter(Character character, ShareIntel shareIntelAbility) {
        //TODO
    }
    private void GiveIntelToCharacter(Character character, ShareIntel shareIntelAbility) {
        if (character.intelReactions.ContainsKey(_currentlySelectedIntel.id)) {
            GameEvent gameEvent = EventManager.Instance.AddNewEvent(character.intelReactions[_currentlySelectedIntel.id]);
            shareIntelAbility.HasGivenIntel(character);
        }
    }
    #endregion
}
