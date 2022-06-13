﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using Nez.Textures;
using static Betreten_Verboten.GlobalFields;

namespace Betreten_Verboten.Components.Base
{
	public abstract class BVBoard : RenderableComponent, ITelegramReceiver
	{

		private const int TEX_RES = 1000;
		private const int CIRCLE_RES = 5;
		private const float CLEAR_COLOR = 0.15f;

		//Cached field positions
		private Vector2[] _connectingSegments;
		private Vector2[] _fieldsRegular;
		private Vector2[] _fieldsHome;
		private Vector2[] _fieldsHouse;

		private Player[] _players;

		private RenderTexture _boardTexture;
		protected ShapeRenderer _shapeRenderer;
		protected RenderTexture _shadowProjection;
		protected StaticBody _kinematicBody;

		public override void OnAddedToEntity()
		{
			//Check for correct Entity type
			Insist.IsNotNull(Entity as GeonEntity, "Board has to be added to GeonEntity!");

			//Add board texture renderer
			_shadowProjection = Entity.Scene.AddRenderer(new ShadowPlaneRenderer(-2)).RenderTexture;
			Entity.Scene.AddRenderer(new RenderLayerRenderer(-1, RENDER_LAYER_BOARD) { RenderTexture = _boardTexture = new RenderTexture(TEX_RES, TEX_RES) { ResizeBehavior = RenderTexture.RenderTextureResizeBehavior.None }, RenderTargetClearColor = new Color(Color.White * CLEAR_COLOR, 1f) });

			//Configure 3D renderer
			var geonEntity = Entity as GeonEntity;
			_shapeRenderer = geonEntity.AddComponentAsChild(new ShapeRenderer(ShapeMeshes.Plane));
			_shapeRenderer.Node.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
			_shapeRenderer.Node.RotationType = RotationType.Euler;
			_shapeRenderer.Node.Scale = new Vector3(20);
			_shapeRenderer.Node.Position = new Vector3(0, 1, 0);
			_shapeRenderer.RenderingQueue = RenderingQueue.BackgroundNoCull;
			_shapeRenderer.SetMaterial(new ShadowPlaneMaterial() { Texture = _boardTexture, ShadowMap = _shadowProjection, TextureEnabled = true, DiffuseColor = Color.LightGray, AmbientLight = Color.White * 0.20f });

			//Configure physics
			_kinematicBody = geonEntity.AddComponent(new StaticBody(new EndlessPlaneInfo(Vector3.Up)));
			_kinematicBody.CollisionGroup = Nez.GeonBit.Physics.CollisionGroups.Terrain;
			_kinematicBody.Restitution = 1f;


			_players = new Player[PlayerCount]; //Create player array
			SetRenderLayer(RENDER_LAYER_BOARD); //Config renderable 
			CalculateCachedFields(); //Calculate and cache rendering coords and segments
			this.TeleRegister(); //Register in telegram service


		}

		public override void OnRemovedFromEntity() => this.TeleDeregister();

		protected Vector2 _centerOffset = new Vector2(TEX_RES / 2);

		//Abstract properties
		public abstract int FieldDistance { get; }
		public abstract int FieldHouseDiameter { get; }
		public abstract int FieldHomeDiameter { get; }
		public abstract int FieldPlayerDiameter { get; }
		public abstract int FieldCount { get; }
		public abstract int FigureCount { get; }
		public abstract int PlayerCount { get; }
		public abstract float CharScale { get; }
		public abstract Vector2 GetFieldPosition(int player, int fieldNr, FieldType fieldType, bool centerOffset = true);


		public override bool IsVisibleFromCamera(Camera camera) => true;
		public override RectangleF Bounds => new RectangleF(0, 0, TEX_RES, TEX_RES);
		public string TelegramSender => "board";


		protected override void Render(Batcher batcher, Camera camera)
		{

			for (int i = 0; i < PlayerCount; i++)
			{
				//Draw field
				for (int j = 0; j < FieldCount; j++)
				{
					batcher.DrawCircle(_fieldsRegular[i * FieldCount + j], FieldPlayerDiameter, Color.White, 3, CIRCLE_RES);
				}

				for (int j = 0; j < FigureCount; j++)
				{
					int idx = i * FigureCount + j;
					batcher.DrawCircle(_fieldsHome[idx], FieldHomeDiameter, Color.White, 2, CIRCLE_RES);
					batcher.DrawCircle(_fieldsHouse[idx], FieldHouseDiameter, Color.White, 2, CIRCLE_RES);
				}
			}

			for (int i = 0; i < _connectingSegments.Length; i += 2)
			{
				batcher.DrawLine(_connectingSegments[i], _connectingSegments[i + 1], Color.White, 2);
			}
		}

		/// <summary>
		/// Calculates the coordinates of fields on the board and caches them to speed up rendering.
		/// </summary>
		protected void CalculateCachedFields()
		{
			//Init arrays
			_connectingSegments = new Vector2[PlayerCount * FieldCount * 2];
			_fieldsRegular = new Vector2[PlayerCount * FieldCount];
			_fieldsHouse = new Vector2[PlayerCount * FigureCount];
			_fieldsHome = new Vector2[PlayerCount * FigureCount];

			//Define variables
			var invFieldDistance = (float)FieldPlayerDiameter / FieldDistance; //Multiplier for calculating the overlap between connection segments and their respective fields
			bool isFirstElement = true; //Flag used for skipping the segment, as it requires a previous field, gets handled later
			Vector2 nuPos = Vector2.Zero, lastPos, conVec;
			int idxPt = 0, idxSeg = 0, idxSpc = 0; //Indices pointing to the next item in the respective array, for Regular Field(Pt), Connection Segment(Seg) & Home/House Fields(Spc)

			//Loop through every connection field
			for (int i = 0; i < PlayerCount; i++)
			{
				//Draw field
				for (int j = 0; j < FieldCount; j++)
				{
					//Fetch new field position and cache old one
					lastPos = nuPos;
					nuPos = _fieldsRegular[idxPt++] = GetFieldPosition(i, j, FieldType.Regular);

					//If not the first element(being handled later), calculate connecing segment
					if (isFirstElement)
					{
						isFirstElement = false;
					}
					else
					{
						conVec = (nuPos - lastPos) * invFieldDistance;
						_connectingSegments[idxSeg++] = lastPos + conVec;
						_connectingSegments[idxSeg++] = nuPos - conVec;
						lastPos = nuPos;
					}
				}

				//Calculate home and house field positions
				for (int j = 0; j < FigureCount; j++)
				{
					_fieldsHouse[idxSpc] = GetFieldPosition(i, j, FieldType.House);
					_fieldsHome[idxSpc++] = GetFieldPosition(i, j, FieldType.Home);
				}
			}

			//Add in last missing segment
			lastPos = nuPos;
			nuPos = GetFieldPosition(0, 0, FieldType.Regular);
			conVec = (nuPos - lastPos) * invFieldDistance;
			_connectingSegments[idxSeg++] = lastPos + conVec;
			_connectingSegments[idxSeg++] = nuPos - conVec;

		}

		public void MessageReceived(Telegram message)
		{
			switch (message.Head)
			{
				case "player_registered":
					int source = int.Parse(message.Sender.Substring(7));
					_players[source] = (Player)message.Body;
					break;
				default:
					break;
			}
		}
	}
}