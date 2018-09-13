using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BehaviorBtn : MonoBehaviour {
    public Text buttonText;

    public void OnClickBehavior() {
        AttributePanelUI.Instance.SetCurrentSelectBehaviorBtn(this);
    }
	
}
