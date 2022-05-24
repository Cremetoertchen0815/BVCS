using GeonBit.ECS;
using GeonBit.ECS.Components.Graphics;
using GeonBit.UI.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten
{
	class Lol : GeonBit.GeonBitGame
	{
		public override void Draw(GameTime gameTime)
		{
			
		}

		public override void Initialize()
		{
			ActiveScene.UserInterface.AddEntity(new Paragraph("This is a fucking test text!"));

			var soos = new GameObject("camera");
			soos.AddComponent(new Camera());
			soos.SceneNode.PositionZ = 5;
			soos.Parent = ActiveScene.Root;


			/// Example 3: add 3d shape to scene
			GameObject shapeObject = new GameObject("shape");
			shapeObject.AddComponent(new ShapeRenderer(ShapeMeshes.Sphere));
			shapeObject.Parent = ActiveScene.Root;

		}

		public override void Update(GameTime gameTime)
		{
			
		}
	}
	class Program
	{
		static void Main(string[] args)
		{
			var lol = new Lol();
			GeonBit.GeonBitMain.Instance.Run(new Lol());
		}
	}
}
