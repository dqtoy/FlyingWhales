using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;
using System.Linq;

public class Player : ILeader {

    private const int MAX_IMPS = 5;

    private int _corruption;
    public Faction playerFaction { get; private set; }
    public Area playerArea { get; private set; }
    public int snatchCredits { get; private set; }
    public int mana { get; private set; }
    public int supplies { get; private set; }
    public int imps { get; private set; }

    private int _threatLevel;
    private int _redMagic;
    private int _blueMagic;
    private int _greenMagic;
    private int _lifestones;
    private float _currentLifestoneChance;
    private IPlayerPicker _currentlySelectedPlayerPicker;
    private IInteractable _currentTargetInteractable;
    private PlayerAbility _currentActiveAbility;
    private Character _markedCharacter;
    private BaseLandmark _demonicPortal;
    private List<CharacterAction> _actions;
    private List<Character> _snatchedCharacters;
    private List<Intel> _intels;
    private List<Item> _items;
    private List<PlayerAbility> _allAbilities;
    private List<Minion> _minions;


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
    public int lifestones {
        get { return _lifestones; }
    }
    public float currentLifestoneChance {
        get { return _currentLifestoneChance; }
    }
    public PlayerAbility currentActiveAbility {
        get { return _currentActiveAbility; }
    }
    public Character markedCharacter {
        get { return _markedCharacter; }
    }
    public BaseLandmark demonicPortal {
        get { return _demonicPortal; }
    }
    public IInteractable currentTargetInteractable {
        get { return _currentTargetInteractable; }
    }
    public List<CharacterAction> actions {
        get { return _actions; }
    }
    public List<Intel> intels {
        get { return _intels; }
    }
    public List<Item> items {
        get { return _items; }
    }
    public List<PlayerAbility> allAbilities {
        get { return _allAbilities; }
    }
    #endregion

