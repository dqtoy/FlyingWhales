using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DualObjectPickerTab : PooledObject {

    [SerializeField] private Toggle toggle;
    [SerializeField] private TextMeshProUGUI tabLbl;

    public void Initialize(DualObjectPickerTabSetting settings, ToggleGroup group) {
        tabLbl.text = settings.name;
        toggle.onValueChanged.AddListener(settings.onToggleTabAction.Invoke);
        toggle.group = group;
        if (!group.allowSwitchOff && !group.AnyTogglesOn()) {
            toggle.isOn = true; //auto turn on tab if the toggle group it is in does not allow all off toggles and there are no toggles that are on in the given group.
        }
    }

    public override void Reset() {
        base.Reset();
        toggle.isOn = false;
        toggle.onValueChanged.RemoveAllListeners();
        tabLbl.text = string.Empty;
        toggle.group = null;
    }
}
