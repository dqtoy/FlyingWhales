using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class CharacterSim : ICharacterSim {
    [SerializeField] private string _name;
    [SerializeField] private string _className;
    [SerializeField] private int _level;
    [SerializeField] private int _strBuild;
    [SerializeField] private int _intBuild;
    [SerializeField] private int _agiBuild;
    [SerializeField] private int _vitBuild;
    [SerializeField] private int _str;
    [SerializeField] private int _int;
    [SerializeField] private int _agi;
    [SerializeField] private int _vit;
    [SerializeField] private int _maxHP;
    [SerializeField] private int _maxSP;
    [SerializeField] private int _weaponAttack;
    [SerializeField] private int _pDefHead;
    [SerializeField] private int _pDefBody;
    [SerializeField] private int _pDefLegs;
    [SerializeField] private int _pDefHands;
    [SerializeField] private int _pDefFeet;
    [SerializeField] private int _mDefHead;
    [SerializeField] private int _mDefBody;
    [SerializeField] private int _mDefLegs;
    [SerializeField] private int _mDefHands;
    [SerializeField] private int _mDefFeet;
    [SerializeField] private List<string> _skillNames;

    private int _currentHP;
    private int _currentSP;
    private List<Skill> _skills;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public string className {
        get { return _className; }
    }
    public int level {
        get { return _level; }
    }
    public int str {
        get { return _str; }
    }
    public int intl {
        get { return _int; }
    }
    public int agi {
        get { return _agi; }
    }
    public int vit {
        get { return _vit; }
    }
    public int maxHP {
        get { return _maxHP; }
    }
    public int maxSP {
        get { return _maxSP; }
    }
    public int strBuild {
        get { return _strBuild; }
    }
    public int intBuild {
        get { return _intBuild; }
    }
    public int agiBuild {
        get { return _agiBuild; }
    }
    public int vitBuild {
        get { return _vitBuild; }
    }
    public int weaponAttack {
        get { return _weaponAttack; }
    }
    public int pDefHead {
        get { return _pDefHead; }
    }
    public int pDefBody {
        get { return _pDefBody; }
    }
    public int pDefLegs {
        get { return _pDefLegs; }
    }
    public int pDefHands {
        get { return _pDefHands; }
    }
    public int pDefFeet {
        get { return _pDefFeet; }
    }
    public int mDefHead {
        get { return _mDefHead; }
    }
    public int mDefBody {
        get { return _mDefBody; }
    }
    public int mDefLegs {
        get { return _mDefLegs; }
    }
    public int mDefHands {
        get { return _mDefHands; }
    }
    public int mDefFeet {
        get { return _mDefFeet; }
    }
    public List<string> skillNames {
        get { return _skillNames; }
    }
    #endregion

    public void Initialize() {
        //Construct skills
        //Construct class
    }
    public void SetDataFromCharacterPanelUI() {
        _name = CharacterPanelUI.Instance.nameInput.text;
        _className = CharacterPanelUI.Instance.classOptions.options[CharacterPanelUI.Instance.classOptions.value].text;
        _level = int.Parse(CharacterPanelUI.Instance.levelInput.text);
        _strBuild = CharacterPanelUI.Instance.strBuild;
        _intBuild = CharacterPanelUI.Instance.intBuild;
        _agiBuild = CharacterPanelUI.Instance.agiBuild;
        _vitBuild = CharacterPanelUI.Instance.vitBuild;
        _str = CharacterPanelUI.Instance.str;
        _int = CharacterPanelUI.Instance.intl;
        _agi = CharacterPanelUI.Instance.agi;
        _vit = CharacterPanelUI.Instance.vit;
        _maxHP = CharacterPanelUI.Instance.hp;
        _maxSP = CharacterPanelUI.Instance.sp;
        _weaponAttack = int.Parse(CharacterPanelUI.Instance.weaponAttackInput.text);

        _pDefHead = int.Parse(CharacterPanelUI.Instance.pHeadInput.text);
        _pDefBody = int.Parse(CharacterPanelUI.Instance.pBodyInput.text);
        _pDefLegs = int.Parse(CharacterPanelUI.Instance.pLegsInput.text);
        _pDefHands = int.Parse(CharacterPanelUI.Instance.pHandsInput.text);
        _pDefFeet = int.Parse(CharacterPanelUI.Instance.pFeetInput.text);

        _mDefHead = int.Parse(CharacterPanelUI.Instance.mHeadInput.text);
        _mDefBody = int.Parse(CharacterPanelUI.Instance.mBodyInput.text);
        _mDefLegs = int.Parse(CharacterPanelUI.Instance.mLegsInput.text);
        _mDefHands = int.Parse(CharacterPanelUI.Instance.mHandsInput.text);
        _mDefFeet = int.Parse(CharacterPanelUI.Instance.mFeetInput.text);

        _skillNames = CharacterPanelUI.Instance.skillNames;
    }


    #region Utilities
    private int GetPDefBonus() {
        return _pDefHead + _pDefBody + _pDefLegs + _pDefHands + _pDefFeet;
    }
    private int GetMDefBonus() {
        return _mDefHead + _mDefBody + _mDefLegs + _mDefHands + _mDefFeet;
    }
    #endregion
}
