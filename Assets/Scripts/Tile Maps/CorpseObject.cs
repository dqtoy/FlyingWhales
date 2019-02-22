using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CorpseObject : MonoBehaviour, IPointerClickHandler {

    private Corpse corpse;

    public void SetCorpse(Corpse corpse) {
        this.corpse = corpse;
    }

    public void OnPointerClick(PointerEventData eventData) {
        UIManager.Instance.ShowCharacterInfo(corpse.character);
    }
}
