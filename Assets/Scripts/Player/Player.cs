using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;
using System.Linq;

public class Player : ILeader {

    private const int MAX_IMPS = 5;

    public Faction playerFaction { get; private set; }
    public Area playerArea { get; private set; }
    public int snatchCredits { get; private set; }
    public int maxImps { get; private set; }

    private int _lifestones;
    private int _maxMinions;
    private float _currentLifestoneChance;
    private IPlayerPicker _currentlySelectedPlayerPicker;
    private IInteractable _currentTargetInteractable;
    //private PlayerAbility _currentActiveAbility;
    private Character _markedCharacter;
    private BaseLandmark _demonicPortal;
    private List<CharacterAction> _actions;
    private List<Character> _snatchedCharacters;
    private List<Intel> _intels;
    private List<Item> _items;
    //private List<PlayerAbility> _allAbilities;
    private List<Minion> _minions;
    private Dictionary<CURRENCY, int> _currencies;

    public List<ICharacter> otherCharacters;

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
    public int lifestones {
        get { return _lifestones; }
    }
    public int maxMinions {
        get { return _maxMinions; }
    }
    public float currentLifestoneChance {
        get { return _currentLifestoneChance; }
    }
    public bool areMinionsMaxed {
        get { return _minions.Count >= maxMinions; }
    }
    public RACE race {
        get { return RACE.HUMANS; }
    }
    //public PlayerAbility currentActiveAbility {
    //    get { return _currentActiveAbility; }
    //}
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
    //public List<PlayerAbility> allAbilities {
    //    get { return _allAbilities; }
    //}
    public Dictionary<CURRENCY, int> currencies {
        get { return _currencies; }
    }
    public List<Minion> minions {
        get { return _minions; }
    }
    #endregion

    public Player() {
        playerArea = null;
        snatchCredits = 0;
        _snatchedCharacters = new List<ECS.Character>();
        _intels = new List<Intel>();
        _items = new List<Item>();
        otherCharacters = new List<ICharacter>();
        //_maxMinions = PlayerUI.Instance.minionItems.Count;
        maxImps = 5;
        SetCurrentLifestoneChance(25f);
        //ConstructAbilities();
        ConstructCurrencies();
        //Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_ADDED, OnTileAddedToPlayerArea);
        Messenger.AddListener<Area, HexTile>(Signals.AREA_TILE_REMOVED, OnTileRemovedFromPlayerArea);
        Messenger.AddListener<Character>(Signals.CHARACTER_RELEASED, OnCharacterReleased);
        Messenger.AddListener(Signals.HOUR_STARTED, EverydayAction);
        //ConstructPlayerActions();
        AddWinListener();
    }

    private void EverydayAction() {
        //DepleteThreatLevel();
    }

