using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(Selectable))]
public class StepperSide : UIBehaviour, IPointerClickHandler, ISubmitHandler {
    Selectable button { get { return GetComponent<Selectable>(); } }

    Stepper stepper { get { return GetComponentInParent<Stepper>(); } }

    bool leftmost { get { return button == stepper.sides[0]; } }

    protected StepperSide() { }

    public virtual void OnPointerClick(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Press();
    }

    public virtual void OnSubmit(BaseEventData eventData) {
        Press();
    }

    private void Press() {
        if (!button.IsActive() || !button.IsInteractable())
            return;

        if (leftmost) {
            stepper.StepDown();
        } else {
            stepper.StepUp();
        }
    }
}