using Betreten_Verboten.Components.BV.Player;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;
using Nez.Textures;
using System.Collections.Generic;
using static Betreten_Verboten.GlobalFields;

namespace Betreten_Verboten.Components.Base
{
    public abstract class BVBoard : RenderableComponent, ITelegramReceiver
    {
        //Const
        private const int TEX_RES = 1000;
        private const int CIRCLE_RES = 5;
        private const float CLEAR_COLOR = 0.04f;

        //Cached field positions
        private Vector2[] _connectingSegments;
        private Vector2[] _fieldsRegular;
        private Vector2[] _fieldsHome;
        private Vector2[] _fieldsHouse;
        private Vector2[] _suicideOffsets;

        //Fields
        private List<int> _ufoFields;
        private BVPlayer[] _players;

        //Assets & renderers
        private RenderTexture _boardTexture;
        protected ShapeRenderer _shapeRenderer;
        protected RenderTexture _shadowProjection;
        protected StaticBody _kinematicBody;
        private Texture2D _texArrow;


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
            _kinematicBody.Restitution = 0f;

            //Load assets & prepare fields
            _ufoFields = new List<int>();
            _texArrow = Entity.Scene.Content.LoadTexture("texture/arrow_right");

            //Add dice limiting box
            ((GeonScene)Entity.Scene).CreateGeonEntity("DiceLimiterA", new Vector3(-550, 50, -500), NodeType.BoundingBoxCulling).AddComponent(new StaticBody(new BoxInfo(new Vector3(50, 100, 50)))).Restitution = 1f;
            ((GeonScene)Entity.Scene).CreateGeonEntity("DiceLimiterB", new Vector3(-500, 50, -550), NodeType.BoundingBoxCulling).AddComponent(new StaticBody(new BoxInfo(new Vector3(50, 100, 50)))).Restitution = 1f;
            ((GeonScene)Entity.Scene).CreateGeonEntity("DiceLimiterC", new Vector3(-450, 50, -500), NodeType.BoundingBoxCulling).AddComponent(new StaticBody(new BoxInfo(new Vector3(50, 100, 50)))).Restitution = 1f;
            ((GeonScene)Entity.Scene).CreateGeonEntity("DiceLimiterD", new Vector3(-500, 50, -450), NodeType.BoundingBoxCulling).AddComponent(new StaticBody(new BoxInfo(new Vector3(50, 100, 50)))).Restitution = 1f;

            _players = new BVPlayer[PlayerCount]; //Create player array
            SetRenderLayer(RENDER_LAYER_BOARD); //Config renderable 
            CalculateCachedFields(); //Calculate and cache rendering coords and segments
            this.TeleRegister(); //Register in telegram service


        }

        public override void OnRemovedFromEntity() => this.TeleDeregister();

        protected Vector2 _centerOffset = new Vector2(TEX_RES / 2);

        //Abstract properties
        protected abstract int FieldDistance { get; }
        protected abstract int FieldHouseDiameter { get; }
        protected abstract int FieldHomeDiameter { get; }
        protected abstract int FieldPlayerDiameter { get; }
        protected abstract int FieldPlayerArrowDiameter { get; }
        public abstract int FieldCountPP { get; }
        public abstract int FigureCountPP { get; }
        public virtual int FieldCountTotal => FieldCountPP * PlayerCount; 
        public virtual int FigureCountTotal => FigureCountPP * PlayerCount; 
        public virtual int DistanceLimit => FieldCountTotal + FigureCountPP - 1;
        public abstract float FigureJumpHeight { get; }
        public abstract int PlayerCount { get; }
        public abstract float CharScale { get; }
        public abstract Vector2 GetFieldPosition(int player, int fieldNr, FieldType fieldType, bool centerOffset = true);


        public override bool IsVisibleFromCamera(Camera camera) => true;
        public override RectangleF Bounds => new RectangleF(0, 0, TEX_RES, TEX_RES);
        public string TelegramSender => "board";


