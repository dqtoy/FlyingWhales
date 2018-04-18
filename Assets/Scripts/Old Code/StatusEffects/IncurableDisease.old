using UnityEngine;
using System.Collections;

public class IncurableDisease : StatusEffect {

    private string[] diseaseAdjectives = new string[] {
        "Red", "Green", "Yellow", "Black", "Rotting", "Silent", "Screaming", "Trembling", "Sleeping",
        "Cat", "Dog", "Pig", "Lamb", "Lizard", "Bog", "Death", "Stomach", "Eye", "Finger", "Rabid",
        "Fatal", "Blistering", "Icy", "Scaly", "Sexy", "Violent", "Necrotic", "Foul", "Vile", "Nasty",
        "Ghastly", "Malodorous", "Cave", "Phantom", "Wicked", "Strange"
    };

    private string[] diseases = new string[] {
        "Sores", "Ebola", "Anthrax", "Pox", "Face", "Sneeze", "Gangrene", "Throat", "Rash", "Warts",
        "Cholera", "Colds", "Ache", "Syndrome", "Tumor", "Chills", "Blisters", "Mouth", "Fever", "Delirium",
        "Measles", "Mutata", "Disease"
    };

    public IncurableDisease(Citizen owner) : base(owner) {
        this.name = GenerateDiseaseName();
        this.statusEffectType = CITIZEN_STATUS_EFFECTS.INCURABLE_DISEASE;
        ApplyEffects();
    }
    private string GenerateDiseaseName() {
        return diseaseAdjectives[Random.Range(0, diseaseAdjectives.Length)] + " " + diseases[Random.Range(0, diseases.Length)];
    }

    #region overrides
    internal override void ApplyEffects() {
        GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        dueDate.AddDays(4);
        SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => CheckStatusEffect());

        Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "StatusEffects", "IncurableDisease", "infected");
        newLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        newLog.AddToFillers(null, name, LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog);
    }

    internal override void CheckStatusEffect() {
        if (Random.Range(0, 100) < 3) {
            //Citizen dies of incurable disease
            owner.Death(DEATH_REASONS.INCURABLE_DISEASE);
            Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "StatusEffects", "IncurableDisease", "death");
            newLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            newLog.AddToFillers(null, name, LOG_IDENTIFIER.OTHER);
            UIManager.Instance.ShowNotification(newLog);
        } else {
            GameDate dueDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
            dueDate.AddDays(4);
            SchedulingManager.Instance.AddEntry(dueDate.month, dueDate.day, dueDate.year, () => CheckStatusEffect());
        }
    }
    #endregion
}
