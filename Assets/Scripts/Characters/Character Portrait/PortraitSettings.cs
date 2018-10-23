using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortraitSettings {

    public RACE race;
    public GENDER gender;
    public int headIndex;
    public int eyesIndex;
    public int eyeBrowIndex;
    public int hairIndex;
    public int noseIndex;
    public int mouthIndex;
    public int bodyIndex;
    public int facialHairIndex = 5; //blank
    public Color32 hairColor;

    public PortraitSettings CreateNewCopy() {
        PortraitSettings newSettings = new PortraitSettings();
        newSettings.race = this.race;
        newSettings.gender = this.gender;
        newSettings.headIndex = this.headIndex;
        newSettings.eyesIndex = this.eyesIndex;
        newSettings.eyeBrowIndex = this.eyeBrowIndex;
        newSettings.hairIndex = this.hairIndex;
        newSettings.noseIndex = this.noseIndex;
        newSettings.mouthIndex = this.mouthIndex;
        newSettings.bodyIndex = this.bodyIndex;
        newSettings.facialHairIndex = this.facialHairIndex;
        newSettings.hairColor = this.hairColor;
        return newSettings;
    }
}
