using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegionInfoUI : UIMenu {

    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI regionNameLbl;
    [SerializeField] private TextMeshProUGUI regionTypeLbl;

    [Header("Main")]
    [SerializeField] private TextMeshProUGUI worldObjLbl;
    [SerializeField] private Button invadeBtn;

    private Region activeRegion;

    public override void OpenMenu() {
        base.OpenMenu();
        activeRegion = _data as Region;
    }

    #region Basic Info
    private void UpdateBasicInfo() {

    }
    #endregion

}
