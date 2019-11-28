using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterBehaviourComponent {

    private List<Character> _isDisabledFor;

    public abstract bool TryDoBehaviour(Character character, ref string log);

    #region Enabling/Disabling
    protected void DisableFor(Character character) {
        if (_isDisabledFor == null) { _isDisabledFor = new List<Character>(); }
        _isDisabledFor.Add(character);
    }
    protected void EnableFor(Character character) {
        _isDisabledFor.Remove(character);
    }
    public bool IsDisabledFor(Character character) {
        if (_isDisabledFor != null) {
            return _isDisabledFor.Contains(character);
        }
        return false;
    }
    #endregion

}
