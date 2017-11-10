using UnityEngine;
using System.Collections;

public class KingdomEmblem : MonoBehaviour {

    private Kingdom _kingdom;

    [SerializeField] private UI2DSprite _emblemBG;
    [SerializeField] private UI2DSprite _emblemSprite;
    [SerializeField] private UI2DSprite _emblemOutline;

    internal void SetKingdom(Kingdom kingdom) {
        _kingdom = kingdom;

        _emblemBG.sprite2D = kingdom.emblemBG;
        _emblemSprite.sprite2D = kingdom.emblem;
        _emblemOutline.sprite2D = kingdom.emblemBG;
    }
}
