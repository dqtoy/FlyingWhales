using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour {

    [Header("Minion")]
    [SerializeField] private CharacterPortrait minionPortrait;
    [SerializeField] private TextMeshProUGUI minionText;

    [Header("Choices")]
    [SerializeField] private RectTransform choicesParent;
    [SerializeField] private GameObject choicePrefab;
    [SerializeField] private ToggleGroup choiceToggleGroup;

    [Header("Other")]
    [SerializeField] private Button levelUpBtn;

    private System.Action<object, object> onClickOk; //object1 is object to replace and object2 is objectToAdd
    private System.Action<object> onClickCancel; //object is the rejected object

    private object selectedObj;
    private Minion minionToLevelUp;

    private List<System.Action> pendingReplaceActions = new List<System.Action>();

    //string identifierToLevelUp = this identifies what to level up for the particular minion, whether it's Combat Ability, Intervention Ability, Summon, or Artifact
    //if it is a Summon or Artifact, since it is not attached to a minion, load all player summons or artifacts
    public void ShowLevelUpUI(Minion minionToLevelUp, string identifierToLevelUp) {
        if (this.gameObject.activeInHierarchy) {
            pendingReplaceActions.Add(() => ShowLevelUpUI(minionToLevelUp, identifierToLevelUp));
            return;
        }
        //UIManager.Instance.Pause();
        Utilities.DestroyChildren(choicesParent);
        UpdateMinionToLevelUp(minionToLevelUp);

        List<object> choices = new List<object>();
        if(identifierToLevelUp.ToLower() == "combat ability") {
            choices.Add(minionToLevelUp.combatAbility);
        } else if (identifierToLevelUp.ToLower() == "intervention ability") {
            for (int i = 0; i < minionToLevelUp.interventionAbilities.Length; i++) {
                if(minionToLevelUp.interventionAbilities[i] != null) {
                    choices.Add(minionToLevelUp.interventionAbilities[i]);
                }
            }
        } else if (identifierToLevelUp.ToLower() == "summon") {
            List<Summon> summons = PlayerManager.Instance.player.GetAllSummons();
            for (int i = 0; i < summons.Count; i++) {
                choices.Add(summons[i]);
            }
        } else if (identifierToLevelUp.ToLower() == "artifact") {
            for (int i = 0; i < PlayerManager.Instance.player.artifacts.Length; i++) {
                if (PlayerManager.Instance.player.artifacts[i] != null) {
                    choices.Add(PlayerManager.Instance.player.artifacts[i]);
                }
            }
        }
        for (int i = 0; i < choices.Count; i++) {
            object currItem = choices[i];
            GameObject choiceGO = ObjectPoolManager.Instance.InstantiateObjectFromPool(choicePrefab.name, Vector3.zero, Quaternion.identity, choicesParent);
            LevelUpChoiceItem item = choiceGO.GetComponent<LevelUpChoiceItem>();
            item.toggle.group = choiceToggleGroup;
            item.SetObject(currItem, OnSelectChoice);
        }
        levelUpBtn.interactable = false;
        this.gameObject.SetActive(true);
    }

    private void UpdateMinionToLevelUp(Minion minion) {
        minionToLevelUp = minion;
        minionPortrait.GeneratePortrait(minionToLevelUp.character);
        string text = minionToLevelUp.character.name;
        text += "\nLvl. " + minionToLevelUp.character.level + " " + minionToLevelUp.character.raceClassName;
        minionText.text = text;
    }


    private void Close() {
        //UIManager.Instance.Unpause();
        this.gameObject.SetActive(false);
        if (pendingReplaceActions.Count > 0) {
            System.Action pending = pendingReplaceActions[0];
            pendingReplaceActions.RemoveAt(0);
            pending.Invoke();
        }
    }

    private void OnSelectChoice(object obj) {
        levelUpBtn.interactable = true;
        selectedObj = obj;
    }

    public void OnClickLevelUp() {
        if(selectedObj != null) {
            if(selectedObj is CombatAbility) {
                (selectedObj as CombatAbility).LevelUp();
            }else if (selectedObj is PlayerJobAction) {
                (selectedObj as PlayerJobAction).LevelUp();
            }else if (selectedObj is Summon) {
                (selectedObj as Summon).LevelUp();
            }else if (selectedObj is Artifact) {
                (selectedObj as Artifact).LevelUp();
            }
        }
        Close();
    }
    public void OnClickCancel() {
        Close();
    }
}
