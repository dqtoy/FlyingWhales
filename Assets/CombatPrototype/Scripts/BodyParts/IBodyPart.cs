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
            CAN_WHIP
		}

		[SerializeField] internal BODY_PART bodyPart;
		[SerializeField] internal IMPORTANCE importance;
		[SerializeField] internal List<ATTRIBUTE> attributes;
		internal List<STATUS_EFFECT> statusEffects = new List<STATUS_EFFECT>();
		internal List<Item> itemsAttached = new List<Item>();
	}
}

