using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base.Dice
{
    public abstract class Dice : GeonComponent, IUpdatable
    {
        public const int ENTITY_TAG = 15;

        public abstract void Update();

        public static Vector3 GetCamPosOverride(DiceType type) => type == DiceType.Physics ? new Vector3(-470, 50, -470) : Vector3.Zero;
        public static Vector3 GetCamFocusOverride(DiceType type) => type == DiceType.Physics ? new Vector3(-495, 3, -495) : Vector3.Zero;

        /// <summary>
        /// Returns if another dice roll is necessary, depending on if the player can roll thrice.
        /// The dice should reroll if none of the following conditions is satisfied:
        /// 1. You can't roll thrice & you didn't roll a 6. (regular on-board roll)
        /// 2. You can roll thrice, your last roll was indeed a 6 and this one is not. (Positive roll-thrice exit condition)
        /// 3. You can roll thrice and already did, but even your last one isn't a 6. (Negative roll-thrice exit condition)
        /// </summary>
        /// <param name="nrs"></param>
        /// <param name="RollThrice"></param>
        /// <returns></returns>
        public static bool ShouldReroll(List<int> nrs, bool RollThrice) => !(!RollThrice && nrs[nrs.Count - 1] < 6 || RollThrice && nrs.Count > 1 && nrs[nrs.Count - 1] < 6 && nrs[nrs.Count - 2] >= 6 || RollThrice && nrs.Count >= 3 && nrs[nrs.Count - 1] < 6);

        public static void Throw(GeonScene scene, DiceType type)
        {
            switch (type)
            {
                case DiceType.Physics:
                    scene.CreateGeonEntity("dice", new Vector3(-500 + Random.MinusOneToOne() * 5, Random.Range(25, 40), -500 + Random.MinusOneToOne() * 5), NodeType.BoundingBoxCulling).SetTag(ENTITY_TAG).AddComponent(new PhysicsDice());
                    break;
                case DiceType.Simple:
                    break;
                default:
                    break;
            }
        }

        public enum DiceType
        {
            Physics,
            Simple
        }
    }
}
