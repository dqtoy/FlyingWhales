using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRelationshipItem : PooledObject {

    [SerializeField] private CharacterPortrait portrait;

    private AlterEgoData alterEgo;
    private IRelationshipData data;

    public void Initialize(AlterEgoData alterEgo, IRelationshipData data) {
        this.alterEgo = alterEgo;
        this.data = data;

        portrait.GeneratePortrait(alterEgo.owner, false);
    }

    public override void Reset() {
        base.Reset();
    }

    public void ShowRelationshipData() {
        string summary = $"Relationships with {alterEgo.owner.name} ({alterEgo.name})";
        summary += $"\nStatus: {data.relationshipStatus.ToString()}";
        for (int i = 0; i < data.relationships.Count; i++) {
            summary += $"\n- {data.relationships[i].ToString()}";
        }
        UIManager.Instance.ShowSmallInfo(summary);
    }
    public void HideSmallInfo() {
        UIManager.Instance.HideSmallInfo();
    }
}
