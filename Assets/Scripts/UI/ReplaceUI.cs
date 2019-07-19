using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceUI : MonoBehaviour {

    [Header("Object To Add")]
    [SerializeField] private Image otaImage;
    [SerializeField] private TextMeshProUGUI otaText;

    [Header("Choices")]
    [SerializeField] private RectTransform choicesParent;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private ToggleGroup choiceToggleGroup;

    [Header("Other")]
    [SerializeField] private TextMeshProUGUI newObjectLbl;
    [SerializeField] private Button replaceBtn;

    private System.Action<object, object> onClickReplace; //object1 is object to replace and object2 is objectToAdd
    private System.Action<object> onClickCancel; //object is the rejected object

    private object selectedObj;
    private object objToAdd;

    private List<System.Action> pendingReplaceActions = new List<System.Action>();

    /// <summary>
    /// Show Replace Object UI.
    /// </summary>
    /// <typeparam name="T">The type of objects concerned</typeparam>
    /// <param name="choices">The objects that can be replaced.</param>
    /// <param name="objectToAdd">The object to add.</param>
    /// <param name="onClickReplace">What should happen when an object is replaced.</param>
    /// <param name="onClickCancel">What should happen when the offer was rejected.</param>
    public void ShowReplaceUI<T>(List<T> choices, T objectToAdd, System.Action<object, object> onClickReplace, System.Action<object> onClickCancel) {
        if (this.gameObject.activeInHierarchy) {
            pendingReplaceActions.Add(() => ShowReplaceUI(choices, objectToAdd, onClickReplace, onClickCancel));
            return;
        }
        UIManager.Instance.Pause();
        Utilities.DestroyChildren(choicesParent);
        newObjectLbl.text = "New " + Utilities.NormalizeNoSpaceString(objectToAdd.GetType().BaseType.ToString()) + "!";
        UpdateObjectToAdd(objectToAdd);
        for (int i = 0; i < choices.Count; i++) {
            T currItem = choices[i];
            GameObject choiceGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(choicePrefab.name, Vector3.zero, Quaternion.identity, choicesParent);
            ReplaceChoiceItem item = choiceGO.GetComponent<ReplaceChoiceItem>();
            item.toggle.group = choiceToggleGroup;
            item.SetObject(currItem, OnSelectChoice);
        }
        replaceBtn.interactable = false;
        this.onClickReplace = onClickReplace;
        this.onClickCancel = onClickCancel;
        this.gameObject.SetActive(true);
    }

    private void UpdateObjectToAdd(object obj) {
        objToAdd = obj;
        if (obj is Summon) {
            Summon summon = obj as Summon;
            otaImage.sprite = CharacterManager.Instance.GetSummonSettings(summon.summonType).summonPortrait;
            string text = summon.name + " (" + summon.summonType.SummonName() + ")";
            text += "\nLevel: " + summon.level.ToString();
            text += "\nDescription: " + PlayerManager.Instance.player.GetSummonDescription(summon.summonType);
            otaText.text = text;
        } else if (obj is Artifact) {
            Artifact artifact = obj as Artifact;
            string text = artifact.name;
            text += "\nLevel: " + artifact.level.ToString();
            text += "\nDescription: " + PlayerManager.Instance.player.GetArtifactDescription(artifact.type);
            otaText.text = text; 
            otaImage.sprite = CharacterManager.Instance.GetArtifactSettings(artifact.type).artifactPortrait;
        } else if (obj is PlayerJobAction) {
            PlayerJobAction action = obj as PlayerJobAction;
            string text = action.name;
            text += "\nDescription: " + PlayerManager.Instance.player.GetInterventionAbilityDescription(action);
            otaText.text = text;
            otaImage.sprite = PlayerManager.Instance.GetJobActionSprite(action.name);
        }
    }


    private void Close() {
        UIManager.Instance.Unpause();
        this.gameObject.SetActive(false);
        if (pendingReplaceActions.Count > 0) {
            System.Action pending = pendingReplaceActions[0];
            pendingReplaceActions.RemoveAt(0);
            pending.Invoke();
        }
    }

    private void OnSelectChoice(object obj) {
        replaceBtn.interactable = true;
        selectedObj = obj;
    }

    public void OnClickReplace() {
        onClickReplace.Invoke(selectedObj, objToAdd);
        Close();
    }
    public void OnClickCancel() {
        onClickCancel.Invoke(objToAdd);
        Close();
    }
}
