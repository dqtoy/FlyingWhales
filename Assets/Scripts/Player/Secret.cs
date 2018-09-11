using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secret {
    public int id;
    public string name;
    public string description;
    public int intelIDToBeUnlocked;

    private bool _isRevealed;

    #region getters/setters
    public bool isRevealed {
        get { return _isRevealed; }
    }
    #endregion

    public void RevealSecret() {
        if (!_isRevealed) {
            _isRevealed = true;
            if(intelIDToBeUnlocked != -1) {
                Intel intel = IntelManager.Instance.intelLookup[intelIDToBeUnlocked];
                PlayerManager.Instance.player.AddIntel(intel);
            }
            //Reveal goals/subgoals
        }
    }
}
