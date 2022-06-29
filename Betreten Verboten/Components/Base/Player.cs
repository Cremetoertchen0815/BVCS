using Betreten_Verboten.Components.Base.Characters;
using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Betreten_Verboten.Components.Base
{
	public abstract class Player : Component, ITelegramReceiver
	{
		public int Nr { get; set; }
		public string ID { get; set; }
		public bool MissingTurn { get; set; }
		public abstract int Points { get; }
		public string TelegramSender => "player_" + Nr;
		public CharConfig CharacterConfig { get; set; }


		protected Character[] _figures;
		public BVBoard Board { get; private set; }

		//ctor
		public Player(int Nr) => this.Nr = Nr;

		public override void OnAddedToEntity()
		{
			//Fetch GeonScene & board
			var geonScene = (GeonScene)Entity.Scene;
			Board = geonScene.FindEntity("board").GetComponent<BVBoard>();

			//Create figures
			_figures = new Character[Board.FigureCount];
			for (int i = 0; i < _figures.Length; i++)
			{
				var ent = geonScene.CreateGeonEntity("figure" + Nr + "-" + i, new Vector3(0, Character.CHAR_HITBOX_HEIGHT - 1f, 0), NodeType.BoundingBoxCulling);
				_figures[i] = ent.AddComponent(new Character(this, i, CharacterConfig)).SetPosition(-1);
			}

			//Register in telegram service
			this.TeleRegister("players");
			this.SendPublicTele("player_registered", this);
		}

		public abstract void MessageReceived(Telegram message);

		public Character[] GetFigures() => _figures;

		public void DecideAfterDiceroll(List<int> nrs) => Entity.AddComponent(new CharPicker(nrs.Sum()));
	}
}
