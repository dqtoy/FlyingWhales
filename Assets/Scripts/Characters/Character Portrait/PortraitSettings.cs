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
}
