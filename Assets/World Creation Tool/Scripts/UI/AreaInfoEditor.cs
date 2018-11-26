using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AreaInfoEditor : MonoBehaviour {

    internal Area currentArea;

    [Header("Basic Info")]
    [SerializeField] private InputField areaNameField;

    public void Initialize() {
        
    }

    public void Show(Area area) {
        currentArea = area;
        this.gameObject.SetActive(true);
        LoadData();
    }
    public void Hide() {
        this.gameObject.SetActive(false);
    }

    public void LoadData() {
        areaNameField.text = currentArea.name;
    }

    #region Basic Info
    public void SetAreaName(string name) {
        currentArea.SetName(name);
    }
    #endregion

}
