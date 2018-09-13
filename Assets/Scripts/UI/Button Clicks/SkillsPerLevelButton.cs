using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SkillsPerLevelButton : MonoBehaviour {
    public Text buttonText;
    [NonSerialized] public LevelCollapseUI collapseUI;

    public void SetCurrentlySelectedButton() {
        collapseUI.currentSelectedButton = this;
    }
}