    public Player() {
        _corruption = 0;
        playerArea = null;
        snatchCredits = 0;
        _snatchedCharacters = new List<ECS.Character>();
        _intels = new List<Intel>();
        _items = new List<Item>();
        SetRedMagic(50);
        SetBlueMagic(50);
        SetGreenMagic(50);
        SetThreatLevel(20);
        SetCurrentLifestoneChance(25f);
        ConstructAbilities();
        //Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_ADDED, OnTileAddedToPlayerArea);
        Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_REMOVED, OnTileRemovedFromPlayerArea);
        Messenger.AddListener<Character>(Signals.CHARACTER_RELEASED, OnCharacterReleased);
        Messenger.AddListener(Signals.HOUR_STARTED, EverydayAction);
        //ConstructPlayerActions();
    }

    private void EverydayAction() {
        DepleteThreatLevel();
    }

    #region Corruption
    public void AdjustCorruption(int adjustment) {
        _corruption += adjustment;
    }
    #endregion

    #region Area
    public void CreatePlayerArea(HexTile chosenCoreTile) {
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION);
        _demonicPortal = LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        Biomes.Instance.CorruptTileVisuals(chosenCoreTile);
        chosenCoreTile.SetCorruption(true);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        _demonicPortal.tileLocation.ScheduleCorruption();
        //OnTileAddedToPlayerArea(playerArea, chosenCoreTile);
    }
    public void CreatePlayerArea(BaseLandmark portal) {
        _demonicPortal = portal;
        Area playerArea = LandmarkManager.Instance.CreateNewArea(portal.tileLocation, AREA_TYPE.DEMONIC_INTRUSION);
        Biomes.Instance.CorruptTileVisuals(portal.tileLocation);
        portal.tileLocation.SetCorruption(true);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        _demonicPortal.tileLocation.ScheduleCorruption();
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
        _actions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.MOVE_TO));
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
    public void AdjustMana(int amount) {
        mana += amount;
        mana = Mathf.Max(mana, 0); //maybe 999?
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
    private void DepleteThreatLevel() {
        AdjustThreatLevel(-1);
    }
    #endregion

    #region Intel
    public void AddIntel(Intel intel) {
        if (!_intels.Contains(intel)) {
            _intels.Add(intel);
            Debug.Log("Added intel " + intel.ToString());
            Messenger.Broadcast(Signals.INTEL_ADDED, intel);
        }
    }
    public bool RemoveIntel(Intel intel) {
        return _intels.Remove(intel);
    }
    public void PickIntelToGiveToCharacter(Character character, ShareIntel shareIntelAbility) {
        //TODO
        _currentTargetInteractable = character;
        _currentActiveAbility = shareIntelAbility;
        PlayerUI.Instance.ShowPlayerPickerAndPopulate();
    }
    private void GiveIntelToCharacter() {
        Intel intel = _currentlySelectedPlayerPicker as Intel;
        ShareIntel shareIntel = _currentActiveAbility as ShareIntel;
        Character character = _currentTargetInteractable as Character;
        GiveIntelToCharacter(intel, character, shareIntel);
    }
    public void GiveIntelToCharacter(Intel intel, Character character, ShareIntel shareIntel = null) {
        if (character.intelReactions.ContainsKey(intel.id)) {
            character.OnIntelGiven(intel);
            if (shareIntel != null) {
                shareIntel.HasGivenIntel(character);
            }
        }
    }
    public List<Intel> GetIntelConcerning(Character character) {
        List<Intel> intel = new List<Intel>();
        for (int i = 0; i < _intels.Count; i++) {
            Intel currIntel = _intels[i];
            if (currIntel.description.Contains(character.name) || currIntel.name.Contains(character.name)) {
                intel.Add(currIntel);
            }
        }
        return intel;
    }
    public bool HasIntel(Intel intel) {
        return _intels.Contains(intel);
    }
    #endregion

    #region Items
    public void AddItem(Item item) {
        _items.Add(item);
    }
    public bool RemoveItem(Item item) {
        return _items.Remove(item);
    }
    public Item GetItem(string itemName) {
        for (int i = 0; i < _items.Count; i++) {
            if(_items[i].itemName == itemName) {
                return _items[i];
            }
        }
        return null;
    }
    public void PickItemToGiveToCharacter(Character character, GiveItem giveItemAbility) {
        //TODO
        _currentTargetInteractable = character;
        _currentActiveAbility = giveItemAbility;
        PlayerUI.Instance.ShowPlayerPickerAndPopulate();
    }
    private void GiveItemToCharacter() {
        Item item = _currentlySelectedPlayerPicker as Item;
        GiveItem giveItem = _currentActiveAbility as GiveItem;
        Character character = _currentTargetInteractable as Character;
        character.PickupItem(item);
        RemoveItem(item);
        giveItem.HasGivenItem(character);
    }
    public void PickItemToTakeFromLandmark(BaseLandmark landmark, TakeItem takeItem) {
        //TODO
        _currentTargetInteractable = landmark;
        _currentActiveAbility = takeItem;
        PlayerUI.Instance.ShowPlayerPickerAndPopulate();
    }
    private void TakeItemFromLandmark() {
        Item item = _currentlySelectedPlayerPicker as Item;
        TakeItem takeItem = _currentActiveAbility as TakeItem;
        BaseLandmark landmark = _currentTargetInteractable as BaseLandmark;
        AddItem(item);
        landmark.RemoveItemInLandmark(item);
        takeItem.HasTakenItem(landmark);
    }
    #endregion

    #region Lifestone
    public void DecreaseLifestoneChance() {
        if(_currentLifestoneChance > 2f) {
            float decreaseRate = 5f;
            if(_currentLifestoneChance <= 15f) {
                decreaseRate = 1f;
            }
            _currentLifestoneChance -= decreaseRate;
        }
    }
    public void SetCurrentLifestoneChance(float amount) {
        _currentLifestoneChance = amount;
    }
    public void SetLifestone(int amount) {
        _lifestones = amount;
    }
    public void AdjustLifestone(int amount) {
        _lifestones += amount;
        Debug.Log("Adjusted player lifestones by: " + amount + ". New total is " + _lifestones);
    }
    #endregion

    #region PlayerPicker
    public void SetCurrentlySelectedPlayerPicker(IPlayerPicker playerPicker) {
        _currentlySelectedPlayerPicker = playerPicker;
    }
    public void OnOkPlayerPicker() {
        if(_currentlySelectedPlayerPicker != null && _currentTargetInteractable != null && _currentActiveAbility != null) {
            if(_currentActiveAbility is GiveItem) {
                GiveItemToCharacter();
            }else if (_currentActiveAbility is ShareIntel) {
                GiveIntelToCharacter();
            } else if (_currentActiveAbility is TakeItem) {
                TakeItemFromLandmark();
            }
        }
    }
    public void OnHidePlayerPicker() {
        _currentlySelectedPlayerPicker = null;
        _currentTargetInteractable = null;
        _currentActiveAbility = null;
    }
    #endregion

    #region Abilities
    private void ConstructAbilities() {
        _allAbilities = new List<PlayerAbility>();
        Inspect inspect = new Inspect();
        RevealSecret revealSecret = new RevealSecret();
        Spook spook = new Spook();
        Assist assist = new Assist();
        GiveItem giveItem = new GiveItem();
        ShareIntel shareIntel = new ShareIntel();
        TakeItem takeItem = new TakeItem();
        MonsterAttack monsterAttack = new MonsterAttack();
        Mark mark = new Mark();
        Awaken awaken = new Awaken();
        AwakenDesire awakenDesire = new AwakenDesire();

        _allAbilities.Add(inspect);
        _allAbilities.Add(revealSecret);
        _allAbilities.Add(spook);
        _allAbilities.Add(assist);
        _allAbilities.Add(giveItem);
        _allAbilities.Add(shareIntel);
        _allAbilities.Add(takeItem);
        _allAbilities.Add(monsterAttack);
        _allAbilities.Add(mark);
        _allAbilities.Add(awaken);
        _allAbilities.Add(awakenDesire);

        PlayerAbilitiesUI.Instance.ConstructAbilityButtons(_allAbilities);
    }
    public PlayerAbility GetAbility(string abilityName) {
        for (int i = 0; i < _allAbilities.Count; i++) {
            PlayerAbility currAbility = _allAbilities[i];
            if (currAbility.name.Equals(abilityName)) {
                return currAbility;
            }
        }
        return null;
    }
    #endregion

    #region Character
    public void SetMarkedCharacter(Character character) {
        _markedCharacter = character;
    }
    #endregion

    #region Supplies
    public void AdjustSupplies(int amount) {
        supplies += amount;
        supplies = Mathf.Max(supplies, 0);
    }
    #endregion

    #region Minions
    public void CreateInitialMinions() {
        _minions = new List<Minion>();
        _minions.Add(new Minion(CharacterManager.Instance.CreateNewCharacter(CHARACTER_ROLE.CIVILIAN, "Farmer", RACE.HUMANS, GENDER.MALE, playerFaction, _demonicPortal), GetAbility("Inspect")));
        _minions.Add(new Minion(CharacterManager.Instance.CreateNewCharacter(CHARACTER_ROLE.CIVILIAN, "Farmer", RACE.HUMANS, GENDER.MALE, playerFaction, _demonicPortal), GetAbility("Spook")));
        _minions.Add(new Minion(CharacterManager.Instance.CreateNewCharacter(CHARACTER_ROLE.CIVILIAN, "Farmer", RACE.HUMANS, GENDER.MALE, playerFaction, _demonicPortal), GetAbility("Mark")));
        _minions.Add(new Minion(CharacterManager.Instance.CreateNewCharacter(CHARACTER_ROLE.CIVILIAN, "Farmer", RACE.HUMANS, GENDER.MALE, playerFaction, _demonicPortal), GetAbility("Awaken")));
        _minions.Add(new Minion(CharacterManager.Instance.CreateNewCharacter(CHARACTER_ROLE.CIVILIAN, "Farmer", RACE.HUMANS, GENDER.MALE, playerFaction, _demonicPortal), GetAbility("Awaken Desire")));

        for (int i = 0; i < PlayerUI.Instance.minionItems.Length; i++) {
            MinionItem minionItem = PlayerUI.Instance.minionItems[i];
            if(i < _minions.Count) {
                minionItem.SetMinion(_minions[i]);
            } else {
                minionItem.SetMinion(null);
            }
        }
        PlayerUI.Instance.minionsScrollRect.verticalNormalizedPosition = 1f;
    }
    public void SortByLevel() {
        _minions = _minions.OrderBy(x => x.lvl).ToList();
        for (int i = 0; i < PlayerUI.Instance.minionItems.Length; i++) {
            MinionItem minionItem = PlayerUI.Instance.minionItems[i];
            if (i < _minions.Count) {
                minionItem.SetMinion(_minions[i]);
            } else {
                minionItem.SetMinion(null);
            }
        }
    }
    public void SortByType() {
        _minions = _minions.OrderBy(x => x.type.ToString()).ToList();
        for (int i = 0; i < PlayerUI.Instance.minionItems.Length; i++) {
            MinionItem minionItem = PlayerUI.Instance.minionItems[i];
            if (i < _minions.Count) {
                minionItem.SetMinion(_minions[i]);
            } else {
                minionItem.SetMinion(null);
            }
        }
    }
    public List<Party> GetMinionsAndArmies() {
        List<Party> armies = new List<Party>();
        for (int i = 0; i < _minions.Count; i++) {
            Minion currMinion = _minions[i];
            if (!armies.Contains(currMinion.icharacter.currentParty)) {
                armies.Add(currMinion.icharacter.currentParty);
            }
        }
        return armies;
    }
    #endregion

    #region Imps
    public void AdjustImps(int amount) {
        imps += amount;
        imps = Mathf.Clamp(imps, 0, MAX_IMPS);
    }
    #endregion
}
