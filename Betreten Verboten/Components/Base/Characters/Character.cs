using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.Tweens;

namespace Betreten_Verboten.Components.Base.Characters
{
    public class Character : GeonComponent, ITelegramReceiver
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
        private bool _rbActive = false;

        private static bool _playKick = false;

        public Character(Player owner, int nr, CharConfig config)
        {
            Owner = owner;
            Nr = nr;
            _config = config;
        }

        public override void OnAddedToEntity()
        {

            //Set color
            _config.SetStdColorScheme(Owner.Nr);

            //Config renderer
            Renderer = Entity.AddComponentAsChild(new ModelRenderer("mesh/piece_std"));
            Renderer.Node.Position = Vector3.Down * CHAR_HITBOX_HEIGHT * 0.5f;
            Renderer.Node.Scale = new Vector3(Owner.Board.CharScale);
            Renderer.Node.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
            Renderer.SetMaterials(_config.GetMaterials());

            //Config rigid body

            this.TeleRegister("char");
        }

        public override void OnRemovedFromEntity() => this.TeleDeregister();

        public bool RigidBodyEnabled
        {
            get => _rbActive;
            set
            {
                if (_rbActive == value) return;
                _rbActive = value;
                if (_rbActive)
                {
                    RigidBody = Entity.AddComponent(new RigidBody(new ConeInfo(CHAR_HITBOX_WIDTH, CHAR_HITBOX_HEIGHT), 10f, 10f, 1));
                    RigidBody.Position = Node.Position;
                    RigidBody.Restitution = 0.4f;
                    RigidBody.AngularDamping = RigidBody.LinearDamping = 0.65f;
                    RigidBody.Enabled = true;
                    RigidBody.EnableSimulation = true;
                }
                else
                {
                    Entity.RemoveComponent(RigidBody);
                    RigidBody = null;
                }
            }
        }

        public void MessageReceived(Telegram message)
        {
            switch (message.Head)
            {
                case "char_move":
                    AdvanceSteps((int)message.Body);
                    break;
                case "landed_on_field":
                    var source = ((Character kicker, bool finalField))message.Body;
                    int oldPos = source.kicker.Position;
                    //Test if real kicking is gonna be checked of if simple land is in progress
                    if (source.kicker == this || source.kicker.GlobalPosition != GlobalPosition)  break;

                    //Check for kicking condition. That being that either landing the character on its final landing field or the character standing on its homebase.
                    //Of course we ignore our own characters.
                    System.Action<ITween<float>> ac = x =>
                    {
                        if (!source.finalField && oldPos != 0) return;
                        this.SendPrivateTele("base", "play_sfx", "stomp"); //Play kicking sound
                        SetPosition(-1);
                        source.kicker.Owner.AdditionalPoints += source.finalField ? 25 : 50;
                    };
                    //Play ducking animation
                    this.Tween("ScaleHeight", 0f, CHAR_HALF_WALK_TIME * 2f).SetEaseType(EaseType.Linear).SetLoops(LoopType.PingPong).Start();
                    this.Tween("PosHeight", 0f, CHAR_HALF_WALK_TIME * 2f).SetFrom(3f).SetEaseType(EaseType.Linear).SetLoopCompletionHandler(ac).SetLoops(LoopType.PingPong).Start();
                    if (source.finalField || oldPos == 0) _playKick = true;
                    break;
                default:
                    break;
            }
        }

        public void Kick(Character killer)
        {
            this.Tween("ScaleHeight", 0f, CHAR_HALF_WALK_TIME * 2f).SetFrom(1f).SetEaseType(EaseType.Linear).SetLoops(LoopType.PingPong).SetLoopCompletionHandler(x => SetPosition(-1)).Start();
            this.Tween("PosHeight", 0f, CHAR_HALF_WALK_TIME * 2f).SetFrom(3f).SetEaseType(EaseType.Linear).SetLoops(LoopType.PingPong).Start();
        }

        public Character SetPosition(int pos)
        {
            _position = pos;
            GlobalPosition = GlobalPosition.FromChar(this);
            var node = Entity?.Node;
            if (node == null) return this;
            var pos2D = Owner.Board.GetCharacterPosition(this);
            node.Position = new Vector3(pos2D.X, node.Position.Y, pos2D.Y);
            if (RigidBody != null)
            {
                RigidBody.Position = node.Position;
                RigidBody.CopyNodeWorldMatrix();
            }
            return this;
        }

        public void AdvanceSteps(int distance)
        {
            if ((_travelDistLeft = distance) > 0) TakeStep(true);
        }

        private Vector2 Pos2D { get => new Vector2(Node.Position.X, Node.Position.Z); set => Node.Position = new Vector3(value.X, Node.Position.Y, value.Y); }
        private float PosHeight { get => Node.Position.Y; set => Node.Position = new Vector3(Node.Position.X, value, Node.Position.Z); }
        private float ScaleHeight { get => Node.ScaleY; set => Node.Scale = new Vector3(value); }

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

            //Play sfx
            if (firstStep) return;
            if (_playKick) _playKick = false; else this.SendPrivateTele("base", "play_sfx", "jump");

        }

        private void AdvAnimationStep(ITween<Vector2> y)
        {
            if (_travelDistLeft < 1)
            {
                this.SendPrivateTele("base", "play_sfx", _playKick ? "stomp" : "land");
                _playKick = false;
                Owner.DecideAfterCharacterLand();
                RigidBody?.CopyNodeWorldMatrix(); //Update rigid body
                this.SendPrivateTele("saucer", "check_saucer", this); //Report landing
            }
            else
            {
                TakeStep();
            }
        }
    }
}
