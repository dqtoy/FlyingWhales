using UnityEngine;
using System.Collections;

public class StatusEffect {

    public Citizen owner;
    public string name;
    public STATUS_EFFECTS statusEffectType;
    public GameDate contractionDate;

    public StatusEffect(Citizen owner) {
        this.owner = owner;
        this.contractionDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
    }

    #region Overrides
    internal virtual void ApplyEffects() {}
    internal virtual void CheckStatusEffect() { }
    #endregion
}
