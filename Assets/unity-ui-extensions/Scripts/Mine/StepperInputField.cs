using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StepperInputField : InputField {

	public void OnStepperValueChanged(int value) {
        this.text = value.ToString();
    }
}
