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
