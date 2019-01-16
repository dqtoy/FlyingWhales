using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationPortrait : MonoBehaviour {

    private Area area;

    [SerializeField] private Image portrait;

    public void SetLocation(Area area) {
        this.area = area;
        portrait.sprite = LandmarkManager.Instance.locationPortraits[area.name];
    }
}
