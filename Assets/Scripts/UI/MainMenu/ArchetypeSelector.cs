using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class ArchetypeSelector : MonoBehaviour {

    [SerializeField] private HorizontalScrollSnap _horizontalScrollSnap;
    [SerializeField] private Button continueBtn;

    public void Show() {
        this.gameObject.SetActive(true);
        _horizontalScrollSnap.GoToScreen(0);
    }

    public void Hide() {
        this.gameObject.SetActive(false);
    }

    public void OnClickContinue() {
        // Hide();
        continueBtn.interactable = false;
        MainMenuUI.Instance.StartNewGame();
    }
}
