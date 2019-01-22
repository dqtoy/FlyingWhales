using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortraitSettings {

    public RACE race;
    public GENDER gender;
    public int skinIndex;
    public int hairIndex;
    public int bodyIndex;
    public int underIndex;
    public int topIndex;
    //public Color32 hairColor;

    public PortraitSettings CreateNewCopy() {
        PortraitSettings newSettings = new PortraitSettings();
        newSettings.race = this.race;
        newSettings.gender = this.gender;
        newSettings.skinIndex = this.skinIndex;
        newSettings.hairIndex = this.hairIndex;
        newSettings.bodyIndex = this.bodyIndex;
        return newSettings;
    }
}
