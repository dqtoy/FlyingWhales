using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using ECS;
using TMPro;

public class CombatSimManager : MonoBehaviour {
    public static CombatSimManager Instance;

    public TextMeshProUGUI combatText;

    public Dropdown sideAOptions;
    public Dropdown sideBOptions;

    public Transform sideAContentTransform;
    public Transform sideBContentTransform;

    public GameObject combatPanel;
    public GameObject editorPanel;
    public GameObject characterSimBtnPrefab;

    [NonSerialized] public CharacterSimButton currentlySelectedSideAButton;
    [NonSerialized] public CharacterSimButton currentlySelectedSideBButton;
    [NonSerialized] public CombatSim currentCombat;


    private string characterPath;
    private string monsterPath;
    private List<ICharacterSim> _sideAList;
    private List<ICharacterSim> _sideBList;
    private List<string> _allMonsters;
    private List<string> _allCharacters;
    private Dictionary<ELEMENT, float> _elementsChanceDictionary;
    private Dictionary<WEAPON_TYPE, WeaponType> _weaponTypeData;
    private Dictionary<WEAPON_PREFIX, WeaponPrefix> _weaponPrefixes;
    private Dictionary<WEAPON_SUFFIX, WeaponSuffix> _weaponSuffixes;

    #region getters/setters
    public Dictionary<ELEMENT, float> elementsChanceDictionary {
        get { return _elementsChanceDictionary; }
    }
    public Dictionary<WEAPON_TYPE, WeaponType> weaponTypeData {
        get { return _weaponTypeData; }
    }
    public Dictionary<WEAPON_PREFIX, WeaponPrefix> weaponPrefixes {
        get { return _weaponPrefixes; }
    }
    public Dictionary<WEAPON_SUFFIX, WeaponSuffix> weaponSuffixes {
        get { return _weaponSuffixes; }
    }
    public List<ICharacterSim> sideAList {
        get { return _sideAList; }
    }
    public List<ICharacterSim> sideBList {
        get { return _sideBList; }
    }
    #endregion

    void Awake() {
        Instance = this;  
    }

    void Start () {
        characterPath = Utilities.dataPath + "CharacterSims/";
        monsterPath = Utilities.dataPath + "Monsters/";
        _allMonsters = new List<string>();
        _allCharacters = new List<string>();
        _sideAList = new List<ICharacterSim>();
        _sideBList = new List<ICharacterSim>();
        ConstructElementChanceDictionary();
        ConstructWeaponTypeData();
        ConstructWeaponPrefixesAndSuffixes();
        UpdateAllCharacters();
        UpdateAllMonsters();
        UpdateCharacterOptions();
	}

