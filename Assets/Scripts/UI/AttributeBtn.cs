using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttributeBtn : MonoBehaviour {
    public Text buttonText;

	public void OnClickAttribute() {
        ItemPanelUI.Instance.SetCurrentlySelectedButton(this);
    }
}
