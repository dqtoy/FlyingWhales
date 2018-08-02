using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class EditStorylinesMenu : MonoBehaviour {
    public static EditStorylinesMenu Instance;

    public Dropdown storylineOptions;
    public Transform contentTransform;
    public Transform editorTransform;
    public GameObject storylineBtnPrefab;
    [NonSerialized] public StorylineBtn currentSelectedBtn;
    [NonSerialized] public List<StorylineBtn> _storylineButtons;

    [Space(10)]
    [Header("Storyline Editor Prefabs")]
    public GameObject dyingKingEditor;

    // Use this for initialization
    void Awake () {
        Instance = this;
	}
    void Start() {
        LoadAllStorylinesOptions();
    }

    #region Button Clicks
    public void OnClickAddStoryline() {
        STORYLINE chosenType = (STORYLINE) System.Enum.Parse(typeof(STORYLINE), storylineOptions.options[storylineOptions.value].text);
        if (!AlreadyHasStoryline(chosenType)) {
            GameObject go = GameObject.Instantiate(storylineBtnPrefab, contentTransform);
            StorylineBtn storylineBtn = go.GetComponent<StorylineBtn>();
            Storyline storyline = CreateStoryline(chosenType);
            storylineBtn.storyline = storyline;
            storylineBtn.buttonText.text = storyline.storylineType.ToString();
            _storylineButtons.Add(storylineBtn);
        }
    }
    public void OnClickRemoveStoryline() {
        if(currentSelectedBtn != null) {
            GameObject.Destroy(currentSelectedBtn.gameObject);
            _storylineButtons.Remove(currentSelectedBtn);
            currentSelectedBtn = null;
        }
    }
    public void OnClickEditStoryline() {
        if (currentSelectedBtn != null) {
            GameObject prefab = GetEditorPrefab(currentSelectedBtn.storyline.storylineType);
            GameObject go = GameObject.Instantiate(prefab, editorTransform);
            StorylineEditor editor = go.GetComponent<StorylineEditor>();
            editor.PopulateData(currentSelectedBtn);
        }
    }
    #endregion

    #region Utilities
    private void LoadAllStorylinesOptions() {
        storylineOptions.ClearOptions();

        string[] storylines = System.Enum.GetNames(typeof(STORYLINE));

        storylineOptions.AddOptions(storylines.ToList());
    }
    public bool AlreadyHasStoryline(STORYLINE type) {
        for (int i = 0; i < _storylineButtons.Count; i++) {
            if(_storylineButtons[i].storyline.storylineType == type) {
                return true;
            }
        }
        return false;
    }
    public Storyline CreateStoryline(STORYLINE type) {
        switch (type) {
            case STORYLINE.THE_DYING_KING:
            return new TheDyingKing();
        }
        return null;
    }
    public GameObject GetEditorPrefab(STORYLINE type) {
        switch (type) {
            case STORYLINE.THE_DYING_KING:
            return dyingKingEditor;
        }
        return null;
    }
    #endregion
}
