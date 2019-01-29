using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationSmallInfo : MonoBehaviour {

    public Area area { get; private set; }

    [SerializeField] private LocationPortrait portrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI typeLbl;
    [SerializeField] private TextMeshProUGUI subLbl;

    public void ShowAreaInfo(Area area, string subText = "") {
        this.area = area;
        nameLbl.text = area.name;
        typeLbl.text = area.GetAreaTypeString();
        subLbl.text = subText;
        portrait.SetLocation(area);
        this.gameObject.SetActive(true);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }
}
