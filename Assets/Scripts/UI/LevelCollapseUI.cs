using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelCollapseUI : MonoBehaviour {
    public Text workAroundText;
    public Text lvlText;
    public Dropdown skillsOptions;
    public Image arrowImg;
    public Transform skillsContentTransform;
    public GameObject skillsGO;
    public GameObject skillsPerLevelBtnGO;

    private bool _isShowing;

    [NonSerialized] public SkillsPerLevelButton currentSelectedButton;

    private List<string> _skills;

    #region getters/setters
    public List<string> skills {
        get { return _skills; }
    }
    #endregion
    void Awake() {
        if(_skills == null) {
            _skills = new List<string>();
        }
    }
    void Start() {
        UpdateSkillList();
    }
    public void ToggleCollapse() {
        _isShowing = !_isShowing;
        if (_isShowing) {
            UpdateSkillList();
            arrowImg.rectTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
            skillsGO.SetActive(true);
            workAroundText.text = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee" +
                "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeerrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr";
        } else {
            arrowImg.rectTransform.localEulerAngles = new Vector3(0f, 0f, 90f);
            skillsGO.SetActive(false);
            workAroundText.text = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeereeeeeeee";
        }
        //ClassPanelUI.Instance.skillsScrollView.Rebuild(CanvasUpdate.PostLayout);
        //StartCoroutine(UpdateSkillsSrollView());
    }

    #region Utilities
    public void UpdateSkillList() {
        skillsOptions.ClearOptions();
        skillsOptions.AddOptions(SkillPanelUI.Instance.allSkills);
    }
    private IEnumerator UpdateSkillsSrollView() {
        yield return null;
        RectTransform thisTransform = this.transform as RectTransform;
        Vector2 newSize = thisTransform.sizeDelta;
        if (_isShowing) {
            newSize.y = 200f;
            //(this.transform as RectTransform).sizeDelta = new Vector2((this.transform as RectTransform).sizeDelta.x, 200f);
        } else {
            newSize.y = 30f;
            //(this.transform as RectTransform).sizeDelta = new Vector2((this.transform as RectTransform).sizeDelta.x, 30f);
        }
        (this.transform as RectTransform).sizeDelta = newSize;
        Canvas.ForceUpdateCanvases();
    }
    public void SetSkills(List<string> skills) {
        _skills = new List<string>(skills);
        for (int i = 0; i < _skills.Count; i++) {
            GameObject go = GameObject.Instantiate(skillsPerLevelBtnGO, skillsContentTransform);
            go.GetComponent<SkillsPerLevelButton>().buttonText.text = _skills[i];
            go.GetComponent<SkillsPerLevelButton>().collapseUI = this;
        }
    }
    #endregion

    #region Button Clicks
    public void OnClickAdd() {
        string skillToAdd = skillsOptions.options[skillsOptions.value].text;
        if (!_skills.Contains(skillToAdd)) {
            _skills.Add(skillToAdd);
            GameObject go = GameObject.Instantiate(skillsPerLevelBtnGO, skillsContentTransform);
            go.GetComponent<SkillsPerLevelButton>().buttonText.text = skillToAdd;
            go.GetComponent<SkillsPerLevelButton>().collapseUI = this;
        }
    }
    public void OnClickRemove() {
        if (currentSelectedButton != null) {
            string skillToRemove = currentSelectedButton.buttonText.text;
            if (_skills.Remove(skillToRemove)) {
                GameObject.Destroy(currentSelectedButton.gameObject);
                currentSelectedButton = null;
            }
        }
    }
    #endregion
}
