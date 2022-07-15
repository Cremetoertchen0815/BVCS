using Betreten_Verboten.Components.Base.Characters;
using Betreten_Verboten.Components.BV.Player;
using Nez;

namespace Betreten_Verboten.Components.BV
{
    public static class God
    {
        public const int POINTS_FOR_SACRIFICING = 25;
        public static void Sacrifice(Character character)
        {
            //Prepare and kick character
            if (!(character.Owner is BVPlayer p && p.Sacrificable)) throw new System.Exception("Player has to be a BV player!");
            p.Sacrificable = false;
            p.AdditionalPoints += POINTS_FOR_SACRIFICING;
            character.Kick(null);

            //TODO: Display message like "xxx offered one of his pieces to the gods..."
            character.SendPrivateTele("base", "show_action_result", p.CharacterConfig.Name + " offered one of his pieces to the gods...");

            float progress = character.Position / p.Board.FieldCountTotal; //Captures the total progress from home to house of a figure in a normalized space.
            float pogfactor = progress * 0.55f + 0.15f; //Result gets positive when the random normalizes float lies under this limit.
            float flopfactor = pogfactor + 0.15f; //Result gets negative when the random normalizes float lies above this limit.

            Core.Schedule(2f, x =>
            {
                //Choose type of sacrifice
                float RNG = Random.NextFloat();
                if (RNG < pogfactor) SacrificePositive(character);
                else if (RNG > flopfactor) SacrificeNegative(character);
                else SacrificeNeutral(character);
            });
        }

        private static void SacrificePositive(Character c)
        {
            bool loopActive = true;
            while (loopActive)
            {
                try
                {
                    //Pick action type
                    switch (Random.Range(0, 4))
                    {
                        case 0: //Boost ally
                            var fig = GetRandomAllyFigure(c);
                            int boost = Random.Range(0, System.Math.Max(fig.Owner.Board.FieldCountTotal - fig.Position, 0));
                            if (fig == null || c.Owner.IsFieldBlocked(fig.Position + boost, out int _)) break;
                            fig.AdvanceSteps(boost);
                            c.SendPrivateTele("base", "show_action_result", "You're lucky! Your figure is being boosted!");
                            loopActive = false;
                            break;
                        case 1: //Kick random enemy
                            fig = GetRandomEnemyFigure(c);
                            if (fig == null || fig.Position >= fig.Owner.Board.FieldCountTotal) break;
                            fig.Kick(null);
                            c.SendPrivateTele("base", "show_action_result", "You're lucky! A random enemy figure got kicked!");
                            c.SendPrivateTele("base", "char_move_done", null);
                            loopActive = false;
                            break;
                        case 2: //Add anger button
                            ((BVPlayer)c.Owner).AngerCount++;
                            c.SendPrivateTele("base", "show_action_result", "You're lucky! You got another anger button!");
                            c.SendPrivateTele("base", "char_move_done", null);
                            loopActive = false;
                            break;
                        case 3: //Add points
                            ((BVPlayer)c.Owner).AdditionalPoints += 75;
                            c.SendPrivateTele("base", "show_action_result", "You're lucky! You gained 75 points!");
                            c.SendPrivateTele("base", "char_move_done", null);
                            loopActive = false;
                            break;
                    }

                }
                catch (System.Exception)
                { }
            }
        }
        private static void SacrificeNeutral(Character c)
        {
            c.SendPrivateTele("base", "show_action_result", "You got spared.");
            c.SendPrivateTele("base", "char_move_done", null);
        }
        private static void SacrificeNegative(Character c)
        {
            bool loopActive = true;
            while (loopActive)
            {
                try
                {
                    //Pick action type
                    switch (Random.Range(0, 4))
                    {
                        case 0: //Kick ally
                            var fig = GetRandomAllyFigure(c);
                            int boost = Random.Range(0, System.Math.Max(fig.Owner.Board.FieldCountTotal - fig.Position, 0));
                            if (fig == null || c.Owner.IsFieldBlocked(fig.Position + boost, out int _)) break;
                            fig.Kick(null);
                            c.SendPrivateTele("base", "show_action_result", "You're lucky! Your figure is being boosted!");
                            loopActive = false;
                            break;
                        case 1: //Skip turn
                            ((BVPlayer)c.Owner).SkipRound = true;
                            c.SendPrivateTele("base", "show_action_result", "You're lucky! A random enemy figure got kicked!");
                            c.SendPrivateTele("base", "char_move_done", null);
                            loopActive = false;
                            break;
                        case 2: //Remove anger button
                            var owner = (BVPlayer)c.Owner;
                            owner.AngerCount = System.Math.Max(0, ++owner.AngerCount);
                            c.SendPrivateTele("base", "show_action_result", "Oh ooh! You've lost an anger button!");
                            c.SendPrivateTele("base", "char_move_done", null);
                            loopActive = false;
                            break;
                        case 3: //Subtract points
                            ((BVPlayer)c.Owner).AdditionalPoints -= 75;
                            c.SendPrivateTele("base", "show_action_result", "Oh ooh! You lost 75 points!");
                            c.SendPrivateTele("base", "char_move_done", null);
                            loopActive = false;
                            break;
                    }

                }
                catch (System.Exception)
                { }
            }
        }

        //Helper methods
        private static Character GetRandomEnemyFigure(Character c) => null;
        private static Character GetRandomAllyFigure(Character c) => null;
    }
}
