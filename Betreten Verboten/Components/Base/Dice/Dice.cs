using System.Collections.Generic;

namespace Betreten_Verboten.Components.Base.Dice
{
    public static class Dice
    {
        public const int ENTITY_TAG = 15;

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

    }
}
