using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

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

    #region getters/setters
    public Dictionary<ELEMENT, float> elementsChanceDictionary {
        get { return _elementsChanceDictionary; }
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
            //string path = monsterPath + chosenCharacterName + ".json";
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
            //string path = monsterPath + chosenCharacterName + ".json";
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
