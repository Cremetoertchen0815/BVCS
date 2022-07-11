using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.Tweens;

namespace Betreten_Verboten.Components.Base.Characters
{
    public class Character : Component, ITelegramReceiver
    {
        //Properties
        public int Position { get => _position; set => SetPosition(value); }
        public GlobalPosition GlobalPosition { get; private set; }
        public int Nr { get; private set; }
        public bool CanBeSelected { get; set; }
        public Player Owner { get; private set; }
        public ModelRenderer Renderer { get; private set; }
        public RigidBody RigidBody { get; private set; }
        public string TelegramSender => "char" + Owner.Nr + "_" + Nr;


        //Fields
        public const float CHAR_HITBOX_HEIGHT = 4f;
        public const float CHAR_HITBOX_WIDTH = 0.8f;
        public const float CHAR_HALF_WALK_TIME = 0.15f;
        private int _position;
        private int _travelDistLeft = 0;
        private CharConfig _config;
        private GeonEntity _geonCache;

        public Character(Player owner, int nr, CharConfig config)
        {
            Owner = owner;
            Nr = nr;
            _config = config;
        }

        public override void OnAddedToEntity()
        {
            _geonCache = (GeonEntity)Entity;

            //Set color
            _config.SetStdColorScheme(Owner.Nr);

            //Config renderer
            Renderer = _geonCache.AddComponentAsChild(new ModelRenderer("mesh/piece_std"));
            Renderer.Node.Position = Vector3.Down * CHAR_HITBOX_HEIGHT * 0.5f;
            Renderer.Node.Scale = new Vector3(Owner.Board.CharScale);
            Renderer.Node.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
            Renderer.SetMaterials(_config.GetMaterials());

            //Config rigid body
            RigidBody = _geonCache.AddComponent(new RigidBody(new ConeInfo(CHAR_HITBOX_WIDTH, CHAR_HITBOX_HEIGHT), 10, 1, 1));
            RigidBody.Position = _geonCache.Node.Position;
            RigidBody.AngularDamping = RigidBody.LinearDamping = 0.80f;
            RigidBody.Enabled = false;
            RigidBody.EnableSimulation = false;

            this.TeleRegister("char");
        }

        public override void OnRemovedFromEntity() => this.TeleDeregister();

        public void MessageReceived(Telegram message)
        {
            switch (message.Head)
            {
                case "char_move":
                    AdvanceSteps((int)message.Body);
                    break;
                case "landed_on_field":
                    var source = ((Character kicker, bool finalField))message.Body;
                    if (source.kicker == this || source.kicker.GlobalPosition != GlobalPosition) break;
                    //Play ducking animation
                    this.Tween("ScaleHeight", 0f, CHAR_HALF_WALK_TIME * 2f).SetEaseType(EaseType.Linear).SetLoops(LoopType.PingPong).Start();
                    this.Tween("PosHeight", 0f, CHAR_HALF_WALK_TIME * 2f).SetFrom(3f).SetEaseType(EaseType.Linear).SetLoops(LoopType.PingPong).Start();
                    //Check for kicking condition. That being that either landing the character on its final landing field or the character standing on its homebase.
                    //Of course we ignore our own characters.
                    if (source.finalField || source.kicker.Position == 0) Kick(source.kicker);
                    break;
                default:
                    break;
            }
        }

        public void Kick(Character killer) => SetPosition(-1);

        public Character SetPosition(int pos)
        {
            _position = pos;
            GlobalPosition = GlobalPosition.FromChar(this);
            var node = (Entity as GeonEntity).Node;
            if (node == null) return this;
            var pos2D = Owner.Board.GetCharacterPosition(this);
            node.Position = new Vector3(pos2D.X, node.Position.Y, pos2D.Y);
            if (RigidBody != null) RigidBody.Position = node.Position;
            return this;
        }

        public void AdvanceSteps(int distance)
        {
            if ((_travelDistLeft = distance) > 0) TakeStep(true);
        }

        private Vector2 Pos2D { get => new Vector2(_geonCache.Node.Position.X, _geonCache.Node.Position.Z); set => _geonCache.Node.Position = new Vector3(value.X, _geonCache.Node.Position.Y, value.Y); }
        private float PosHeight { get => _geonCache.Node.Position.Y; set => _geonCache.Node.Position = new Vector3(_geonCache.Node.Position.X, value, _geonCache.Node.Position.Z); }
        private float ScaleHeight { get => _geonCache.Node.ScaleY; set => _geonCache.Node.Scale = new Vector3(value); }

        private void TakeStep(bool firstStep = false)
        {
            //Prepare variables
            _travelDistLeft--;
            _position++;
            GlobalPosition = GlobalPosition.FromChar(this);
            var nuPos2D = Owner.Board.GetCharacterPosition(this);

            //Init animations
            this.Tween("PosHeight", Owner.Board.FigureJumpHeight, CHAR_HALF_WALK_TIME).SetFrom(3f).SetEaseType(EaseType.CubicOut).SetLoops(LoopType.PingPong).Start();
            this.Tween("Pos2D", nuPos2D, 2 * CHAR_HALF_WALK_TIME).SetEaseType(EaseType.Linear).SetCompletionHandler(AdvAnimationStep).Start();

            //Send telegrams
            this.SendPrivateTele("char", "landed_on_field", (this, _travelDistLeft < 1));
            this.SendPrivateTele("base", "resort_score", null);
            
        }

        private void AdvAnimationStep(ITween<Vector2> y) { if (_travelDistLeft < 1) this.SendPrivateTele("base", "char_move_done", null); else TakeStep(); }
    }
}
