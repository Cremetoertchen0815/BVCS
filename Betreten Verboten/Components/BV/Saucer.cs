using Betreten_Verboten.Components.Base.Characters;
using Betreten_Verboten.Scenes.Main;
using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.Tweens;
using System;

namespace Betreten_Verboten.Components.BV
{
    public class Saucer : GeonComponent, ITelegramReceiver, IUpdatable
    {

        private const float SPEEN_SPEED = 3f;

        private int _moveCounter;
        private Vector3 _positionDelta;
        private GeonEntity _targetEntity;
        private BVGame _ownerScene;
        private ModelRenderer _renderer;
        private ITween<float> _camSpeener;

        public string TelegramSender => "saucer";

        public void MessageReceived(Telegram message)
        {
            switch (message.Head)
            {
                case "player_change":
                    if (++_moveCounter >= _ownerScene.Board.SaucerSpawnRate)
                    {
                        _moveCounter = 0;
                        SpawnField();
                    }

                    break;
                case "check_saucer":
                    if (message.Body is Character chr && chr.GlobalPosition.Valid && _ownerScene.Board.SaucerFields.Contains(chr.GlobalPosition.Position))
                        TriggerAnimation(chr);
                    else
                        this.SendPrivateTele("base", "char_move_done", false);
                    break;
            }
        }

        private void SpawnField()
        {
            int tryCount = 0;
            int nr = Nez.Random.Range(0, _ownerScene.Board.FieldCountTotal);
            var lst = _ownerScene.Board.SaucerFields;
            while (lst.Contains(nr))
            {
                if (++tryCount > _ownerScene.Board.FieldCountTotal) return;
                nr = Nez.Random.Range(0, _ownerScene.Board.FieldCountTotal);
            }
            lst.Add(nr);
        }

        public override void OnAddedToEntity()
        {
            _ownerScene = Entity.Scene as BVGame ?? throw new Exception("Can only be added to a BVGame scene!");
            Node.Position = new Vector3(0, 20, 0);

            //Create renderer
            _renderer = Entity.AddComponentAsChild(new ModelRenderer("mesh/saucer"));

            //Scale him and make him speen
            _renderer.Node.Scale = 0.05f * Vector3.One;
            _renderer.Node.RotationX = -MathHelper.PiOver2;
            _renderer.Node.Tween("RotationY", MathHelper.TwoPi, SPEEN_SPEED).SetFrom(0f).SetLoops(LoopType.RestartFromBeginning, -1).SetEaseType(EaseType.Linear).Start();
            _renderer.Enabled = false;

            this.TeleRegister();
        }

        public void TriggerAnimation(Character c)
        {
            if (_ownerScene.GameState == GameState.SaucerActive) return;
            _ownerScene.GameState = GameState.SaucerActive;
            _renderer.Enabled = true;
            _ownerScene.Board.SaucerFields.Remove(c.GlobalPosition.Position);

            //Speen camera
            _ownerScene.Camera.LookAtTarget = Entity;
            float srcSpeen = Nez.Random.NextAngle();
            _camSpeener = this.Tween("Rotator", MathHelper.TwoPi + srcSpeen, 20f).SetFrom(srcSpeen).SetEaseType(EaseType.Linear).SetLoops(LoopType.RestartFromBeginning, -1);
            _camSpeener.Start();

            //Play saucer animation
            var pos = _ownerScene.Board.GetCharacterPosition(c);
            Node.Tween("Position", new Vector3(pos.X, 3, pos.Y), 1f).SetFrom(new Vector3(0, 20, 0)).SetEaseType(EaseType.QuadOut).SetCompletionHandler(x => Lift(c)).Start();
        }

        private void Lift(Character c)
        {
            int tryCnt = 0;
            int boost = Nez.Random.Range(-6, 7);
            while (tryCnt++ < 15 && !IsChrPosValid(c.Position + boost)) boost = Nez.Random.Range(-6, 7);
            var pos = _ownerScene.Board.GetCharacterPosition(c, boost);
            _positionDelta = (_targetEntity = c.Entity).Node.Position - Node.Position;

            this.SendPrivateTele("base", "play_sfx", "saucer");

            this.Tween("PosY", 10f, 1.5f).SetDelay(0.5f).SetFrom(3f).SetEaseType(EaseType.SineInOut).SetLoops(LoopType.PingPong).Start();
            this.Tween("PosXZ", pos, 3f).SetDelay(0.5f).SetEaseType(EaseType.CubicInOut).SetCompletionHandler(z =>
            {
                _targetEntity = null;
                c.Position += boost;
                if (_ownerScene.Board.SaucerFields.Contains(c.GlobalPosition.Position))
                {
                    //Repeat saucer fly
                    _ownerScene.Board.SaucerFields.Remove(c.GlobalPosition.Position);
                    Lift(c);
                }
                else
                {
                    //Back out saucer
                    Node.Tween("Position", new Vector3(0, 40, 0), 1f).SetEaseType(EaseType.CubicInOut).SetDelay(0.5f).SetCompletionHandler(_ =>
                    {
                        _camSpeener.Stop();
                        _ownerScene.GameState = GameState.OtherAction;
                        _renderer.Enabled = false;
                        _ownerScene.Camera.OverridePosition = null;
                        _ownerScene.Camera.LookAtTarget = null;
                        _ownerScene.Camera.LookAt = null;
                        this.SendPrivateTele("base", "char_move_done", true);
                    }).Start();
                }
            }).Start();
        }

        private bool IsChrPosValid(int pos) => pos > -1;
        public void Update()
        {
            if (_targetEntity == null) return;
            _targetEntity.Node.Position = Node.Position - _positionDelta;
        }

        private float PosY
        {
            get => Node.PositionY;
            set => Node.PositionY = value;
        }

        private Vector2 PosXZ
        {
            get => new Vector2(Node.PositionX, Node.PositionZ);
            set
            {
                Node.PositionX = value.X;
                Node.PositionZ = value.Y;
            }
        }

        private float _rotator = 0f;
        public float Rotator
        {
            get => _rotator;
            set
            {
                if (_ownerScene.GameState != GameState.SaucerActive && _ownerScene.GameState != GameState.Outro) return;
                _rotator = value;
                _ownerScene.Camera.OverridePosition = new Vector3(33f * Mathf.Cos(_rotator), 15f, 33f * Mathf.Sin(_rotator));
            }
        }
    }
}
