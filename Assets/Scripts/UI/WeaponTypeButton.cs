using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponTypeButton : MonoBehaviour {
    public Text buttonText;
    public string panelName;

    public void SetCurrentlySelectedButton() {
        if(panelName == "skill") {
            SkillPanelUI.Instance.currentSelectedWeaponTypeButton = this;
        }else if (panelName == "class") {
            ClassPanelUI.Instance.currentSelectedWeaponTypeButton = this;
        }
    }
}
