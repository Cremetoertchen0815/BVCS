﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.GeonBit;
using Nez.GeonBit.Materials;

namespace Betreten_Verboten
{
	internal class GameCore : Core
	{
		protected override void Initialize()
		{
			base.Initialize();
			DebugRenderEnabled = true;
			Window.AllowUserResizing = true;
			RegisterGlobalManager(new SceneManager(System.Reflection.Assembly.GetAssembly(GetType()), "Betreten_Verboten.Scenes"));

			GraphicsDevice.PresentationParameters.MultiSampleCount = 16;
			Screen.EnableAA = true;
			Screen.ApplyChanges();
			Scene.SetDefaultDesignResolution(1920, 1080, Scene.SceneResolutionPolicy.BestFit);

			Scene = new Scenes.Main.BaseGame();

		}
	}
}
