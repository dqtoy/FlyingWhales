using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterRelationshipItem : PooledObject {

    public Relationship rel { get; private set; }
    private Color relStatItemBGColor;

    [SerializeField] private Image bg;
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private TextMeshProUGUI characterNameLbl;
    [SerializeField] private AffiliationsObject affiliations;
    [SerializeField] private ScrollRect relationshipStatusScrollView;
    [SerializeField] private GameObject relStatItemPrefab;

    public void Initialize() {
        affiliations.Initialize();
    }
    public override void Reset() {
        base.Reset();
        affiliations.Reset();
        rel = null;
    }

    public void SetRelationship(Relationship rel) {
        this.rel = rel;
        UpdateInfo();
    }
    public void SetBGColor(Color color, Color relStatColor) {
        bg.color = color;
        relStatItemBGColor = relStatColor;
    }

    public void UpdateInfo() {
        //portrait.SetDimensions(42f);
        portrait.GeneratePortrait(rel.targetCharacter);
        characterNameLbl.text = rel.targetCharacter.name;
        affiliations.SetCharacter(rel.targetCharacter);
        affiliations.UpdateAffiliations();
        LoadRelationshipStatuses();
    }

    private void LoadRelationshipStatuses() {
        RelationshipStatusItem[] items = Utilities.GetComponentsInDirectChildren<RelationshipStatusItem>(relationshipStatusScrollView.content.gameObject);
        for (int i = 0; i < items.Length; i++) {
            ObjectPoolManager.Instance.DestroyObject(items[i].gameObject);
        }
        for (int i = 0; i < rel.relationshipStatuses.Count; i++) {
            CHARACTER_RELATIONSHIP relStat = rel.relationshipStatuses[i];
            GameObject newRelStatGO = UIManager.Instance.InstantiateUIObject(relStatItemPrefab.name, relationshipStatusScrollView.content);
            RelationshipStatusItem item = newRelStatGO.GetComponent<RelationshipStatusItem>();
            item.SetStatus(relStat);
            item.SetBGColor(relStatItemBGColor);
        }
        
    }
}
