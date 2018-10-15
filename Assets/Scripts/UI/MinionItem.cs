using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using TMPro;

public class MinionItem : MonoBehaviour {
    public TextMeshProUGUI nameLbl;
    public TextMeshProUGUI lvlLbl;
    public Slider expSlider;
    public CharacterPortrait portrait;
    public GameObject grayedOutGO;
    public MinionDraggable minionDraggable;
    public Image bgImage;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    private Minion _minion;

    #region getters/setters
    public Minion minion {
        get { return _minion; }
    }
    #endregion

    public void SetMinion(Minion minion) {
        _minion = minion;
        if(_minion != null) {
            _minion.SetMinionItem(this);
            portrait.GeneratePortrait(minion.icharacter.portraitSettings, (int) portrait.gameObject.GetComponent<RectTransform>().rect.width, true);
            portrait.gameObject.SetActive(true);
            bgImage.sprite = unlockedSprite;
            nameLbl.text = _minion.icharacter.name;
            minionDraggable.SetDraggable(true);
        } else {
            portrait.gameObject.SetActive(false);
            bgImage.sprite = lockedSprite;
            nameLbl.text = "???";
            minionDraggable.SetDraggable(false);
        }
        UpdateMinionItem();
    }

    public void UpdateMinionItem() {
        if (_minion != null) {
            lvlLbl.text = "Lvl. " + _minion.lvl + " " + _minion.strType;
            expSlider.value = (float) _minion.exp / 100f;
        } else {
            lvlLbl.text = "Lvl. ? ???";
            expSlider.value = 0f;
        }
    }
    public void SetEnabledState(bool state) {
        minionDraggable.SetDraggable(state);
        grayedOutGO.SetActive(!state);
    }
}
