using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten.Components.Base.Characters
{
	class Character : Component, IUpdatable
	{
		public const float CHAR_HITBOX_HEIGHT = 4f;
		public const float CHAR_HITBOX_WIDTH = 0.8f;
		private CharConfig _config;
		private float _scale;

		public Character(CharConfig config, float Scale)
		{
			_config = config;
			_scale = Scale;
		}

		public override void OnAddedToEntity()
		{
			var ent = (GeonEntity)Entity;
			var renderer = ent.AddComponentAsChild(new ModelRenderer("mesh/piece_std"));
			renderer.SetMaterials(_config.GetMaterials());

			renderer.Node.Position = Vector3.Down * CHAR_HITBOX_HEIGHT * 0.5f;
			renderer.Node.Scale = new Vector3(_scale);
			renderer.Node.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);

			var rb = ent.AddComponent(new RigidBody(new ConeInfo(CHAR_HITBOX_WIDTH, CHAR_HITBOX_HEIGHT), 10, 1, 1));
			rb.Position = ent.Node.Position;
		}

		public void Update()
		{
			Console.WriteLine();

			var ent = (GeonEntity)Entity;
			var siis = ent.GetComponent<ModelRenderer>();
			if (siis.Node == ent.Node) Console.WriteLine();
		}
	}
}
