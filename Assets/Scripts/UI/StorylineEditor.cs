using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorylineEditor : MonoBehaviour {
    protected StorylineBtn _storylineBtn;

    #region Virtuals
    public virtual void PopulateData(StorylineBtn storylineBtn) {
        _storylineBtn = storylineBtn;
    }
    public virtual void SaveData() {}
    #endregion

    #region Button Clicks
    public void OnClickSave() {
        SaveData();
    }
    #endregion
}
