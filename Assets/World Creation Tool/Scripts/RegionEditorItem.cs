using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionEditorItem : MonoBehaviour {

    [SerializeField] private Text regionLbl;
    [SerializeField] private Text tilesLbl;

    private Region _region;

    public void SetRegion(Region region) {
        _region = region;
    }
}
