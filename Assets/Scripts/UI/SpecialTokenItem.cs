using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpecialTokenItem : MonoBehaviour, IDragParentItem {

    public SpecialToken token;

    [SerializeField] private TextMeshProUGUI tokenNameLbl;
    public GameObject specialTokenVisual;

    public object associatedObj {
        get { return token; }
    }

    public void SetToken(SpecialToken token) {
        this.token = token;
        tokenNameLbl.text = token.name;
    }

}
