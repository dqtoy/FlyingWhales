using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;

public class MinionItem : MonoBehaviour {
    public CharacterPortrait portrait;
    public GameObject grayedOutGO;

    private Minion _minion;

    #region getters/setters
    public Minion minion {
        get { return _minion; }
    }
    #endregion

    public void SetMinion(Minion minion) {
        _minion = minion;
        _minion.SetMinionItem(this);
        portrait.GeneratePortrait(minion.icharacter.portraitSettings, (int) portrait.gameObject.GetComponent<RectTransform>().rect.width, true);
    }

    public void SetEnabledState(bool state) {
        GetComponent<ReorderableListElement>().isDraggable = state;
        grayedOutGO.SetActive(!state);
    }
}
