using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewAbilityUI : MonoBehaviour {
    [Header("General")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI obtainText;

    [Header("Minion")]
    [SerializeField] private CharacterPortrait minionPortrait;
    [SerializeField] private TextMeshProUGUI minionText;
    [SerializeField] private GameObject minionGO;

    [Header("New Ability")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI abilityText;

    private Minion minion;

    private List<System.Action> pendingReplaceActions = new List<System.Action>();

    //string identifierToLevelUp = this identifies what to level up for the particular minion, whether it's Combat Ability, Intervention Ability, Summon, or Artifact
    //if it is a Summon or Artifact, since it is not attached to a minion, load all player summons or artifacts
    public void ShowNewAbilityUI(Minion minionToLevelUp, object ability) {
        if (this.gameObject.activeInHierarchy) {
            pendingReplaceActions.Add(() => ShowNewAbilityUI(minionToLevelUp, ability));
            return;
        }
        //UIManager.Instance.Pause();
        UpdateMinionToLevelUp(minionToLevelUp);
        UpdateNewAbility(ability);
        this.gameObject.SetActive(true);
    }

    private void UpdateMinionToLevelUp(Minion minion) {
        this.minion = minion;
        minionPortrait.GeneratePortrait(this.minion.character);
        string text = this.minion.character.name;
        text += "\nLvl. " + this.minion.character.level + " " + this.minion.character.raceClassName;
        minionText.text = text;
    }

    public void UpdateNewAbility(object obj) {
        obtainText.gameObject.SetActive(false);
        minionGO.SetActive(false);
        if (obj is PlayerJobAction) {
            titleText.text = "New Intervention Ability";
            PlayerJobAction action = obj as PlayerJobAction;
            abilityIcon.sprite = PlayerManager.Instance.GetJobActionSprite(action.name);
            string text = action.name;
            text += "\nLevel: " + action.lvl;
            text += "\nDescription: " + action.description;
            abilityText.text = text;
            minionGO.SetActive(true);
        } else if (obj is CombatAbility) {
            titleText.text = "New Combat Ability";
            CombatAbility ability = obj as CombatAbility;
            abilityIcon.sprite = PlayerManager.Instance.GetCombatAbilitySprite(ability.name);
            string text = ability.name;
            text += "\nLevel: " + ability.lvl;
            text += "\nDescription: " + ability.description;
            abilityText.text = text;
            minionGO.SetActive(true);
        } else if (obj is Summon) {
            titleText.text = "New Summon";
            obtainText.text = "You obtained a new Summon!";
            Summon summon = obj as Summon;
            abilityIcon.sprite = CharacterManager.Instance.GetSummonSettings(summon.summonType).summonPortrait;
            string text = summon.name + " (" + summon.summonType.SummonName() + ")";
            text += "\nLevel: " + summon.level.ToString();
            text += "\nDescription: " + PlayerManager.Instance.player.GetSummonDescription(summon.summonType);
            abilityText.text = text;
            obtainText.gameObject.SetActive(true);
        } else if (obj is Artifact) {
            titleText.text = "New Artifact";
            obtainText.text = "You obtained a new Artifact!";
            Artifact artifact = obj as Artifact;
            abilityIcon.sprite = CharacterManager.Instance.GetArtifactSettings(artifact.type).artifactPortrait;
            string text = artifact.name;
            text += "\nLevel: " + artifact.level;
            text += "\nDescription: " + PlayerManager.Instance.player.GetArtifactDescription(artifact.type);
            abilityText.text = text;
            obtainText.gameObject.SetActive(true);
        }
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

    public void OnClickOk() {
        Close();
    }
}
