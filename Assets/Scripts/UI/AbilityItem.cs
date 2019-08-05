﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityItem : MonoBehaviour {

    public object ability { get; private set; }
    public Image img;

    public void SetAbility(object ability) {
        this.ability = ability;
        if(this.ability != null) {
            if(this.ability is CombatAbility) {
                img.sprite = PlayerManager.Instance.GetCombatAbilitySprite((this.ability as CombatAbility).name);
            }else if (this.ability is PlayerJobAction) {
                img.sprite = PlayerManager.Instance.GetJobActionSprite((this.ability as PlayerJobAction).name);
            }
        }
    }

    public void OnHoverThis() {
        if (this.ability != null) {
            if (this.ability is CombatAbility) {
                UIManager.Instance.ShowSmallInfo((this.ability as CombatAbility).name);
            } else if (this.ability is PlayerJobAction) {
                UIManager.Instance.ShowSmallInfo((this.ability as PlayerJobAction).name);
            }
        }
    }

    public void OnHoverOut() {
        UIManager.Instance.HideSmallInfo();
    }
}