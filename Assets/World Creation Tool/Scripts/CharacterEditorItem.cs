using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterEditorItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private ECS.Character _character;

    [SerializeField] private Text characterNameLbl;
    [SerializeField] private Button editBtn;

    #region getters/setters
    public ECS.Character character {
        get { return _character; }
    }
    #endregion

    public void SetCharacter(ECS.Character character) {
        _character = character;
        UpdateInfo();
    }

    private void UpdateInfo() {
        characterNameLbl.text = _character.name;
    }
    public void SetEditAction(UnityAction action) {
        editBtn.onClick.RemoveAllListeners();
        editBtn.onClick.AddListener(action);
    }
    public void DeleteCharacter() {
        CharacterManager.Instance.RemoveCharacter(_character);
    }

    #region Events
    public void OnPointerEnter(PointerEventData eventData) {
        worldcreator.WorldCreatorUI.Instance.ShowSmallCharacterInfo(_character);
    }
    public void OnPointerExit(PointerEventData eventData) {
        worldcreator.WorldCreatorUI.Instance.HideSmallCharacterInfo();
    }
    #endregion
}
