using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using Nez.GeonBit.UI;
using Nez.GeonBit.UI.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base.Dice
{
    public class SimpleDice : Component, IUpdatable
    {
        private const float _tickSleep = 0.01f;
        private const float _afterSleep = 0.2f;
        private const int _tickCount = 20;
        private const float DICE_SIZE = 300f;
        private const float MARGIN = 50f;

        private Image _uiEntity;
        private Image _uiBG;
        private int _ticksLeft = 0;
        private float _tickTimer;
        private int _currentNr;
        public SimpleDice()
        {
            _uiEntity = UserInterface.Active.AddEntity(new Image(Core.Content.LoadTexture("texture/dice_simple_eyes"), new Vector2(DICE_SIZE), ImageDrawMode.Stretch, Anchor.BottomRight, new Vector2(-DICE_SIZE, MARGIN)));
            _uiEntity.Background = _uiBG = new Image(Core.Content.LoadTexture("texture/dice_simple_border"), new Vector2(DICE_SIZE), ImageDrawMode.Stretch, Anchor.Center);
        }

        private bool _isVisible = false;
        public bool Visible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _uiEntity.Tween("Offset", _isVisible ? new Vector2(MARGIN) : new Vector2(-DICE_SIZE, MARGIN), 0.1f).SetEaseType(Nez.Tweens.EaseType.CubicInOut).Start();
                //Reset value if fading in
                if (_isVisible)
                {
                    _currentNr = -1;
                    _uiEntity.SourceRectangle = GetNrSourceRect();
                }
            }
        }

        public void SetColor(Color c) => _uiEntity.FillColor = _uiBG.FillColor = c;

        public void Throw()
        {
            if (_ticksLeft > 0) return;
            _ticksLeft = _tickCount;
            _tickTimer = _tickSleep;
        }

        public void Update()
        {
            if (_ticksLeft < 1) return;
            if ((_tickTimer -= Time.DeltaTime) <= 0)
            {
                //Tick down timer
                _tickTimer = _tickSleep;
                _ticksLeft--;
                //Generate new number
                _currentNr = Random.Range(1, 7);
                _uiEntity.SourceRectangle = GetNrSourceRect();
            }

            if (_ticksLeft < 1) Core.Schedule(_afterSleep, x => _currentNr.SendPrivateObj("dice", "base", "dice_value_set"));
        }

        private Rectangle GetNrSourceRect()
        {
            switch (_currentNr)
            {
                case 1:
                    return new Rectangle(0, 0, 260, 260);
                case 2:
                    return new Rectangle(260, 0, 260, 260);
                case 3:
                    return new Rectangle(520, 0, 260, 260);
                case 4:
                    return new Rectangle(0, 260, 260, 260);
                case 5:
                    return new Rectangle(260, 260, 260, 260);
                case 6:
                    return new Rectangle(520, 260, 260, 260);
                default:
                    return Rectangle.Empty;
            }
        }
    }
}
