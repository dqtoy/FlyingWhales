using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmallCharacterInfo : MonoBehaviour {

    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private Text characterInfoLbl;

    public void ShowCharacterInfo(Character character) {
        portrait.GeneratePortrait(character, IMAGE_SIZE.X256, true);
        characterInfoLbl.text = string.Empty;
        characterInfoLbl.text += "Name: " + character.name;
        characterInfoLbl.text += "\nRace: " + character.raceSetting.race.ToString();
        characterInfoLbl.text += "\nGender: " + character.gender.ToString();
        characterInfoLbl.text += "\nRole: " + character.role.roleType.ToString();
        characterInfoLbl.text += "\nClass: " + character.characterClass.className;
        this.gameObject.SetActive(true);
    }
    public void HideSmallCharacterInfo() {
        this.gameObject.SetActive(false);
    }

    //private void Update() {
    //    if (this.gameObject.activeSelf) {
    //        this.transform.position = CameraMove.Instance.uiCamera.ScreenToWorldPoint(Input.mousePosition);
    //    }
    //}
}
