using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;

namespace Betreten_Verboten.Components.Base.Characters
{
	public class Character : Component
	{
		public int Position { get; set; }
		public ModelRenderer Renderer { get; private set; }
		public RigidBody RigidBody { get; private set; }

		public const float CHAR_HITBOX_HEIGHT = 4f;
		public const float CHAR_HITBOX_WIDTH = 0.8f;
		private CharConfig _config;
		private Player _owner;
		private readonly float _scale;

		public Character(Player owner, CharConfig config, float Scale)
		{
			_owner = owner;
			_config = config;
			_scale = Scale;
		}

		public override void OnAddedToEntity()
		{
			var ent = (GeonEntity)Entity;

			//Config renderer
			Renderer = ent.AddComponentAsChild(new ModelRenderer("mesh/piece_std"));
			Renderer.Node.Position = Vector3.Down * CHAR_HITBOX_HEIGHT * 0.5f;
			Renderer.Node.Scale = new Vector3(_scale);
			Renderer.Node.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
			Renderer.SetMaterials(_config.GetMaterials());

			//Config rigid body
			RigidBody = ent.AddComponent(new RigidBody(new ConeInfo(CHAR_HITBOX_WIDTH, CHAR_HITBOX_HEIGHT), 10, 1, 1));
			RigidBody.Position = ent.Node.Position;
			RigidBody.AngularDamping = RigidBody.LinearDamping = 0.80f;
		}
	}
}
