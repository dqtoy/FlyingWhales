using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attribute {
    [SerializeField] protected string _name;
    [SerializeField] protected bool _isHidden;
    [SerializeField] protected ATTRIBUTE_CATEGORY _category;

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public bool isHidden {
        get { return _isHidden; }
    }
    public ATTRIBUTE_CATEGORY category {
        get { return _category; }
    }
    #endregion

    public void SetDataFromAttributePanelUI() {
        _name = AttributePanelUI.Instance.nameInput.text;
        _isHidden = AttributePanelUI.Instance.hiddenToggle.isOn;
        _category = (ATTRIBUTE_CATEGORY) System.Enum.Parse(typeof(ATTRIBUTE_CATEGORY), AttributePanelUI.Instance.categoryOptions.options[AttributePanelUI.Instance.categoryOptions.value].text);
    }
}
