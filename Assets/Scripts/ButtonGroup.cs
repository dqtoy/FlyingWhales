using UnityEngine;
using System.Collections;

public class ButtonGroup : MonoBehaviour {

	public ButtonGroupItem[] buttons;

	public void ClickedItem(ButtonGroupItem itemClicked){
		for (int i = 0; i < buttons.Length; i++) {
			if (buttons[i] != itemClicked) {
				buttons[i].SetClickState(false);
            }
		}
	}
}
