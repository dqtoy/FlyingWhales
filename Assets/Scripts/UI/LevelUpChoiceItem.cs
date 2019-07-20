using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpChoiceItem : PooledObject {

    private object obj;

    [SerializeField] private Image img;
    [SerializeField] private TextMeshProUGUI info;
    public Toggle toggle;

    private System.Action<object> onSelected;

    public void SetObject(object obj, System.Action<object> onSelected) {
        this.obj = obj;
        this.onSelected = onSelected;
        if (obj is Summon) {
            Summon summon = obj as Summon;
            toggle.interactable = summon.level < PlayerManager.MAX_LEVEL_SUMMON;
            img.sprite = CharacterManager.Instance.GetSummonSettings(summon.summonType).summonPortrait;
        } else if (obj is Artifact) {
            Artifact artifact = obj as Artifact;
            toggle.interactable = artifact.level < PlayerManager.MAX_LEVEL_ARTIFACT;
            img.sprite = CharacterManager.Instance.GetArtifactSettings(artifact.type).artifactPortrait;
        } else if (obj is PlayerJobAction) {
            PlayerJobAction interventionAbility = obj as PlayerJobAction;
            toggle.interactable = interventionAbility.lvl < PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY;
            img.sprite = PlayerManager.Instance.GetJobActionSprite(interventionAbility.name);
        } else if (obj is CombatAbility) {
            CombatAbility combatAbility = obj as CombatAbility;
            toggle.interactable = combatAbility.lvl < PlayerManager.MAX_LEVEL_COMBAT_ABILITY;
            img.sprite = PlayerManager.Instance.GetCombatAbilitySprite(combatAbility.name);
        }
        UpdateTextInfo();
    }

    public void OnClick(bool selected) {
        if (selected) {
            onSelected.Invoke(obj);
        }
    }

    private void UpdateTextInfo() {
        if (obj is Summon) {
            Summon summon = obj as Summon;
            string text = summon.name + " (" + summon.summonType.SummonName() + ")";
            text += "\nLevel: " + summon.level.ToString();
            text += "\nDescription: " + PlayerManager.Instance.player.GetSummonDescription(summon.summonType);
            info.text = text;
        } else if (obj is Artifact) {
            Artifact artifact = obj as Artifact;
            string text = artifact.name;
            text += "\nLevel: " + artifact.level.ToString();
            text += "\nDescription: " + PlayerManager.Instance.player.GetArtifactDescription(artifact.type);
            info.text = text;
        } else if (obj is PlayerJobAction) {
            PlayerJobAction action = obj as PlayerJobAction;
            string text = action.name;
            text += "\nDescription: " + action.description;
            info.text = text;
        } else if (obj is CombatAbility) {
            CombatAbility ability = obj as CombatAbility;
            string text = ability.name;
            text += "\nDescription: " + ability.description;
            info.text = text;
        }
    }

    public override void Reset() {
        base.Reset();
        obj = null;
        toggle.group = null;
        toggle.isOn = false;
    }
}
