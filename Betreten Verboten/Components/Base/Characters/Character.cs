using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;

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
        private int _position;
        private int _travelDistLeft = 0;
        private CharConfig _config;

        public Character(Player owner, int nr, CharConfig config)
        {
            Owner = owner;
            Nr = nr;
            _config = config;
        }

        public override void OnAddedToEntity()
        {
            var ent = (GeonEntity)Entity;

            //Set color
            _config.SetStdColorScheme(Owner.Nr);

            //Config renderer
            Renderer = ent.AddComponentAsChild(new ModelRenderer("mesh/piece_std"));
            Renderer.Node.Position = Vector3.Down * CHAR_HITBOX_HEIGHT * 0.5f;
            Renderer.Node.Scale = new Vector3(Owner.Board.CharScale);
            Renderer.Node.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
            Renderer.SetMaterials(_config.GetMaterials());

            //Config rigid body
            RigidBody = ent.AddComponent(new RigidBody(new ConeInfo(CHAR_HITBOX_WIDTH, CHAR_HITBOX_HEIGHT), 10, 1, 1));
            RigidBody.Position = ent.Node.Position;
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
                    //Check for kicking condition. That being that either landing the character on its final landing field or the character standing on its homebase.
                    //Of course we ignore our own characters.
                    if (source.kicker == this) break;
                    if (source.finalField && source.kicker.GlobalPosition == GlobalPosition || source.kicker.GlobalPosition == GlobalPosition && source.kicker.Position == 0) Kick(source.kicker);
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

        private void TakeStep(bool firstStep = false)
        {
            _travelDistLeft--;
            SetPosition(Position + 1);
            this.SendPrivateTele("char", "landed_on_field", (this, _travelDistLeft < 1));
            this.SendPrivateTele("base", "resort_score", null);
            if (_travelDistLeft < 1) this.SendPrivateTele("base", "char_move_done", null); else Core.Schedule(0.5f, x => TakeStep());
        }
    }
}
