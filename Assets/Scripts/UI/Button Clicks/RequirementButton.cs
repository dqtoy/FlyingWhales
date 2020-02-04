using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RequirementButton : MonoBehaviour {
    public Text buttonText;
    private string _requirement;

    #region getters/setters
    public string requirement {
        get { return _requirement; }
    }
    #endregion

    public void SetCurrentlySelectedButton() {
        TraitPanelUI.Instance.currentSelectedRequirementButton = this;
    }
    public void SetRequirement(string requirement) {
        _requirement = requirement;
        buttonText.text = _requirement;
    }
}
