using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleTextColorizer : MonoBehaviour {

    private Color onColor = new Color(73f/255f, 93f/255f, 107f/255f, 255f/255f);
    private Color offColor = new Color(247f/255f, 238f/255f, 212f/255f, 255f/255f);
    [SerializeField] private TextMeshProUGUI targetText;

    public void OnValueChange(bool isOn) {
        if (isOn) {
            targetText.color = onColor;
        } else {
            targetText.color = offColor;
        }
    }
}
