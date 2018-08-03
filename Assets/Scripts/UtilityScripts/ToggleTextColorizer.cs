using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleTextColorizer : MonoBehaviour {

    [SerializeField] private Color onColor;
    [SerializeField] private Color offColor;
    [SerializeField] private TextMeshProUGUI targetText;

    public void OnValueChange(bool isOn) {
        if (isOn) {
            targetText.color = onColor;
        } else {
            targetText.color = offColor;
        }
    }
}
