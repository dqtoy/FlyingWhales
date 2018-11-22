using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;
using UnityEngine.EventSystems;

public class PlayerCharacterItem : PooledObject, IDragParentItem {
    public TextMeshProUGUI nameLbl;
    public TextMeshProUGUI lvlLbl;
    public Slider expSlider;
    public CharacterPortrait portrait;
    public GameObject grayedOutGO;
    public MinionDraggable minionDraggable;
    public Image bgImage;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    public TweenPosition tweenPos;
    public int supposedIndex;

    private ICharacter _character;

    #region getters/setters
    public ICharacter character {
        get { return _character; }
    }
    public Minion minion {
        get { return _character.minion; }
    }
    public object associatedObj {
        get { return _character; }
    }
    #endregion

    public void SetCharacter(ICharacter character) {
        _character = character;
        if (_character == null) {
            //character is null
            portrait.gameObject.SetActive(false);
            bgImage.sprite = lockedSprite;
            nameLbl.text = "???";
            minionDraggable.SetDraggable(false);
            minionDraggable.SetAssociatedObject(null);
            grayedOutGO.SetActive(false);
        } else if (_character.minion != null) {
            //chartacter is a minion
            _character.minion.SetPlayerCharacterItem(this);
            portrait.GeneratePortrait(_character, 104);
            portrait.gameObject.SetActive(true);
            bgImage.sprite = unlockedSprite;
            nameLbl.text = _character.name;
            minionDraggable.SetDraggable(true);
            minionDraggable.SetAssociatedObject(_character.minion);
        } else if(_character != null) {
            //character is a character
            portrait.GeneratePortrait(_character, 104);
            portrait.gameObject.SetActive(true);
            bgImage.sprite = unlockedSprite;
            nameLbl.text = _character.name;
            minionDraggable.SetDraggable(true);
            minionDraggable.SetAssociatedObject(_character);
        }
        UpdateMinionItem();
    }

    public void UpdateMinionItem() {
        if (_character == null) {
            lvlLbl.text = "Lvl. ? ???";
            expSlider.value = 0f;
        } else if (_character.minion != null) {
            lvlLbl.text = "Lvl. " + _character.minion.lvl + " " + (_character.minion.type == DEMON_TYPE.NONE ? _character.characterClass.className : _character.minion.strType);
            expSlider.value = (float)_character.minion.exp / 100f;
        } else if(_character != null) {
            lvlLbl.text = "Lvl. " + _character.level;
            expSlider.value = (float)_character.experience / _character.maxExperience;
        }
    }
    public void SetEnabledState(bool state) {
        minionDraggable.SetDraggable(state);
        grayedOutGO.SetActive(!state);
    }
    public void OnFinishRearrange() {
        this.transform.SetSiblingIndex(supposedIndex);
    }

    //public void PointerClicked(BaseEventData data) {
    //    PointerEventData ped = (PointerEventData)data;
    //    if (ped.button == PointerEventData.InputButton.Right) {
    //        //Debug.Log("Right click");
    //        if (minion.icharacter.currentParty.icharacters.Count > 1) {
    //            UIManager.Instance.ShowPartyInfo(minion.icharacter.currentParty);
    //        }
    //    }
    //}
}
