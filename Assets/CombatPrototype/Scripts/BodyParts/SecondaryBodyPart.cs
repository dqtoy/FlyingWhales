using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SecondaryBodyPart: IBodyPart {

    #region Utilities
    internal SecondaryBodyPart CreateNewCopy() {
        SecondaryBodyPart newBodyPart = new SecondaryBodyPart();
//            newBodyPart.bodyPart = this.bodyPart;
		newBodyPart.name = this.name;
        newBodyPart.importance = this.importance;
        newBodyPart.attributes = new List<BodyAttribute>();
        for (int i = 0; i < this.attributes.Count; i++) {
            BodyAttribute originalAttribute = this.attributes[i];
            BodyAttribute newAttribute = new BodyAttribute();
            newAttribute.attribute = originalAttribute.attribute;
            newAttribute.SetAttributeAsUsed(originalAttribute.isUsed);
            newBodyPart.attributes.Add(newAttribute);
        }
        newBodyPart.statusEffects = new List<STATUS_EFFECT>(this.statusEffects);
        newBodyPart.itemsAttached = new List<Item>();
        return newBodyPart;
    }
    #endregion
}

