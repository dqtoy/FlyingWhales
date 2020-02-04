using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterSkillButton : MonoBehaviour {
    public Text buttonText;

    public void SetCurrentlySelectedButton() {
        MonsterPanelUI.Instance.currentSelectedButton = this;
    }
}
