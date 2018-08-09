﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StorylineInfoUI : UIMenu {

    [SerializeField] private TextMeshProUGUI storylineTitleLbl;
    [SerializeField] private TextMeshProUGUI storylineDescriptionLbl;
    [SerializeField] private StorylineEventItem[] eventItems;

    internal override void Initialize() {
        base.Initialize();
    }

    public override void ShowMenu() {
        base.ShowMenu();
    }
}
