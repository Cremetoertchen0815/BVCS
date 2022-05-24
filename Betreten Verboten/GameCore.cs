using GeonBit;
using GeonBit.ECS;
using GeonBit.ECS.Components.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betreten_Verboten
{
	class GameCore : GeonBitGame
	{
		public GameCore()
		{
			InitParams.Title = "New Project";
		}

		public override void Initialize()
		{
			/// TBD create your scene, components and init resources here.
			/// The code below contains a simple example of how to use UI, camera, and basic entity renderer.

			/// Example 1: create UI text
			ActiveScene.UserInterface.AddEntity(new GeonBit.UI.Entities.Paragraph("Welcome to GeonBit! Here's a sphere:"));

			/// Example 2: create camera and add to scene
			GameObject cameraObject = new GameObject("camera");
			cameraObject.AddComponent(new Camera());
			cameraObject.SceneNode.PositionZ = 5;
			cameraObject.Parent = ActiveScene.Root;

			/// Example 3: add 3d shape to scene
			GameObject shapeObject = new GameObject("shape");
			shapeObject.AddComponent(new ShapeRenderer(ShapeMeshes.Sphere));
			shapeObject.Parent = ActiveScene.Root;
		}

		public override void Draw(GameTime gameTime)
		{
			
		}

		public override void Update(GameTime gameTime)
		{
			
		}
	}
}
