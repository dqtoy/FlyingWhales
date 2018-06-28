using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Monster : ICharacter {
    //Serialized fields
    private string _name;
    private MONSTER_TYPE _type;
    private MONSTER_CATEGORY _category;
    private int _level;
    private int _experienceDrop;
    private int _maxHP;
    private int _maxSP;
    private int _attackPower;
    private int _speed;
    private int _pDef;
    private int _mDef;
    private float _dodgeChance;
    private float _hitChance;
    private float _critChance;
    private List<Skill> _skills;
    private Dictionary<ELEMENT, float> _elementalWeaknesses;
    private Dictionary<ELEMENT, float> _elementalResistances;
    //To add item drops and their chances

    private string _characterColorCode;
    private int _id;
    private int _currentHP;
    private int _currentSP;
    private int _actRate;
    private int _currentRow;
    private int _numOfAttackers;
    private bool _isDead;
    private Color _characterColor;
    private Combat _currentCombat;
    private CharacterBattleOnlyTracker _battleOnlyTracker;
    private MonsterObj _monsterObj;
    private Faction _attackedByFaction;
    private BaseLandmark _homeLandmark;
    private StructureObj _homeStructure;
    private RaceSetting _raceSetting;
    private Region _currentRegion;
    private SIDES _currentSide;
    private List<BodyPart> _bodyParts;
    private CharacterIcon _icon;
    private ILocation _specificLocation;
    private PortraitSettings _portraitSettings;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_monster" + '"' + ">" + this._name + "</link>"; }
    }
    public string coloredUrlName {
        get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + "]" + "<color=" + this._characterColorCode + ">" + this._name + "</link>"; }
    }
    public int id {
        get { return _id; }
    }
    public int attackPower {
        get { return _attackPower; }
    }
    public int actRate {
        get { return _actRate; }
        set { _actRate = value; }
    }
    public int strength {
        get { return 0; }
    }
    public int intelligence {
        get { return 0; }
    }
    public int vitality {
        get { return 0; }
    }
    public int agility {
        get { return _speed; }
    }
    public int baseAgility {
        get { return _speed; }
    }
    public int level {
        get { return _level; }
    }
    public int currentHP {
        get { return _currentHP; }
    }
    public int maxHP {
        get { return _maxHP; }
    }
    public int currentRow {
        get { return _currentRow; }
    }
    public int currentSP {
        get { return _currentSP; }
    }
    public int numOfAttackers {
        get { return _numOfAttackers; }
        set { _numOfAttackers = value; }
    }
    public float critChance {
        get { return _critChance; }
    }
    public float critDamage {
        get { return 0f; }
    }
    public float computedPower {
        get { return GetAttackPower() + GetDefensePower(); }
    }
    public bool isDead {
        get { return _isDead; }
    }
    public GENDER gender {
        get { return GENDER.MALE; }
    }
    public SIDES currentSide {
        get { return _currentSide; }
    }
    public Combat currentCombat {
        get { return _currentCombat; }
        set { _currentCombat = value; }
    }
    public CharacterBattleOnlyTracker battleOnlyTracker {
        get { return _battleOnlyTracker; }
    }
    public Faction faction {
        get { return null; }
    }
    public Faction attackedByFaction {
        get { return _attackedByFaction; }
        set { _attackedByFaction = value; }
    }
    public BaseLandmark homeLandmark {
        get { return _homeLandmark; }
    }
    public StructureObj homeStructure {
        get { return _homeStructure; }
    }
    public Region currentRegion {
        get { return _currentRegion; }
    }
    public List<Skill> skills {
        get { return _skills; }
    }
    public List<BodyPart> bodyParts {
        get { return _bodyParts; }
    }
    public Dictionary<ELEMENT, float> elementalWeaknesses {
        get { return _elementalWeaknesses; }
    }
    public Dictionary<ELEMENT, float> elementalResistances {
        get { return _elementalResistances; }
    }
    public ICharacterObject icharacterObject {
        get { return _monsterObj; }
    }
    public MonsterObj monsterObj {
        get { return _monsterObj; }
    }
    public ILocation specificLocation {
        get { return GetSpecificLocation(); }
    }
    public CharacterIcon icon {
        get { return _icon; }
    }
    public PortraitSettings portraitSettings {
        get { return _portraitSettings; }
    }
    #endregion

    public Monster CreateNewCopy() {
        Monster newMonster = new Monster();
        newMonster._name = this._name;
        newMonster._type = this._type;
        newMonster._category = this._category;
        newMonster._level = this._level;
        newMonster._experienceDrop = this._experienceDrop;
        newMonster._maxHP = this._maxHP;
        newMonster._maxSP = this._maxSP;
        newMonster._attackPower = this._attackPower;
        newMonster._speed = this._speed;
        newMonster._pDef = this._pDef;
        newMonster._mDef = this._mDef;
        newMonster._dodgeChance = this._dodgeChance;
        newMonster._hitChance = this._hitChance;
        newMonster._critChance = this._critChance;
        newMonster._portraitSettings = CharacterManager.Instance.GenerateRandomPortrait();
//#if !WORLD_CREATION_TOOL
//        newMonster._monsterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.MONSTER, "MonsterObject") as MonsterObj;
//        newMonster._monsterObj.SetMonster(newMonster);
//#endif
        newMonster._skills = new List<Skill>();
        for (int i = 0; i < this._skills.Count; i++) {
            newMonster._skills.Add(_skills[i].CreateNewCopy());
        }
        newMonster._elementalWeaknesses = new Dictionary<ELEMENT, float>(this._elementalWeaknesses);
        newMonster._elementalResistances = new Dictionary<ELEMENT, float>(this._elementalResistances);

        return newMonster;
    }

    public void SetData(MonsterComponent monsterComponent) {
        this._name = monsterComponent.monsterName;
        this._type = monsterComponent.type;
        this._category = monsterComponent.category;
        this._experienceDrop = monsterComponent.experienceDrop;
        this._level = monsterComponent.level;
        this._maxHP = monsterComponent.maxHP;
        this._maxSP = monsterComponent.maxSP;
        this._attackPower = monsterComponent.attackPower;
        this._speed = monsterComponent.speed;
        this._pDef = monsterComponent.pDef;
        this._mDef = monsterComponent.mDef;
        this._dodgeChance = monsterComponent.dodgeChance;
        this._hitChance = monsterComponent.hitChance;
        this._critChance = monsterComponent.critChance;

        this._skills = new List<Skill>();
        for (int i = 0; i < monsterComponent.skillNames.Count; i++) {
            _skills.Add(SkillManager.Instance.allSkills[monsterComponent.skillNames[i]]);
        }

        this._elementalWeaknesses = new Dictionary<ELEMENT, float>();
        for (int i = 0; i < monsterComponent.elementChanceWeaknesses.Count; i++) {
            ElementChance elementChance = monsterComponent.elementChanceWeaknesses[i];
            this._elementalWeaknesses.Add(elementChance.element, elementChance.chance);
        }

        this._elementalResistances = new Dictionary<ELEMENT, float>();
        for (int i = 0; i < monsterComponent.elementChanceResistances.Count; i++) {
            ElementChance elementChance = monsterComponent.elementChanceResistances[i];
            this._elementalResistances.Add(elementChance.element, elementChance.chance);
        }
    }

    #region Utilities
    public void SetCharacterColor(Color color) {
        _characterColor = color;
        _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
    }
    public void Death() {
        _isDead = true;
        Messenger.RemoveListener<ActionThread>("LookForAction", AdvertiseSelf);
        if (_specificLocation != null) {
            _specificLocation.RemoveCharacterFromLocation(this);
        }
        ObjectState deadState = _monsterObj.GetState("Dead");
        _monsterObj.ChangeState(deadState);
        Messenger.Broadcast(Signals.MONSTER_DEATH, this);

        GameObject.Destroy(_icon.gameObject);
        _icon = null;
    }
    private float GetAttackPower() {
        //float statUsed = (float) Utilities.GetStatByClass(this);
        float weaponAttack = (float)attackPower;
        return (((weaponAttack + (float)strength) * (1f + ((float) strength / 100f))) * (1f + ((float) agility / 100f))) * (1f + ((float) level / 100f));
    }
    private float GetDefensePower() {
        return ((float) (strength + intelligence + _pDef + _mDef + maxHP + (2 * vitality)) * (1f + ((float) level / 100f))) * (1f + ((float) agility / 100f));
    }
    private List<Skill> GetGeneralSkills() {
        List<Skill> allGeneralSkills = new List<Skill>();
        foreach (Skill skill in SkillManager.Instance.generalSkills.Values) {
            if(skill is FleeSkill) {
                continue;
            }
            allGeneralSkills.Add(skill.CreateNewCopy());
        }
        return allGeneralSkills;
    }
    public void GoToLocation(ILocation targetLocation, PATHFINDING_MODE pathfindingMode, Action doneAction = null) {
        if (specificLocation == targetLocation) {
            //action doer is already at the target location
            if (doneAction != null) {
                doneAction();
            }
        } else {
            _icon.SetActionOnTargetReached(doneAction);
            _icon.SetTarget(targetLocation);
        }
    }
    #endregion

    #region Interface
    private void BaseInitialize() {
        _isDead = false;
        CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(Utilities.NormalizeString(_type.ToString()));
        _raceSetting = setup.raceSetting.CreateNewCopy();
        _battleOnlyTracker = new CharacterBattleOnlyTracker();
        _bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
        if (_skills == null) {
            _skills = new List<Skill>();
        }
        _skills.AddRange(GetGeneralSkills());
        _currentHP = _maxHP;
        _currentSP = _maxSP;
        SetCharacterColor(Color.red);
        Messenger.AddListener<ActionThread>("LookForAction", AdvertiseSelf);
#if !WORLD_CREATION_TOOL
        _monsterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.MONSTER, "MonsterObject") as MonsterObj;
        _monsterObj.SetMonster(this);
