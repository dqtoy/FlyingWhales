using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EventChoiceItem : MonoBehaviour {

    public StoryEventChoice choice;

    [SerializeField] private TextMeshProUGUI choiceLbl;

    private System.Action<StoryEventChoice> onClickAction;

    public void SetChoice(StoryEventChoice choice, System.Action<StoryEventChoice> onClickAction) {
        this.choice = choice;
        choiceLbl.text = choice.text;
        this.onClickAction = onClickAction;
    }

    public void OnClick() {
        onClickAction.Invoke(choice); //NOTE: Should never have null on click action!
    }
}
