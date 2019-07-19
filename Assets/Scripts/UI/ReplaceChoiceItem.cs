using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceChoiceItem : PooledObject {

    private object obj;

    [SerializeField] private Image img;
    [SerializeField] private TextMeshProUGUI info;
    public Toggle toggle;

    private System.Action<object> onSelected;

    public void SetObject(object obj, System.Action<object> onSelected) {
        this.obj = obj;
        this.onSelected = onSelected;
        if (obj is Summon) {
            img.sprite = CharacterManager.Instance.GetSummonSettings((obj as Summon).summonType).summonPortrait;
        } else if (obj is Artifact) {
            img.sprite = CharacterManager.Instance.GetArtifactSettings((obj as Artifact).type).artifactPortrait;
        } else if (obj is PlayerJobAction) {
            img.sprite = PlayerManager.Instance.GetJobActionSprite((obj as PlayerJobAction).name);
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
            text += "\nDescription: " + PlayerManager.Instance.player.GetInterventionAbilityDescription(action);
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
