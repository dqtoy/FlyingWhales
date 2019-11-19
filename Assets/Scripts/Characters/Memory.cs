using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Memory {
    public GoapAction goapAction { get; private set; }
    public string text { get; private set; }
    public GameDate date { get; private set; }

	public Memory(GoapAction goapAction) {
        this.goapAction = goapAction;
        //this.date = this.goapAction.currentState.descriptionLog.date;
        this.date = GameManager.Instance.Today();
        SetText();
    }

    public void SetText() {
        //text = Utilities.LogReplacer(this.goapAction.currentState.descriptionLog);
    }
}
