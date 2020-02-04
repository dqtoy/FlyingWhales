using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TraitEffectButton : MonoBehaviour {
    public Text buttonText;
    private TraitEffect _traitEffect;

    #region getters/setters
    public TraitEffect traitEffect {
        get { return _traitEffect; }
    }
    #endregion

    public void SetCurrentlySelectedButton() {
        TraitPanelUI.Instance.currentSelectedTraitEffectButton = this;
    }
    public void SetTraitEffect(TraitEffect traitEffect) {
        _traitEffect = traitEffect;
        if(_traitEffect != null) {
            buttonText.text = _traitEffect.description;
        }
    }
}
