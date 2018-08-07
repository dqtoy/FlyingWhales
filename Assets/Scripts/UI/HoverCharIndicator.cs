using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class HoverCharIndicator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public LandmarkVisual landmarkVisual;

    public void OnPointerEnter(PointerEventData eventData) {
        landmarkVisual.ShowCharactersInLandmark(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        landmarkVisual.ShowCharactersInLandmark(false);
    }
}
