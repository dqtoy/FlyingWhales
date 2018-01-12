using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : QuestCreator {
    public string _name;
    public GENDER _gender;
    public List<Trait> _traits;
    public RACE _race;
    public CharacterRole _role;
    public CHARACTER_CLASS _characterClass;
    public Faction _faction;
    public Party _party;
    public HexTile currLocation;
    public Quest currentQuest;
    public CharacterAvatar _avatar;

    public int hp;
    public int maxHP;

    public Character(RACE race) {
        _gender = Utilities.GetRandomGender();
        _race = race;
        _name = RandomNameGenerator.Instance.GenerateRandomName(_race, _gender);
        maxHP = 100;
        hp = 100;
    }

    #region Roles
    public void AssignRole(CHARACTER_ROLE role) {
        switch (role) {
            case CHARACTER_ROLE.CHIEFTAIN:
                _role = new Chieftain();
                break;
            case CHARACTER_ROLE.VILLAGE_HEAD:
                _role = new VillageHead();
                break;
            case CHARACTER_ROLE.WARLORD:
                _role = new Warlord();
                break;
            case CHARACTER_ROLE.HERO:
                _role = new Hero();
                break;
            case CHARACTER_ROLE.TRADER:
                _role = new Trader();
                break;
            case CHARACTER_ROLE.ADVENTURER:
                _role = new Adventurer();
                break;
            case CHARACTER_ROLE.COLONIST:
                _role = new Colonist();
                break;
            case CHARACTER_ROLE.SPY:
                _role = new Spy();
                break;
            case CHARACTER_ROLE.MEDIATOR:
                _role = new Mediator();
                break;
            case CHARACTER_ROLE.NECROMANCER:
                _role = new Necromancer();
                break;
            case CHARACTER_ROLE.DRAGON_TAMER:
                _role = new DragonTamer();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Character Class
    public void AssignClass(CHARACTER_CLASS charClass) {
        _characterClass = charClass;
    }
    #endregion

    #region Traits
    public bool HasTrait(TRAIT trait) {
        for (int i = 0; i < _traits.Count; i++) {
            if(_traits[i].trait == trait) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Faction
    public void SetFaction(Faction faction) {
        _faction = faction;
    }
    #endregion

    #region Party
    public void SetParty(Party party) {
        _party = party;
    }
    #endregion

    #region Location
    public void SetLocation(HexTile location) {
        currLocation = location;
    }
    #endregion

    #region Quests
    private void DetermineAction() {
        WeightedDictionary<QUEST_TYPE> actionWeights = GetActionWeights();
        if (actionWeights.GetTotalOfWeights() > 0) {
            QUEST_TYPE chosenAction = actionWeights.PickRandomElementGivenWeights();
            switch (chosenAction) {
                case QUEST_TYPE.EXPLORE_REGION:
                    break;
                case QUEST_TYPE.OCCUPY_LANDMARK:
                    break;
                case QUEST_TYPE.INVESTIGATE_LANDMARK:
                    break;
                case QUEST_TYPE.OBTAIN_RESOURCE:
                    break;
                case QUEST_TYPE.EXPAND:
                    break;
                case QUEST_TYPE.REST:
                    StartRestQuest();
                    break;
                case QUEST_TYPE.GO_HOME:
                    break;
                case QUEST_TYPE.DO_NOTHING:
                    break;
                default:
                    break;
            }
        }
    }

    private WeightedDictionary<QUEST_TYPE> GetActionWeights() {
        WeightedDictionary<QUEST_TYPE> actionWeights = new WeightedDictionary<QUEST_TYPE>();
        for (int i = 0; i < _faction.internalQuestManager.activeQuests.Count; i++) {
            Quest currQuest = _faction.internalQuestManager.activeQuests[i];
            if (currQuest.CanAcceptQuest(this)) {
                actionWeights.AddElement(currQuest.questType, GetWeightForQuestType(currQuest.questType));
            }
        }
        actionWeights.AddElement(QUEST_TYPE.REST, GetWeightForQuestType(QUEST_TYPE.REST));
        actionWeights.AddElement(QUEST_TYPE.GO_HOME, GetWeightForQuestType(QUEST_TYPE.GO_HOME));
        actionWeights.AddElement(QUEST_TYPE.DO_NOTHING, GetWeightForQuestType(QUEST_TYPE.DO_NOTHING));
        return actionWeights;
    }
    private int GetWeightForQuestType(QUEST_TYPE questType) {
        int weight = 0;
        switch (questType) {
            case QUEST_TYPE.EXPLORE_REGION:
                weight += GetExploreRegionWeight();
                break;
            case QUEST_TYPE.OCCUPY_LANDMARK:
                break;
            case QUEST_TYPE.INVESTIGATE_LANDMARK:
                break;
            case QUEST_TYPE.OBTAIN_RESOURCE:
                break;
            case QUEST_TYPE.EXPAND:
                break;
            case QUEST_TYPE.REST:
                weight += GetRestWeight();
                break;
            case QUEST_TYPE.GO_HOME:
                weight += GetGoHomeWeight();
                break;
            case QUEST_TYPE.DO_NOTHING:
                weight += GetDoNothingWeight();
                break;
            default:
                break;
        }
        return weight;
    }
    private int GetExploreRegionWeight() {
        int weight = 0;
        switch (_role.roleType) {
            case CHARACTER_ROLE.CHIEFTAIN:
                weight += 100;
                break;
            default:
                break;
        }
        return weight;
    }
    private int GetRestWeight() {
        if(hp < maxHP) {
            int percentMissing = hp / maxHP;
            return 5 * percentMissing;
        }
        return 0;
    }
    private int GetGoHomeWeight() {
        //0 if already at Home Settlement or no path to it
        if (currLocation.isHabitable && currLocation.isOccupied && currLocation.landmarkOnTile.owner == this._faction) {
            return 0;
        }
        if(PathGenerator.Instance.GetPath(currLocation, _faction.settlements[0].location, PATHFINDING_MODE.USE_ROADS) == null) {
            return 0;
        }
        return 5; //5 if not
    }
    private int GetDoNothingWeight() {
        return 10;
    }
    private void StartRestQuest() {
        Rest restQuest = new Rest(this, 0, 1);
        restQuest.StartQuestLine();
    }
    #endregion

    #region HP
    int regenAmount;
    public void StartRegeneration(int amount) {
        regenAmount = amount;
        Messenger.AddListener("OnDayEnd", RegenerateHealth);
    }
    public void StopRegeneration() {
        regenAmount = 0;
        Messenger.RemoveListener("OnDayEnd", RegenerateHealth);
    }
    public void AdjustHP(int amount) {
        hp += amount;
        hp = Mathf.Clamp(hp, 0, maxHP);
    }
    public void RegenerateHealth() {
        AdjustHP(regenAmount);
    }
    #endregion

    #region Avatar
    public void SetAvatar(CharacterAvatar avatar) {
        _avatar = avatar;
    }
    public void DestroyAvatar() {
        if(_avatar != null) {
            _avatar.DestroyObject();
        }
    }
    #endregion
}
