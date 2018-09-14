using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secret {
    public int id;
    public string name;
    public string description;
    public int intelIDToBeUnlocked;

    private bool _isRevealed;
    private string _displayText;

    #region getters/setters
    public bool isRevealed {
        get { return _isRevealed; }
    }
    public string displayText {
        get { return _displayText; }
    }
    #endregion

    public Secret() {
        _displayText = "???";
    }
    public void SetData(SecretComponent secretComponent) {
        id = secretComponent.id;
        name = secretComponent.thisName;
        description = secretComponent.description;
        intelIDToBeUnlocked = secretComponent.intelIDToBeUnlocked;
    }
    public void RevealSecret() {
        if (!_isRevealed) {
            _isRevealed = true;
            _displayText = name;
            if(intelIDToBeUnlocked != -1) {
                Intel intel = IntelManager.Instance.intelLookup[intelIDToBeUnlocked];
                PlayerManager.Instance.player.AddIntel(intel);
            }
            //Reveal goals/subgoals
        }
    }
}
