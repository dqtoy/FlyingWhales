using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CombatSlotItem : MonoBehaviour {
    public CharacterPortrait portrait;
    public GameObject glowGO;
    public GameObject targetSelectGO;
    public Image hpProgress; 
    public SIDES side;

    public Character character { get; private set; }
    public string hoverInfo { get; private set; }
    public int gridNumber { get; private set; }

    public bool isTargetable {
        get { return targetSelectGO.activeSelf; }
    }

    public void Initialize() {
        Messenger.AddListener<Character>(Signals.ADJUSTED_HP, OnAdjustCharacterHP);
    }
    public void SetCharacter(Character character) {
        this.character = character;
        if(this.character != null) {
            portrait.gameObject.SetActive(true);
            portrait.GeneratePortrait(character);
            hoverInfo = this.character.name;
            UpdateHPBar();
        } else {
            portrait.gameObject.SetActive(false);
        }
    }
    public void SetGridNumber(int number) {
        gridNumber = number;
    }
    public void ShowCharacterInfo() {
        if (UIManager.Instance.combatUI.CanSlotBeTarget(this)) {
            UIManager.Instance.combatUI.ShowTargetCharacters(this);
        }
        if (character != null) {
            UIManager.Instance.ShowSmallInfo(hoverInfo);
        }
    }
    public void HideCharacterInfo() {
        if (character != null) {
            UIManager.Instance.HideSmallInfo();
        }
        UIManager.Instance.combatUI.HideTargetCharacters(this);
    }
    public void ShowCharacterUI() {
        if (character != null && portrait.gameObject.activeSelf) {
            UIManager.Instance.ShowCharacterInfo(character);
        }
    }
    public void OnHoverHP() {
        string hp = character.currentHP + "/" + character.maxHP;
        UIManager.Instance.ShowSmallInfo(hp);
    }
    public void OnHoverOutHP() {
        UIManager.Instance.HideSmallInfo();
    }
    public void SetHighlight(bool state) {
        glowGO.SetActive(state);
    }
    public void SetTargetable(bool state) {
        targetSelectGO.SetActive(state);
    }
    public void OnClickCombatSlot(BaseEventData eventData) {
        PointerEventData pointerEvent = eventData as PointerEventData;
        if (pointerEvent.button == PointerEventData.InputButton.Left) {
            UIManager.Instance.combatUI.SelectTargetCharacters(this);
        } else if (pointerEvent.button == PointerEventData.InputButton.Right) {
            ShowCharacterUI();
        }
    }
    private void OnAdjustCharacterHP(Character character) {
        if(this.character != null && this.character.id == character.id) {
            UpdateHPBar();
        }
    }
    private void UpdateHPBar() {
        hpProgress.fillAmount = this.character.currentHP / (float) this.character.maxHP;
    }
}
