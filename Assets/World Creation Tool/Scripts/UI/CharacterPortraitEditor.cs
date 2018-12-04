using BayatGames.SaveGameFree;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
    [SerializeField] private Stepper bodyStepper;
    [SerializeField] private Stepper facialHairStepper;
    [SerializeField] private ColorPickerControl hairColorPicker;
    [SerializeField] private Image hairColorImage;
    [SerializeField] private InputField fileNameField;
    [SerializeField] private Dropdown raceDropdown;
    [SerializeField] private Dropdown genderDropdown;

    private RACE chosenRace {
        get { return (RACE)System.Enum.Parse(typeof(RACE), raceDropdown.options[raceDropdown.value].text); }
    }
    private GENDER chosenGender {
        get { return (GENDER)System.Enum.Parse(typeof(GENDER), genderDropdown.options[genderDropdown.value].text); }
    }

    private void Start() {
        LoadDropdownSettings();
        if (portraitSettings == null) {
            portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(chosenRace, chosenGender);
        }
        SetStepperValues();
        UpdateVisuals();
        UpdatePortraitControls();
    }

    public void OnChangeRace(int choice) {
        portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(chosenRace, chosenGender);
        SetStepperValues();
        UpdateVisuals();
        UpdatePortraitControls();
    }
    public void OnChangeGender(int choice) {
        portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(chosenRace, chosenGender);
        SetStepperValues();
        UpdateVisuals();
        UpdatePortraitControls();
    }

    #region Portrait Editor
    private void UpdateVisuals() {
        portrait.GeneratePortrait(portraitSettings);
    }
    private void SetStepperValues() {
        headStepper.maximum = CharacterManager.Instance.GetHeadSpriteCount(chosenRace, chosenGender) - 1;
        hairStepper.maximum = CharacterManager.Instance.GetHairSpriteCount(chosenRace, chosenGender) - 1;
        eyesStepper.maximum = CharacterManager.Instance.GetEyeSpriteCount(chosenRace, chosenGender) - 1;
        noseStepper.maximum = CharacterManager.Instance.GetNoseSpriteCount(chosenRace, chosenGender) - 1;
        mouthStepper.maximum = CharacterManager.Instance.GetMouthSpriteCount(chosenRace, chosenGender) - 1;
        eyebrowsStepper.maximum = CharacterManager.Instance.GetEyebrowSpriteCount(chosenRace, chosenGender) - 1;
        bodyStepper.maximum = CharacterManager.Instance.GetBodySpriteCount(chosenRace, chosenGender) - 1;
        facialHairStepper.maximum = Mathf.Max(0, CharacterManager.Instance.GetFacialHairSpriteCount(chosenRace, chosenGender) - 1);
    }
    private void LoadDropdownSettings() {
        raceDropdown.ClearOptions();
        List<string> raceOptions = new List<string>();
        raceOptions.Add("HUMANS");
        raceOptions.Add("ELVES");
        raceDropdown.AddOptions(raceOptions);

        genderDropdown.ClearOptions();
        genderDropdown.AddOptions(Utilities.GetEnumChoices<GENDER>());
    }
    private void UpdatePortraitControls() {
        headStepper.SetStepperValue(portraitSettings.headIndex);
        hairStepper.SetStepperValue(portraitSettings.hairIndex);
        eyesStepper.SetStepperValue(portraitSettings.eyesIndex);
        noseStepper.SetStepperValue(portraitSettings.noseIndex);
        mouthStepper.SetStepperValue(portraitSettings.mouthIndex);
        eyebrowsStepper.SetStepperValue(portraitSettings.eyeBrowIndex);
        facialHairStepper.SetStepperValue(portraitSettings.facialHairIndex);
        bodyStepper.SetStepperValue(portraitSettings.bodyIndex);
        hairColorImage.color = portraitSettings.hairColor;
        hairColorPicker.CurrentColor = portraitSettings.hairColor;
    }
    private void UpdatePortraitControls(PortraitSettings settings) {
        raceDropdown.value = Utilities.GetOptionIndex(raceDropdown, settings.race.ToString());
        genderDropdown.value = Utilities.GetOptionIndex(genderDropdown, settings.gender.ToString());
        headStepper.SetStepperValue(settings.headIndex);
        hairStepper.SetStepperValue(settings.hairIndex);
        eyesStepper.SetStepperValue(settings.eyesIndex);
        noseStepper.SetStepperValue(settings.noseIndex);
        mouthStepper.SetStepperValue(settings.mouthIndex);
        eyebrowsStepper.SetStepperValue(settings.eyeBrowIndex);
        bodyStepper.SetStepperValue(settings.bodyIndex);
        facialHairStepper.SetStepperValue(settings.facialHairIndex);
        hairColorImage.color = settings.hairColor;
        hairColorPicker.CurrentColor = settings.hairColor;
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
    public void UpdateBody(int index) {
        portrait.SetBody(index);
        portraitSettings.bodyIndex = index;
    }
    public void UpdateFacialHair(int index) {
        portrait.SetFacialHair(index);
        portraitSettings.facialHairIndex = index;
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
        if (Utilities.DoesFileExist(Utilities.portraitsSavePath + chosenRace.ToString() + "/" + chosenGender + "/" + saveName)) {
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
        SaveGame.Save<PortraitSettings>(Utilities.portraitsSavePath  + chosenRace.ToString() + "/" + chosenGender + "/" + saveName, portraitSettings);
        worldcreator.WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Success", "Successfully saved template!");
        worldcreator.WorldCreatorUI.Instance.OnPortraitTemplatesChanged();
    }
    public void LoadPortrait() {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Choose template", Utilities.portraitsSavePath, Utilities.portraitFileExt.Remove(0, 1));
        if (path.Length != 0) {
            portraitSettings = SaveGame.Load<PortraitSettings>(path);
            UpdatePortraitControls(portraitSettings);
            UpdateVisuals();
        }
#endif
    }

    public void ShowMenu() {
        this.gameObject.SetActive(true);
    }
    public void HideMenu() {
        this.gameObject.SetActive(false);
    }
    #endregion
}
