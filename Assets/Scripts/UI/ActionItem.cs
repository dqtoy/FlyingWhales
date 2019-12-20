using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionItem : PooledObject {

	[SerializeField] private Button button;
	[SerializeField] private Image actionImg;
	[SerializeField] private Image coverImg;
	[SerializeField] private TextMeshProUGUI actionLbl;

	private string expiryKey;
	
	public void SetAction(System.Action action, Sprite icon, string actionName) {
		button.onClick.AddListener(action.Invoke);
		if (icon != null) {
			actionImg.sprite = icon;	
		}
		actionLbl.text = actionName;
		SetAsClickable();
	}
	public void SetAsUninteractableUntil(int ticks) {
		GameDate date = GameManager.Instance.Today();
		date = date.AddTicks(ticks);
		SetAsUninteractableUntil(date);
	}
	public void SetAsUninteractableUntil(GameDate date) {
        SetInteractable(false);
        expiryKey = SchedulingManager.Instance.AddEntry(date, SetAsClickable, this);
	}
	private void SetAsClickable() {
        SetInteractable(true);
	}
    public void SetInteractable(bool state) {
        button.interactable = state;
        coverImg.gameObject.SetActive(!state);
    }

    public override void Reset() {
		base.Reset();
		button.onClick.RemoveAllListeners();
		if (string.IsNullOrEmpty(expiryKey) == false) {
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
		expiryKey = string.Empty;
	}
}
