using BayatGames.SaveGameFree;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class CharacterPortraitEditor : MonoBehaviour {

    private PortraitSettings portraitSettings;

    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private Stepper headStepper;
    [SerializeField] private Stepper hairStepper;
    [SerializeField] private Stepper eyesStepper;
    [SerializeField] private Stepper noseStepper;
    [SerializeField] private Stepper mouthStepper;
    [SerializeField] private Stepper eyebrowsStepper;
    [SerializeField] private ColorPickerControl hairColorPicker;
    [SerializeField] private Image hairColorImage;
    [SerializeField] private InputField fileNameField;

    private void Start() {
        if (portraitSettings == null) {
            portraitSettings = CharacterManager.Instance.GenerateRandomPortrait();
        }
        SetStepperValues();
        UpdateVisuals();
        UpdatePortraitControls();
    }

    #region Portrait Editor
    private void UpdateVisuals() {
        portrait.GeneratePortrait(portraitSettings);
    }
    private void SetStepperValues() {
        headStepper.maximum = CharacterManager.Instance.headSprites.Count - 1;
        hairStepper.maximum = CharacterManager.Instance.hairSettings.Count - 1;
        eyesStepper.maximum = CharacterManager.Instance.eyeSprites.Count - 1;
        noseStepper.maximum = CharacterManager.Instance.noseSprites.Count - 1;
        mouthStepper.maximum = CharacterManager.Instance.mouthSprites.Count - 1;
        eyebrowsStepper.maximum = CharacterManager.Instance.eyeBrowSprites.Count - 1;
    }
    private void UpdatePortraitControls() {
        headStepper.SetStepperValue(portraitSettings.headIndex);
        hairStepper.SetStepperValue(portraitSettings.hairIndex);
        eyesStepper.SetStepperValue(portraitSettings.eyesIndex);
        noseStepper.SetStepperValue(portraitSettings.noseIndex);
        mouthStepper.SetStepperValue(portraitSettings.mouthIndex);
        eyebrowsStepper.SetStepperValue(portraitSettings.eyeBrowIndex);
        hairColorImage.color = portraitSettings.hairColor;
        hairColorPicker.CurrentColor = portraitSettings.hairColor;
    }
    public void ShowHairColorPicker() {
        hairColorPicker.ShowMenu();
    }
    public void UpdateHair(int index) {
        portrait.SetHair(index);
        portraitSettings.hairIndex = index;
    }
    public void UpdateHead(int index) {
        portrait.SetHead(index);
        portraitSettings.headIndex = index;
    }
    public void UpdateEyes(int index) {
        portrait.SetEyes(index);
        portraitSettings.eyesIndex = index;
    }
    public void UpdateNose(int index) {
        portrait.SetNose(index);
        portraitSettings.noseIndex = index;
    }
    public void UpdateMouth(int index) {
        portrait.SetMouth(index);
        portraitSettings.mouthIndex = index;
    }
    public void UpdateEyebrows(int index) {
        portrait.SetEyebrows(index);
        portraitSettings.eyeBrowIndex = index;
    }
    public void UpdateHairColor(Color color) {
        portrait.SetHairColor(color);
        portraitSettings.hairColor = color;
    }
    public void OnClickSave() {
        string saveName = fileNameField.text;
        if (string.IsNullOrEmpty(saveName)) {
            worldcreator.WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Error", "Please enter a filename!");
            return;
        }
        if (!saveName.Contains(Utilities.portraitFileExt)) {
            saveName += Utilities.portraitFileExt;
        }
        if (Utilities.DoesFileExist(Utilities.portraitsSavePath + saveName)) {
            UnityAction yesAction = new UnityAction(() => SavePortrait());
            worldcreator.WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.YES_NO, "Overwrite Template", "Do you want to overwrite template file " + saveName + "?", yesAction);
        } else {
            SavePortrait();
        }
    }
    private void SavePortrait() {
        string saveName = fileNameField.text;
        if (!saveName.Contains(Utilities.portraitFileExt)) {
            saveName += Utilities.portraitFileExt;
        }
        SaveGame.Save<PortraitSettings>(Utilities.portraitsSavePath + saveName, portraitSettings);
        worldcreator.WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Success", "Successfully saved template!");
    }
    public void LoadPortrait() {
        string path = EditorUtility.OpenFilePanel("Choose template", Utilities.portraitsSavePath, Utilities.portraitFileExt.Remove(0, 1));
        if (path.Length != 0) {
            portraitSettings = SaveGame.Load<PortraitSettings>(path);
            UpdateVisuals();
            UpdatePortraitControls();
        }
    }

    public void ShowMenu() {
        this.gameObject.SetActive(true);
    }
    public void HideMenu() {
        this.gameObject.SetActive(false);
    }
    #endregion
}
