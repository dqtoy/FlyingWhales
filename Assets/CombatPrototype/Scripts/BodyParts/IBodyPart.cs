using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class IBodyPart: EntityComponent {
		public enum IMPORTANCE {
			ESSENTIAL,
			NON_ESSENTIAL,
		}

		public enum ATTRIBUTE {
			CLAWED,
			CAN_PUNCH,
			CAN_GRIP,
            CAN_KICK,
            MAGICAL,
			CAN_SLASH,
			CAN_PIERCE,
			CAN_SHOOT,
            CAN_WHIP,
            CAN_EQUIP_TORSO_ARMOR,
            CAN_EQUIP_LEG_ARMOR,
            CAN_EQUIP_FOOT_ARMOR,
            CAN_EQUIP_ARM_ARMOR,
            CAN_EQUIP_HAND_ARMOR,
            CAN_EQUIP_HEAD_ARMOR,
            CAN_EQUIP_TAIL_ARMOR,
            NONE
        }

		[SerializeField] internal BODY_PART bodyPart;
		[SerializeField] internal IMPORTANCE importance;
		[SerializeField] internal List<BodyAttribute> attributes;
		internal List<STATUS_EFFECT> statusEffects = new List<STATUS_EFFECT>();
        internal List<Item> itemsAttached = new List<Item>();

        internal bool HasAttribute(ATTRIBUTE attribute) {
            for (int i = 0; i < attributes.Count; i++) {
                BodyAttribute currAttribute = attributes[i];
                if (currAttribute.attribute == attribute) {
                    return true;
                }
            }
            return false;
        }

        internal bool HasUnusedAttribute(ATTRIBUTE attribute) {
            for (int i = 0; i < attributes.Count; i++) {
                BodyAttribute currAttribute = attributes[i];
                if (currAttribute.attribute == attribute && !currAttribute.isUsed) {
                    return true;
                }
            }
            return false;
        }

        #region Items
        internal void AttachItem(Item item) {
            itemsAttached.Add(item);
            if(item is Weapon) {
                Weapon currItem = (Weapon)item;
                for (int i = 0; i < currItem.equipRequirements.Count; i++) {
                    ATTRIBUTE currAttribute = currItem.equipRequirements[i];
                    BodyAttribute attribute = GetAttribute(currAttribute);
                    attribute.SetAttributeAsUsed(true);
                }
            } else if (item is Armor) {
                Armor currItem = (Armor)item;
                ATTRIBUTE attribute = Utilities.GetNeededAttributeForArmor(currItem);
                BodyAttribute currAttribute = GetAttribute(attribute);
                currAttribute.SetAttributeAsUsed(true);
            }
        }
        internal void DettachItem(Item item) {
            itemsAttached.Remove(item);
            if (item is Weapon) {
                Weapon currItem = (Weapon)item;
                for (int i = 0; i < currItem.equipRequirements.Count; i++) {
                    ATTRIBUTE currAttribute = currItem.equipRequirements[i];
                    BodyAttribute attribute = GetAttribute(currAttribute);
                    attribute.SetAttributeAsUsed(false);
                }
            } else if (item is Armor) {
                Armor currItem = (Armor)item;
                ATTRIBUTE attribute = Utilities.GetNeededAttributeForArmor(currItem);
                BodyAttribute currAttribute = GetAttribute(attribute);
                currAttribute.SetAttributeAsUsed(false);
            }
        }
        internal List<Item> GetAttachedItemsOfType(ITEM_TYPE itemType) {
            List<Item> items = new List<Item>();
            for (int i = 0; i < itemsAttached.Count; i++) {
                Item currItem = itemsAttached[i];
                if(currItem.itemType == itemType) {
                    items.Add(currItem);
                }
            }
            return items;
        }
        #endregion

        #region Attributes
        private BodyAttribute GetAttribute(ATTRIBUTE attribute, bool isUsed = false) {
            for (int i = 0; i < attributes.Count; i++) {
                BodyAttribute currAttribute = attributes[i];
                if(currAttribute.attribute == attribute && currAttribute.isUsed == isUsed) {
                    return currAttribute;
                }
            }
            return null;
        }
        #endregion
    }
}

