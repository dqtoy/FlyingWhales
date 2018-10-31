using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvestigateButton : MonoBehaviour {
    public Toggle toggle;
    public string actionName;
    
	public void OnClickThis(bool state) {
        if (state) {
            UIManager.Instance.landmarkInfoUI.SetCurentSelectedInvestigateButton(this);
        } else {
            if (!toggle.group.AnyTogglesOn()) {
                UIManager.Instance.landmarkInfoUI.SetCurentSelectedInvestigateButton(null);
            }
        }
    }
}
