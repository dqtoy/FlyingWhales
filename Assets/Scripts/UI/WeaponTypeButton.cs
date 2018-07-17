using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponTypeButton : MonoBehaviour {
    public Text buttonText;

    public void SetCurrentlySelectedButton() {
        SkillPanelUI.Instance.currentSelectedWeaponTypeButton = this;
    }
}
