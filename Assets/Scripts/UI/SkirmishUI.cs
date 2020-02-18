using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkirmishUI : MonoBehaviour {
    [Header("General")]
    public Button fightBtn;
    public Button okBtn;
    public TextMeshProUGUI resultText;

    [Header("Character 1")]
    [SerializeField] private CharacterPortrait char1Portrait;
    [SerializeField] private TextMeshProUGUI char1Text;
    [SerializeField] private TextMeshProUGUI char1WinRateText;

    [Header("Character 2")]
    [SerializeField] private CharacterPortrait char2Portrait;
    [SerializeField] private TextMeshProUGUI char2Text;
    [SerializeField] private TextMeshProUGUI char2WinRateText;

    public Character char1 { get; private set; }
    public Character char2 { get; private set; }

    public float char1WinRate { get; private set; }
    public float char2WinRate { get; private set; }

    public void ShowSkirmishUI(Character char1, Character char2) {
        this.char1 = char1;
        this.char2 = char2;

        UpdateChar1();
        UpdateChar2();

        float char1WinRate = 0f;
        float char2WinRate = 0f;

        CombatManager.Instance.GetCombatChanceOfTwoLists(new List<Character>() { this.char1 }, new List<Character>() { this.char2 }, out char1WinRate, out char2WinRate);

        this.char1WinRate = char1WinRate;
        this.char2WinRate = char2WinRate;

        char1WinRateText.text = $"{this.char1WinRate}%";
        char2WinRateText.text = $"{this.char2WinRate}%";

        fightBtn.gameObject.SetActive(true);
        okBtn.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);

        this.gameObject.SetActive(true);
    }

    private void UpdateChar1() {
        char1Portrait.GeneratePortrait(char1);
        string text = char1.name;
        text += $"\nLvl.{char1.level} {char1.raceClassName}";
        char1Text.text = text;
    }
    private void UpdateChar2() {
        char2Portrait.GeneratePortrait(char2);
        string text = char2.name;
        text += $"\nLvl.{char2.level} {char2.raceClassName}";
        char2Text.text = text;
    }
    private void Results() {
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < char1WinRate) {
            //You won
            resultText.text = $"You won! {char1.name} gains a level!";
            char1.LevelUp();
        } else {
            //You lost
            resultText.text = $"You lost! {char1.name} is injured!";
            if(char1.minion != null) {
                char1.minion.AddTrait("Injured", char2);
            } else {
                char1.traitContainer.AddTrait(char1, "Injured", char2);
            }
        }
        resultText.gameObject.SetActive(true);
        fightBtn.gameObject.SetActive(false);
        okBtn.gameObject.SetActive(true);
    }
    private void Close() {
        this.gameObject.SetActive(false);
    }

    public void OnClickFight() {
        Results();
    }
    public void OnClickClose() {
        Close();
    }
    public void OnClickOk() {
        Close();
    }
}
