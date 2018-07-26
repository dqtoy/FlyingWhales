using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelationshipStatusItem : PooledObject {

    [SerializeField] private Image bg;
    [SerializeField] private TextMeshProUGUI statusLbl;
    [SerializeField] private EnvelopContentUnityUI envelopContent;

    public void SetStatus(CHARACTER_RELATIONSHIP rel) {
        statusLbl.text = Utilities.NormalizeStringUpperCaseFirstLetters(rel.ToString());
        envelopContent.Execute();
    }
    public void SetBGColor(Color color) {
        bg.color = color;
    }
}