        public override void Render(Batcher batcher, Camera camera)
        {
            batcher.DrawHollowRect(new RectangleF(0, 0, TEX_RES, TEX_RES), Color.White, 4);

            //Draw connecting lines
            for (int i = 0; i < _connectingSegments.Length; i += 2) batcher.DrawLine(_connectingSegments[i], _connectingSegments[i + 1], Color.White, 2);

            for (int i = 0; i < PlayerCount; i++)
            {
                var pl = _players[i];
                var plColor = Characters.CharConfig.GetStdColor(i);

                //Draw regular fields
                for (int j = 0; j < FieldCountPP; j++)
                {
                    batcher.DrawCircle(_fieldsRegular[i * FieldCountPP + j], FieldPlayerDiameter, j == 0 ? plColor : Color.White, 3, CIRCLE_RES);
                    if (j == 0) batcher.Draw(_texArrow, new Rectangle(_fieldsRegular[i * FieldCountPP].ToPoint(), new Point(FieldPlayerArrowDiameter)), null, plColor, MathHelper.PiOver2 * (4f * i / PlayerCount + 3f), Vector2.One * 17.5f, SpriteEffects.None, 0f );

                }

                //Draw home & house fields
                for (int j = 0; j < FigureCountPP; j++)
                {
                    int idx = i * FigureCountPP + j;
                    batcher.DrawCircle(_fieldsHome[idx], FieldHomeDiameter, plColor, 2, CIRCLE_RES);
                    batcher.DrawCircle(_fieldsHouse[idx], FieldHouseDiameter, plColor, 2, CIRCLE_RES);
                }

                //Draw suicide cross
                if (pl.SuicideField >= 0)
                {
                    var pos = _fieldsRegular[GlobalPosition.FromLocalPosition(pl, pl.SuicideField).Position];
                    batcher.DrawLine(pos + _suicideOffsets[0], pos + _suicideOffsets[1], plColor, 3);
                    batcher.DrawLine(pos + _suicideOffsets[2], pos + _suicideOffsets[3], plColor, 3);
                }

            }

            foreach (var item in _ufoFields) batcher.DrawCircle(_fieldsRegular[item], FieldPlayerDiameter, Color.MintCream, 5, CIRCLE_RES);

        }

        /// <summary>
        /// Calculates the coordinates of fields on the board and caches them to speed up rendering.
        /// </summary>
        protected void CalculateCachedFields()
        {
            //Init arrays
            _connectingSegments = new Vector2[FieldCountTotal * 2];
            _fieldsRegular = new Vector2[FieldCountTotal];
            _fieldsHouse = new Vector2[FigureCountTotal];
            _fieldsHome = new Vector2[FigureCountTotal];

            //Define variables
            float invFieldDistance = (float)FieldPlayerDiameter / FieldDistance; //Multiplier for calculating the overlap between connection segments and their respective fields
            bool isFirstElement = true; //Flag used for skipping the segment, as it requires a previous field, gets handled later
            Vector2 nuPos = Vector2.Zero, lastPos, conVec;
            int idxPt = 0, idxSeg = 0, idxSpc = 0; //Indices pointing to the next item in the respective array, for Regular Field(Pt), Connection Segment(Seg) & Home/House Fields(Spc)

            //Loop through every connection field
            for (int i = 0; i < PlayerCount; i++)
            {
                //Draw field
                for (int j = 0; j < FieldCountPP; j++)
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
                for (int j = 0; j < FigureCountPP; j++)
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

            //Generate suicide cross offsets
            _suicideOffsets = new Vector2[]
            {
                Vector2.UnitX.Rotate(1f / 8 * MathHelper.TwoPi) * FieldPlayerDiameter,
                Vector2.UnitX.Rotate(5f / 8 * MathHelper.TwoPi) * FieldPlayerDiameter,
                Vector2.UnitX.Rotate(7f / 8 * MathHelper.TwoPi) * FieldPlayerDiameter,
                Vector2.UnitX.Rotate(3f / 8 * MathHelper.TwoPi) * FieldPlayerDiameter
            };
        }

        public Vector2 GetCharacterPosition(Characters.Character c)
        {
            if (c.Position < 0) return (_fieldsHome[c.Nr + FigureCountPP * c.Owner.Nr] - _centerOffset) * 0.04f;
            if (c.Position >= FieldCountTotal) return (_fieldsHouse[c.Position - FieldCountTotal + FigureCountPP * c.Owner.Nr] - _centerOffset) * 0.04f;
            return (_fieldsRegular[(c.Position + FieldCountPP * c.Owner.Nr) % (FieldCountTotal)] - _centerOffset) * 0.04f;
        }

        public void MessageReceived(Telegram message)
        {
            switch (message.Head)
            {
                case "player_registered":
                    int source = int.Parse(message.Sender.Substring(7));
                    _players[source] = (BVPlayer)message.Body;
                    break;
                case "ufo_field_added":
                    _ufoFields.Add((int)message.Body);
                    break;
                case "ufo_field_removed":
                    source = (int)message.Body;
                    if (_ufoFields.Contains(source)) _ufoFields.Remove(source);
                    break;
                default:
                    break;
            }
        }
    }
}