#endif

    }
    public void Initialize() {
        _id = Utilities.SetID(this);
        BaseInitialize();
    }
    public void Initialize(MonsterSaveData data){
        _id = Utilities.SetID(this, data.id);
        BaseInitialize();
    }
    public void SetSide(SIDES side) {
        _currentSide = side;
    }
    public void SetRowNumber(int rowNumber) {
        _currentRow = rowNumber;
    }
    public void AdjustSP(int amount) {
        _currentSP += amount;
        _currentSP = Mathf.Clamp(_currentSP, 0, _maxSP);
    }
    public void AdjustHP(int amount) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                FaintOrDeath();
            }
        }
    }
    public void FaintOrDeath() {
        this.currentCombat.CharacterDeath(this);
        Death();
    }
    public int GetPDef(ICharacter enemy) {
        return _pDef;
    }
    public int GetMDef(ICharacter enemy) {
        return _mDef;
    }
    public void ResetToFullHP() {
        AdjustHP(_maxHP);
    }
    public void ResetToFullSP() {
        AdjustSP(_maxSP);
    }
    public void SetSpecificLocation(ILocation specificLocation) {
        _specificLocation = specificLocation;
        if (_specificLocation != null) {
            _currentRegion = _specificLocation.tileLocation.region;
        }
    }
    private ILocation GetSpecificLocation() {
        if (_specificLocation != null) {
            return _specificLocation;
        } else {
            if (_icon != null) {
                Collider2D collide = Physics2D.OverlapCircle(icon.aiPath.transform.position, 1f, LayerMask.GetMask("Hextiles"));
                //Collider[] collide = Physics.OverlapSphere(icon.aiPath.transform.position, 5f);
                HexTile tile = collide.gameObject.GetComponent<HexTile>();
                if (tile != null) {
                    return tile;
                } else {
                    LandmarkObject landmarkObject = collide.gameObject.GetComponent<LandmarkObject>();
                    if (landmarkObject != null) {
                        return landmarkObject.landmark.tileLocation;
                    }
                }
            }
            return null;
        }
    }
    public void EnableDisableSkills(Combat combat) {
        bool isAllAttacksInRange = true;
        bool isAttackInRange = false;

        //Body part skills / general skills
        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            skill.isEnabled = true;

            if (skill is AttackSkill) {
                isAttackInRange = combat.HasTargetInRangeForSkill(skill, this);
                if (!isAttackInRange) {
                    isAllAttacksInRange = false;
                    skill.isEnabled = false;
                    continue;
                }
            } else if (skill is FleeSkill) {
                if (this.currentHP >= (this.maxHP / 2)) {
                    skill.isEnabled = false;
                    continue;
                }
            }
        }

        for (int i = 0; i < this._skills.Count; i++) {
            Skill skill = this._skills[i];
            if (skill is MoveSkill) {
                skill.isEnabled = true;
                if (isAllAttacksInRange) {
                    skill.isEnabled = false;
                    continue;
                }
                if (skill.skillName == "MoveLeft") {
                    if (this._currentRow == 1) {
                        skill.isEnabled = false;
                        continue;
                    } else {
                        bool hasEnemyOnLeft = false;
                        if (combat.charactersSideA.Contains(this)) {
                            for (int j = 0; j < combat.charactersSideB.Count; j++) {
                                ICharacter enemy = combat.charactersSideB[j];
                                if (enemy.currentRow < this._currentRow) {
                                    hasEnemyOnLeft = true;
                                    break;
                                }
                            }
                        } else {
                            for (int j = 0; j < combat.charactersSideA.Count; j++) {
                                ICharacter enemy = combat.charactersSideA[j];
                                if (enemy.currentRow < this._currentRow) {
                                    hasEnemyOnLeft = true;
                                    break;
                                }
                            }
                        }
                        if (!hasEnemyOnLeft) {
                            skill.isEnabled = false;
                            continue;
                        }
                    }
                } else if (skill.skillName == "MoveRight") {
                    if (this._currentRow == 5) {
                        skill.isEnabled = false;
                    } else {
                        bool hasEnemyOnRight = false;
                        if (combat.charactersSideA.Contains(this)) {
                            for (int j = 0; j < combat.charactersSideB.Count; j++) {
                                ICharacter enemy = combat.charactersSideB[j];
                                if (enemy.currentRow > this._currentRow) {
                                    hasEnemyOnRight = true;
                                    break;
                                }
                            }
                        } else {
                            for (int j = 0; j < combat.charactersSideA.Count; j++) {
                                ICharacter enemy = combat.charactersSideA[j];
                                if (enemy.currentRow > this._currentRow) {
                                    hasEnemyOnRight = true;
                                    break;
                                }
                            }
                        }
                        if (!hasEnemyOnRight) {
                            skill.isEnabled = false;
                            continue;
                        }
                    }
                }
            }
        }
    }
    public void SetHomeLandmark(BaseLandmark newHomeLandmark) {
        this._homeLandmark = newHomeLandmark;
    }
    public void GoHome() {
        GoToLocation(_homeStructure.objectLocation, PATHFINDING_MODE.USE_ROADS);
    }
    public void AdvertiseSelf(ActionThread actionThread) {
        actionThread.AddToChoices(_monsterObj);
    }
    public void SetHomeStructure(StructureObj newHomeStructure) {
        this._homeStructure = newHomeStructure;
    }
    #endregion

    #region Icon
    /*
    Create a new icon for this character.
    Each character owns 1 icon.
        */
    public void CreateIcon() {
        GameObject characterIconGO = GameObject.Instantiate(MonsterManager.Instance.monsterIconPrefab,
            Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);
        _icon = characterIconGO.GetComponent<CharacterIcon>();
        _icon.SetCharacter(this);
        //PathfindingManager.Instance.AddAgent(_icon.aiPath);
    }
    #endregion
}
