using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorylineBtn : MonoBehaviour {
    public Storyline storyline;
    public Text buttonText;

	public void OnClick() {
        EditStorylinesMenu.Instance.currentSelectedBtn = this;
    }
}