    #region Utilities
    public void UpdateAllCharacters() {
        _allCharacters.Clear();
        foreach (string file in Directory.GetFiles(characterPath, "*.json")) {
            _allCharacters.Add(Path.GetFileNameWithoutExtension(file));
        }
    }
    public void UpdateAllMonsters() {
        _allMonsters.Clear();
        foreach (string file in Directory.GetFiles(monsterPath, "*.json")) {
            _allMonsters.Add(Path.GetFileNameWithoutExtension(file));
        }
    }
    private void UpdateCharacterOptions() {
        sideAOptions.ClearOptions();
        sideAOptions.AddOptions(_allCharacters);
        sideAOptions.AddOptions(_allMonsters);

        sideBOptions.ClearOptions();
        sideBOptions.AddOptions(_allCharacters);
        sideBOptions.AddOptions(_allMonsters);
    }
    private bool SideAContains(string name) {
        for (int i = 0; i < _sideAList.Count; i++) {
            if(_sideAList[i].name == name) {
                return true;
            }
        }
        return false;
    }
    private bool SideBContains(string name) {
        for (int i = 0; i < _sideBList.Count; i++) {
            if (_sideBList[i].name == name) {
                return true;
            }
        }
        return false;
    }
    private void ResetCharacters() {
        for (int i = 0; i < _sideAList.Count; i++) {
            _sideAList[i].ResetToFullHP();
            _sideAList[i].ResetToFullSP();
        }
        for (int i = 0; i < _sideBList.Count; i++) {
            _sideBList[i].ResetToFullHP();
            _sideBList[i].ResetToFullSP();
        }
        combatText.text = string.Empty;
    }
    private void ConstructElementChanceDictionary() {
        _elementsChanceDictionary = new Dictionary<ELEMENT, float>();
        ELEMENT[] elements = (ELEMENT[]) System.Enum.GetValues(typeof(ELEMENT));
        for (int i = 0; i < elements.Length; i++) {
            _elementsChanceDictionary.Add(elements[i], 0f);
        }
    }
    private void ConstructWeaponTypeData() {
        _weaponTypeData = new Dictionary<WEAPON_TYPE, WeaponType>();
        string path = Utilities.dataPath + "WeaponTypes/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            string currFilePath = files[i];
            string dataAsJson = File.ReadAllText(currFilePath);
            WeaponType data = JsonUtility.FromJson<WeaponType>(dataAsJson);
            _weaponTypeData.Add(data.weaponType, data);
        }
    }
    private void ConstructWeaponPrefixesAndSuffixes() {
        _weaponPrefixes = new Dictionary<WEAPON_PREFIX, WeaponPrefix>();
        _weaponSuffixes = new Dictionary<WEAPON_SUFFIX, WeaponSuffix>();
        WEAPON_PREFIX[] prefixes = (WEAPON_PREFIX[]) Enum.GetValues(typeof(WEAPON_PREFIX));
        WEAPON_SUFFIX[] suffixes = (WEAPON_SUFFIX[]) Enum.GetValues(typeof(WEAPON_SUFFIX));
        for (int i = 0; i < prefixes.Length; i++) {
            CreateWeaponPrefix(prefixes[i]);
        }
        for(int i = 0; i < suffixes.Length; i++) {
            CreateWeaponSuffix(suffixes[i]);
        }

    }
    private void CreateWeaponPrefix(WEAPON_PREFIX prefix) {
        if (!_weaponPrefixes.ContainsKey(prefix)) {
            switch (prefix) {
                case WEAPON_PREFIX.NONE:
                _weaponPrefixes.Add(prefix, new WeaponPrefix(prefix));
                break;
            }
        }
    }
    private void CreateWeaponSuffix(WEAPON_SUFFIX suffix) {
        if (!_weaponSuffixes.ContainsKey(suffix)) {
            switch (suffix) {
                case WEAPON_SUFFIX.NONE:
                _weaponSuffixes.Add(suffix, new WeaponSuffix(suffix));
                break;
            }
        }

    }
    #endregion

    #region Button Clicks
    public void OnClickCombatTab() {
        combatPanel.SetActive(true);
        editorPanel.SetActive(false);
        UpdateCharacterOptions();
    }
    public void OnClickEditorTab() {
        combatPanel.SetActive(false);
        editorPanel.SetActive(true);
    }
    public void OnClickFight() {
        NewCombat();
    }
    public void OnClickReset() {
        ResetCharacters();
    }
    public void OnClickAddSideA() {
        string chosenCharacterName = sideAOptions.options[sideAOptions.value].text;
        ICharacterSim icharacterSim = null;
        if (_allCharacters.Contains(chosenCharacterName)) {
            string path = characterPath + chosenCharacterName + ".json";
            CharacterSim characterSim = JsonUtility.FromJson<CharacterSim>(System.IO.File.ReadAllText(path));
            icharacterSim = characterSim;
        } else {
            string path = monsterPath + chosenCharacterName + ".json";
            Monster monster = JsonUtility.FromJson<Monster>(System.IO.File.ReadAllText(path));
            icharacterSim = monster;
        }
        icharacterSim.InitializeSim();
        _sideAList.Add(icharacterSim);
        GameObject go = GameObject.Instantiate(characterSimBtnPrefab, sideAContentTransform);
        go.GetComponent<CharacterSimButton>().buttonText.text = icharacterSim.idName;
        go.GetComponent<CharacterSimButton>().side = SIDES.A;
        go.GetComponent<CharacterSimButton>().icharacterSim = icharacterSim;
    }
    public void OnClickAddSideB() {
        string chosenCharacterName = sideBOptions.options[sideBOptions.value].text;
        ICharacterSim icharacterSim = null;
        if (_allCharacters.Contains(chosenCharacterName)) {
            string path = characterPath + chosenCharacterName + ".json";
            CharacterSim characterSim = JsonUtility.FromJson<CharacterSim>(System.IO.File.ReadAllText(path));
            icharacterSim = characterSim;
        } else {
            string path = monsterPath + chosenCharacterName + ".json";
            Monster monster = JsonUtility.FromJson<Monster>(System.IO.File.ReadAllText(path));
            icharacterSim = monster;
        }
        icharacterSim.InitializeSim();
        _sideBList.Add(icharacterSim);
        GameObject go = GameObject.Instantiate(characterSimBtnPrefab, sideBContentTransform);
        go.GetComponent<CharacterSimButton>().buttonText.text = icharacterSim.idName;
        go.GetComponent<CharacterSimButton>().side = SIDES.B;
        go.GetComponent<CharacterSimButton>().icharacterSim = icharacterSim;
    }
    public void OnClickRemoveSideA() {
        if(currentlySelectedSideAButton != null) {
            if (_sideAList.Remove(currentlySelectedSideAButton.icharacterSim)) {
                GameObject.Destroy(currentlySelectedSideAButton.gameObject);
                currentlySelectedSideAButton = null;
            }
        }
    }
    public void OnClickRemoveSideB() {
        if (currentlySelectedSideBButton != null) {
            if (_sideBList.Remove(currentlySelectedSideBButton.icharacterSim)) {
                GameObject.Destroy(currentlySelectedSideBButton.gameObject);
                currentlySelectedSideBButton = null;
            }
        }
    }
    #endregion

    #region Combat
    private void NewCombat() {
        combatText.text = string.Empty;
        currentCombat = new CombatSim();
        for (int i = 0; i < _sideAList.Count; i++) {
            currentCombat.AddCharacter(SIDES.A, _sideAList[i]);
        }
        for (int i = 0; i < _sideBList.Count; i++) {
            currentCombat.AddCharacter(SIDES.B, _sideBList[i]);
        }
        currentCombat.CombatSimulation();
    }
    #endregion
}