    #region Area
    public void CreatePlayerArea(HexTile chosenCoreTile) {
        chosenCoreTile.SetCorruption(true);
        Area playerArea = LandmarkManager.Instance.CreateNewArea(chosenCoreTile, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.LoadAdditionalData();
        _demonicPortal = LandmarkManager.Instance.CreateNewLandmarkOnTile(chosenCoreTile, LANDMARK_TYPE.DEMONIC_PORTAL);
        Biomes.Instance.CorruptTileVisuals(chosenCoreTile);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        _demonicPortal.tileLocation.ScheduleCorruption();
        //OnTileAddedToPlayerArea(playerArea, chosenCoreTile);
    }
    public void CreatePlayerArea(BaseLandmark portal) {
        _demonicPortal = portal;
        Area playerArea = LandmarkManager.Instance.CreateNewArea(portal.tileLocation, AREA_TYPE.DEMONIC_INTRUSION);
        playerArea.LoadAdditionalData();
        Biomes.Instance.CorruptTileVisuals(portal.tileLocation);
        portal.tileLocation.SetCorruption(true);
        SetPlayerArea(playerArea);
        //ActivateMagicTransferToPlayer();
        _demonicPortal.tileLocation.ScheduleCorruption();
    }
    public void LoadPlayerArea(Area area) {
        _demonicPortal = area.coreTile.landmarkOnTile;
        Biomes.Instance.CorruptTileVisuals(_demonicPortal.tileLocation);
        _demonicPortal.tileLocation.SetCorruption(true);
        SetPlayerArea(area);
        _demonicPortal.tileLocation.ScheduleCorruption();

    }
    private void SetPlayerArea(Area area) {
        playerArea = area;
    }
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
                //character.OnThisCharacterSnatched();
                //ReleaseCharacterQuest rcq = new ReleaseCharacterQuest(character); //create quest
                //QuestManager.Instance.CreateQuest(QUEST_TYPE.RELEASE_CHARACTER, character);
                //Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Successfully snatched " + character.name, MESSAGE_BOX_MODE.MESSAGE_ONLY, true);
                //Debug.Log("Snatched " + character.name);
                //Messenger.Broadcast(Signals.CHARACTER_SNATCHED, character);
            }
        } else {
            //Messenger.Broadcast(Signals.SHOW_POPUP_MESSAGE, "Failed to snatch " + character.name, MESSAGE_BOX_MODE.MESSAGE_ONLY, true);
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

    //#region Actions
    //private void ConstructPlayerActions() {
    //    _actions = new List<CharacterAction>();
    //    _actions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.MOVE_TO));
    //    for (int i = 0; i < _actions.Count; i++) {
    //        _actions[i].Initialize();
    //        GameObject go = GameObject.Instantiate(UIManager.Instance.playerActionsUI.playerActionsBtnPrefab, UIManager.Instance.playerActionsUI.playerActionsContentTransform);
    //        go.GetComponent<PlayerActionBtn>().SetAction(_actions[i]);
    //    }
    //}
    //#endregion

    #region Intel
    public void AddIntel(Intel intel) {
        if (!_intels.Contains(intel)) {
            _intels.Add(intel);
            intel.SetObtainedState(true);
            Debug.Log("Added intel " + intel.ToString());
            Messenger.Broadcast(Signals.INTEL_ADDED, intel);
            if (intel is CharacterIntel) {
                Messenger.Broadcast(Signals.CHARACTER_INTEL_ADDED, intel as CharacterIntel);
            }
        }
    }
    public bool RemoveIntel(Intel intel) {
        if (_intels.Remove(intel)) {
            intel.SetObtainedState(false);
            Debug.Log("Removed intel " + intel.ToString());
            return true;
        }
        return false;
    }
    //public void PickIntelToGiveToCharacter(Character character, ShareIntel shareIntelAbility) {
    //    //TODO
    //    _currentTargetInteractable = character;
    //    _currentActiveAbility = shareIntelAbility;
    //    PlayerUI.Instance.ShowPlayerPickerAndPopulate();
    //}
    //private void GiveIntelToCharacter() {
    //    Intel intel = _currentlySelectedPlayerPicker as Intel;
    //    ShareIntel shareIntel = _currentActiveAbility as ShareIntel;
    //    Character character = _currentTargetInteractable as Character;
    //    GiveIntelToCharacter(intel, character, shareIntel);
    //}
    //public void GiveIntelToCharacter(Intel intel, Character character, ShareIntel shareIntel = null) {
    //    if (character.intelReactions.ContainsKey(intel.id)) {
    //        character.OnIntelGiven(intel);
    //        if (shareIntel != null) {
    //            shareIntel.HasGivenIntel(character);
    //        }
    //    }
    //}
    //public List<Intel> GetIntelConcerning(Character character) {
    //    List<Intel> intel = new List<Intel>();
    //    for (int i = 0; i < _intels.Count; i++) {
    //        Intel currIntel = _intels[i];
            //if (currIntel.description.Contains(character.name) || currIntel.name.Contains(character.name)) {
            //    intel.Add(currIntel);
            //}
    //    }
    //    return intel;
    //}
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
    //public void PickItemToGiveToCharacter(Character character, GiveItem giveItemAbility) {
    //    //TODO
    //    _currentTargetInteractable = character;
    //    _currentActiveAbility = giveItemAbility;
    //    PlayerUI.Instance.ShowPlayerPickerAndPopulate();
    //}
    //private void GiveItemToCharacter() {
    //    Item item = _currentlySelectedPlayerPicker as Item;
    //    GiveItem giveItem = _currentActiveAbility as GiveItem;
    //    Character character = _currentTargetInteractable as Character;
    //    character.PickupItem(item);
    //    RemoveItem(item);
    //    giveItem.HasGivenItem(character);
    //}
    //public void PickItemToTakeFromLandmark(BaseLandmark landmark, TakeItem takeItem) {
    //    //TODO
    //    _currentTargetInteractable = landmark;
    //    _currentActiveAbility = takeItem;
    //    PlayerUI.Instance.ShowPlayerPickerAndPopulate();
    //}
    //private void TakeItemFromLandmark() {
    //    Item item = _currentlySelectedPlayerPicker as Item;
    //    TakeItem takeItem = _currentActiveAbility as TakeItem;
    //    BaseLandmark landmark = _currentTargetInteractable as BaseLandmark;
    //    AddItem(item);
    //    landmark.RemoveItemInLandmark(item);
    //    takeItem.HasTakenItem(landmark);
    //}
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

    //#region PlayerPicker
    //public void SetCurrentlySelectedPlayerPicker(IPlayerPicker playerPicker) {
    //    _currentlySelectedPlayerPicker = playerPicker;
    //}
    //public void OnOkPlayerPicker() {
    //    if(_currentlySelectedPlayerPicker != null && _currentTargetInteractable != null && _currentActiveAbility != null) {
    //        if(_currentActiveAbility is GiveItem) {
    //            GiveItemToCharacter();
    //        }else if (_currentActiveAbility is ShareIntel) {
    //            //GiveIntelToCharacter();
    //        } else if (_currentActiveAbility is TakeItem) {
    //            TakeItemFromLandmark();
    //        }
    //    }
    //}
    //public void OnHidePlayerPicker() {
    //    _currentlySelectedPlayerPicker = null;
    //    _currentTargetInteractable = null;
    //    _currentActiveAbility = null;
    //}
    //#endregion

    //#region Abilities
    //private void ConstructAbilities() {
    //    _allAbilities = new List<PlayerAbility>();
    //    Inspect inspect = new Inspect();
    //    RevealSecret revealSecret = new RevealSecret();
    //    Spook spook = new Spook();
    //    Assist assist = new Assist();
    //    GiveItem giveItem = new GiveItem();
    //    ShareIntel shareIntel = new ShareIntel();
    //    TakeItem takeItem = new TakeItem();
    //    MonsterAttack monsterAttack = new MonsterAttack();
    //    Mark mark = new Mark();
    //    Awaken awaken = new Awaken();
    //    AwakenDesire awakenDesire = new AwakenDesire();

    //    _allAbilities.Add(inspect);
    //    _allAbilities.Add(revealSecret);
    //    _allAbilities.Add(spook);
    //    _allAbilities.Add(assist);
    //    _allAbilities.Add(giveItem);
    //    _allAbilities.Add(shareIntel);
    //    _allAbilities.Add(takeItem);
    //    _allAbilities.Add(monsterAttack);
    //    _allAbilities.Add(mark);
    //    _allAbilities.Add(awaken);
    //    _allAbilities.Add(awakenDesire);

    //    PlayerAbilitiesUI.Instance.ConstructAbilityButtons(_allAbilities);
    //}
    //public PlayerAbility GetAbility(string abilityName) {
    //    for (int i = 0; i < _allAbilities.Count; i++) {
    //        PlayerAbility currAbility = _allAbilities[i];
    //        if (currAbility.name.Equals(abilityName)) {
    //            return currAbility;
    //        }
    //    }
    //    return null;
    //}
    //#endregion

    #region Character
    public void SetMarkedCharacter(Character character) {
        _markedCharacter = character;
    }
    #endregion

    #region Minions
    public void CreateInitialMinions() {
        PlayerUI.Instance.ResetAllMinionItems();
        _minions = new List<Minion>();
        AddMinion(CreateNewMinion("Farmer", RACE.HUMANS, false));
        AddMinion(CreateNewMinion("Farmer", RACE.HUMANS, false));
        AddMinion(CreateNewMinion("Farmer", RACE.HUMANS, false));
        AddMinion(CreateNewMinion("Farmer", RACE.HUMANS, false));
        AddMinion(CreateNewMinion("Farmer", RACE.HUMANS, false));

        //UpdateMinions();
        PlayerUI.Instance.minionsScrollRect.verticalNormalizedPosition = 1f;
        PlayerUI.Instance.OnStartMinionUI();
    }
    public Minion CreateNewMinion(ICharacter character) {
        return new Minion(character);
    }
    public Minion CreateNewMinion(string className, RACE race, DEMON_TYPE demonType, bool isArmy) {
        Minion minion = null;
        if (isArmy) {
            minion = new Minion(CharacterManager.Instance.CreateCharacterArmyUnit(className, race, playerFaction, _demonicPortal), demonType);
        } else {
            minion = new Minion(CharacterManager.Instance.CreateNewCharacter(className, race, GENDER.MALE, playerFaction, _demonicPortal, false), demonType);
        }
        return minion;
    }
    public Minion CreateNewMinion(string className, RACE race, bool isArmy) {
        Minion minion = null;
        if (isArmy) {
            minion = new Minion(CharacterManager.Instance.CreateCharacterArmyUnit(className, race, playerFaction, _demonicPortal));
        } else {
            minion = new Minion(CharacterManager.Instance.CreateNewCharacter(className, race, GENDER.MALE, playerFaction, _demonicPortal, false));
        }
        return minion;
    }
    public void UpdateMinions() {
        for (int i = 0; i < _minions.Count; i++) {
            RearrangeMinionItem(_minions[i].minionItem, i);
        }
    }
    private void RearrangeMinionItem(PlayerCharacterItem minionItem, int index) {
        if (minionItem.transform.GetSiblingIndex() != index) {
            Vector3 to = PlayerUI.Instance.minionsContentTransform.GetChild(index).transform.localPosition;
            Vector3 from = minionItem.transform.localPosition;
            minionItem.supposedIndex = index;
            minionItem.tweenPos.from = from;
            minionItem.tweenPos.to = to;
            minionItem.tweenPos.ResetToBeginning();
            minionItem.tweenPos.PlayForward();
        }
    }
    public void SortByLevel() {
        _minions = _minions.OrderBy(x => x.lvl).ToList();
        //for (int i = 0; i < PlayerUI.Instance.minionItems.Length; i++) {
        //    MinionItem minionItem = PlayerUI.Instance.minionItems[i];
        //    if (i < _minions.Count) {
        //        minionItem.SetMinion(_minions[i]);
        //    } else {
        //        minionItem.SetMinion(null);
        //    }
        //}
        UpdateMinions();
    }
    public void SortByType() {
        _minions = _minions.OrderBy(x => x.type.ToString()).ToList();
        //for (int i = 0; i < PlayerUI.Instance.minionItems.Length; i++) {
        //    MinionItem minionItem = PlayerUI.Instance.minionItems[i];
        //    if (i < _minions.Count) {
        //        minionItem.SetMinion(_minions[i]);
        //    } else {
        //        minionItem.SetMinion(null);
        //    }
        //}
        UpdateMinions();
    }
    public void SortByDefault() {
        _minions = _minions.OrderBy(x => x.indexDefaultSort.ToString()).ToList();
        UpdateMinions();
    }
    public void AddMinion(Minion minion) {
        if(_minions.Count < _maxMinions) {
            minion.SetIndexDefaultSort(_minions.Count);
            //MinionItem minionItem = PlayerUI.Instance.minionItems[_minions.Count];
            PlayerCharacterItem item = PlayerUI.Instance.GetUnoccupiedCharacterItem();
            item.SetCharacter(minion.icharacter);

            if (PlayerUI.Instance.minionSortType == MINIONS_SORT_TYPE.LEVEL) {
                for (int i = 0; i < _minions.Count; i++) {
                    if(minion.lvl <= _minions[i].lvl) {
                        _minions.Insert(i, minion);
                        item.transform.SetSiblingIndex(i);
                        break;
                    }
                }
            }else if (PlayerUI.Instance.minionSortType == MINIONS_SORT_TYPE.TYPE) {
                string strMinionType = minion.type.ToString();
                for (int i = 0; i < _minions.Count; i++) {
                    int compareResult = string.Compare(strMinionType, _minions[i].type.ToString());
                    if (compareResult == -1 || compareResult == 0) {
                        _minions.Insert(i, minion);
                        item.transform.SetSiblingIndex(i);
                        break;
                    }
                }
            } else {
                _minions.Add(minion);
            }

        }
    }
    public void RemoveMinion(Minion minion) {
        if(_minions.Remove(minion)){
            PlayerUI.Instance.RemoveCharacterItem(minion.minionItem);
            //if(minion.currentlyExploringArea != null) {
            //    minion.currentlyExploringArea.landmarkInvestigation.CancelInvestigation("explore");
            //}
            //if (minion.currentlyAttackingArea != null) {
            //    minion.currentlyAttackingArea.landmarkInvestigation.CancelInvestigation("attack");
            //}
            //minion.minionItem.SetMinion(null);
        }
    }
    public void AdjustMaxMinions(int adjustment) {
        _maxMinions += adjustment;
        _maxMinions = Mathf.Max(0, _maxMinions);
        PlayerUI.Instance.OnMaxMinionsChanged();
    }
    public void SetMaxMinions(int value) {
        _maxMinions = value;
        _maxMinions = Mathf.Max(0, _maxMinions);
        PlayerUI.Instance.OnMaxMinionsChanged();
    }
    #endregion

    #region Currencies
    private void ConstructCurrencies() {
        _currencies = new Dictionary<CURRENCY, int>();
        _currencies.Add(CURRENCY.IMP, 0);
        _currencies.Add(CURRENCY.MANA, 0);
        _currencies.Add(CURRENCY.SUPPLY, 0);
        AdjustCurrency(CURRENCY.IMP, maxImps);
        AdjustCurrency(CURRENCY.SUPPLY, 5000);
    }
    public void AdjustCurrency(CURRENCY currency, int amount) {
        _currencies[currency] += amount;
        if(currency == CURRENCY.IMP) {
            _currencies[currency] = Mathf.Clamp(_currencies[currency], 0, maxImps);
        }else if (currency == CURRENCY.SUPPLY) {
            _currencies[currency] = Mathf.Max(_currencies[currency], 0);
        } else if (currency == CURRENCY.MANA) {
            _currencies[currency] = Mathf.Max(_currencies[currency], 0); //maybe 999?
        }
        Messenger.Broadcast(Signals.UPDATED_CURRENCIES);
    }
    public void SetMaxImps(int imps) {
        maxImps = imps;
        _currencies[CURRENCY.IMP] = Mathf.Clamp(_currencies[CURRENCY.IMP], 0, maxImps);
    }
    public void AdjustMaxImps(int adjustment) {
        maxImps += adjustment;
        AdjustCurrency(CURRENCY.IMP, adjustment);
    }
    #endregion

    #region Rewards
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.SUPPLY:
                AdjustCurrency(CURRENCY.SUPPLY, reward.amount);
                break;
            case REWARD.MANA:
                AdjustCurrency(CURRENCY.MANA, reward.amount);
                break;
            //case REWARD.EXP:
            //    state.assignedMinion.AdjustExp(reward.amount);
            //    break;
            default:
                break;
        }
    }
    #endregion

    #region Other Characters/Units
    public void AddNewCharacter(ICharacter character) {
        if (!otherCharacters.Contains(character)) {
            otherCharacters.Add(character);
            character.OnAddedToPlayer();
            PlayerCharacterItem item = PlayerUI.Instance.GetUnoccupiedCharacterItem();
            item.SetCharacter(character);
        }
    }
    public void RemoveCharacter(ICharacter character) {
        if (otherCharacters.Remove(character)) {
            PlayerUI.Instance.RemoveCharacterItem(character.playerCharacterItem);
        }
    }
    #endregion

    #region Win/Lose Conditions
    private void AddWinListener() {
        Messenger.AddListener<Faction>(Signals.FACTION_LEADER_DIED, OnFactionLeaderDied);
    }
    private void OnFactionLeaderDied(Faction faction) {
        Faction fyn = FactionManager.Instance.GetFactionBasedOnName("Fyn");
        Faction orelia = FactionManager.Instance.GetFactionBasedOnName("Orelia");
        if (fyn.isDestroyed && orelia.isDestroyed) {
            Debug.LogError("Fyn and Orelia factions are destroyed! Player won!");
        }
    }
    public void OnPlayerLandmarkRuined(BaseLandmark landmark) {
        switch (landmark.specificLandmarkType) {
            case LANDMARK_TYPE.DWELLINGS:
                //add 2 minion slots
                AdjustMaxMinions(-2);
                break;
            case LANDMARK_TYPE.IMP_KENNEL:
                //adds 1 Imp capacity
                AdjustMaxImps(-1);
                break;
            case LANDMARK_TYPE.DEMONIC_PORTAL:
                //player loses if the Portal is destroyed
                throw new System.Exception("Demonic Portal Was Destroyed! Game Over!");
            case LANDMARK_TYPE.RAMPART:
                //remove bonus 25% HP to all Defenders
                for (int i = 0; i < playerArea.landmarks.Count; i++) {
                    BaseLandmark currLandmark = playerArea.landmarks[i];
                    currLandmark.RemoveDefenderBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                    //if (currLandmark.defenders != null) {
                    //    currLandmark.defenders.RemoveBuff(new Buff() { buffedStat = STAT.HP, percentage = 0.25f });
                    //}
                }
                break;
            default:
                break;
        }
    }
    #endregion
}
