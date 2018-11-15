using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassTraitButton : MonoBehaviour {
    public Text buttonText;

    private string _traitName;

    #region getters/setters
    public string traitName {
        get { return _traitName; }
    }
    #endregion

    public void SetCurrentlySelectedButton() {
        ClassPanelUI.Instance.currentSelectedClassTraitButton = this;
    }
    public void SetTraitName(string traitName) {
        _traitName = traitName;
        buttonText.text = _traitName;
    }
}
