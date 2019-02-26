using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogItem : MonoBehaviour {

    public enum Position { Left, Right }

    [SerializeField] private RectTransform portraitRT;
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private TextMeshProUGUI characterDialog;
    [SerializeField] private RectTransform characterDialogParent;

    [SerializeField] private Vector2 leftPortraitPos;
    [SerializeField] private Vector2 rightPortraitPos;
    [SerializeField] private LayoutGroup characterDialogLayoutGroup;

    public void SetData(Character character, string text, Position position = Position.Left) {
        portrait.GeneratePortrait(character);
        characterDialog.text = text;
        AlignToPosition(position);
    }
    private void AlignToPosition(Position position) {
        if (position == Position.Left) {
            portraitRT.anchoredPosition = leftPortraitPos;
            characterDialogLayoutGroup.childAlignment = TextAnchor.UpperLeft;
            characterDialog.alignment = TextAlignmentOptions.MidlineLeft;
        } else if (position == Position.Right) {
            portraitRT.anchoredPosition = rightPortraitPos;
            characterDialogLayoutGroup.childAlignment = TextAnchor.UpperRight;
            characterDialog.alignment = TextAlignmentOptions.MidlineRight;
        }
    }

    [ContextMenu("Alignment test")]
    public void AlignmentTest() {
        AlignToPosition(Position.Left);
    }
}
