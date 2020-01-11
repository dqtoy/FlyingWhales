using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;


public class HextileInfoUI : UIMenu {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private LocationPortrait _locationPortrait;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI tileTypeLbl;
    
    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI featuresLbl;
    
    public HexTile currentlyShowingHexTile { get; private set; }
    
    public override void OpenMenu() {
        base.OpenMenu();
        currentlyShowingHexTile = _data as HexTile;
        UpdateBasicInfo();
        UpdateHexTileInfo();
    }
    public override void SetData(object data) {
        base.SetData(data); //replace this existing data
        if (isShowing) {
            UpdateHexTileInfo();
        }
    }

    private void UpdateBasicInfo() {
        _locationPortrait.SetPortrait(currentlyShowingHexTile.landmarkOnTile.specificLandmarkType);
        nameLbl.text = currentlyShowingHexTile.landmarkOnTile.landmarkName;
        tileTypeLbl.text =
            Utilities.NormalizeStringUpperCaseFirstLetters(currentlyShowingHexTile.landmarkOnTile.specificLandmarkType.ToString());
    }
    
    public void UpdateHexTileInfo() {
        featuresLbl.text = string.Empty;
        if (currentlyShowingHexTile.featureComponent.features.Count == 0) {
            featuresLbl.text = $"{featuresLbl.text}None";
        } else {
            for (int i = 0; i < currentlyShowingHexTile.featureComponent.features.Count; i++) {
                TileFeature feature = currentlyShowingHexTile.featureComponent.features[i];
                if (i != 0) {
                    featuresLbl.text = $"{featuresLbl.text}, ";
                }
                featuresLbl.text = $"{featuresLbl.text}<link=\"{i}\">{feature.name}</link>";
            }
        }
    }
    
    public void OnHoverFeature(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            UIManager.Instance.ShowSmallInfo(currentlyShowingHexTile.featureComponent.features[index].description);
        }
    }
    public void OnHoverExitFeature() {
        UIManager.Instance.HideSmallInfo();
    }

}
