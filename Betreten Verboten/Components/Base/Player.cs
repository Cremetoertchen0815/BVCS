using Betreten_Verboten.Components.Base.Characters;
using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;

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


		private Character[] _figures;
		private BVBoard _board;

		//ctor
		public Player(int Nr) => this.Nr = Nr;

		public override void OnAddedToEntity()
		{
			//Fetch GeonScene & board
			var geonScene = (GeonScene)Entity.Scene;
			_board = geonScene.FindEntity("board").GetComponent<BVBoard>();

			//Create figures
			_figures = new Character[_board.FieldCount];
			for (int i = 0; i < _figures.Length; i++)
			{
				var pos = _board.GetFieldPosition(Nr, i, FieldType.Regular, false) * 0.04f;
				var ent = geonScene.CreateGeonEntity("figure" + Nr + "-" + i, new Vector3(pos.X, Character.CHAR_HITBOX_HEIGHT, pos.Y), NodeType.BoundingBoxCulling);
				_figures[i] = ent.AddComponent(new Character(CharacterConfig, _board.CharScale));
			}

			//Register in telegram service
			this.TeleRegister("players");
			this.TeleSendPublic("player_registered", this);
		}

		public abstract void MessageReceived(Telegram message);
	}
}
