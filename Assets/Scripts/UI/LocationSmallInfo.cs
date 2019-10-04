using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationSmallInfo : MonoBehaviour {

    public Region region { get; private set; }

    [SerializeField] private LocationPortrait portrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI typeLbl;
    [SerializeField] private TextMeshProUGUI subLbl;

    public void ShowRegionInfo(Region region, string subText = "") {
        this.region = region;
        nameLbl.text = region.name;
        //typeLbl.text = region.GetAreaTypeString();
        subLbl.text = subText;
        portrait.SetLocation(region);
        this.gameObject.SetActive(true);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }
}
