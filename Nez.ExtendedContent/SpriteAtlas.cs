using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nez.ExtendedContent
{

    public class SpriteAtlasData
	{
		public List<string> Names = new List<string>();
		public List<Rectangle> SourceRects = new List<Rectangle>();
		public List<Vector2> Origins = new List<Vector2>();

		public List<string> AnimationNames = new List<string>();
		public List<int> AnimationFps = new List<int>();
		public List<List<int>> AnimationFrames = new List<List<int>>();

		public byte[] RawTextureData;


		//Copied from Nez
		/// <summary>
		/// parses a .atlas file into a temporary SpriteAtlasData class. If leaveOriginsRelative is true, origins will be left as 0 - 1 range instead
		/// of multiplying them by the width/height.
		/// </summary>
		public static SpriteAtlasData ParseSpriteAtlasData(string dataFile, bool leaveOriginsRelative = false)
		{
			var spriteAtlas = new SpriteAtlasData();

			var parsingSprites = true;
			var commaSplitter = new char[] { ',' };

			string line = null;
			using (var streamFile = File.OpenRead(dataFile))
			{
				using (var stream = new StreamReader(streamFile))
				{
					while ((line = stream.ReadLine()) != null)
					{
						// once we hit an empty line we are done parsing sprites so we move on to parsing animations
						if (parsingSprites && string.IsNullOrWhiteSpace(line))
						{
							parsingSprites = false;
							continue;
						}

						if (parsingSprites)
						{
							spriteAtlas.Names.Add(line);

							// source rect
							line = stream.ReadLine();
							var lineParts = line.Split(commaSplitter, StringSplitOptions.RemoveEmptyEntries);
							var rect = new Rectangle(int.Parse(lineParts[0]), int.Parse(lineParts[1]), int.Parse(lineParts[2]), int.Parse(lineParts[3]));
							spriteAtlas.SourceRects.Add(rect);

							// origin
							line = stream.ReadLine();
							lineParts = line.Split(commaSplitter, StringSplitOptions.RemoveEmptyEntries);
							var origin = new Vector2(float.Parse(lineParts[0], System.Globalization.CultureInfo.InvariantCulture), float.Parse(lineParts[1], System.Globalization.CultureInfo.InvariantCulture));

							if (leaveOriginsRelative)
								spriteAtlas.Origins.Add(origin);
							else
								spriteAtlas.Origins.Add(origin * new Vector2(rect.Width, rect.Height));
						}
						else
						{
							// catch the case of a newline at the end of the file
							if (string.IsNullOrWhiteSpace(line))
								break;

							spriteAtlas.AnimationNames.Add(line);

							// animation fps
							line = stream.ReadLine();
							spriteAtlas.AnimationFps.Add(int.Parse(line));

							// animation frames
							line = stream.ReadLine();
							var frames = new List<int>();
							spriteAtlas.AnimationFrames.Add(frames);
							var lineParts = line.Split(commaSplitter, StringSplitOptions.RemoveEmptyEntries);

							foreach (var part in lineParts)
								frames.Add(int.Parse(part));
						}
					}
				}
			}
			return spriteAtlas;
		}
	}

	[ContentImporter(".atlas", CacheImportedData = false, DefaultProcessor = "BinaryGZipProcessor", DisplayName = "SpriteAtlas Importer")]
	public class SpriteAtlasImporter : ContentImporter<byte[]>
	{
		public override byte[] Import(string filename, ContentImporterContext context)
		{
			
			//Parse data file
			var spriteAtlas = SpriteAtlasData.ParseSpriteAtlasData(filename);

			//Load texture
			spriteAtlas.RawTextureData = File.ReadAllBytes(filename.Replace(".atlas", ".png"));

			using (var m = new MemoryStream())
			{
				using (var b = new BinaryWriter(m))
				{
					//Write sprites
					b.Write(spriteAtlas.Names.Count);
					for(int i = 0; i < spriteAtlas.Names.Count; i++)
					{
						b.Write(spriteAtlas.Names[i]);
						b.Write(spriteAtlas.SourceRects[i].X);
						b.Write(spriteAtlas.SourceRects[i].Y);
						b.Write(spriteAtlas.SourceRects[i].Width);
						b.Write(spriteAtlas.SourceRects[i].Height);
						b.Write(spriteAtlas.Origins[i].X);
						b.Write(spriteAtlas.Origins[i].Y);
					}

					//Write animations
					b.Write(spriteAtlas.AnimationNames.Count);
					for (int i = 0; i < spriteAtlas.AnimationNames.Count; i++)
					{
						b.Write(spriteAtlas.AnimationNames[i]);
						b.Write(spriteAtlas.AnimationFps[i]);

						var frame = spriteAtlas.AnimationFrames[i];
						b.Write(frame.Count);
						for(int j = 0; j < frame.Count ; j++)
						{
							b.Write(frame[j]);
						}
					}

					//Write texture
					b.Write(spriteAtlas.RawTextureData);
				}

				var data = m.ToArray();
				context.Logger.LogMessage(data.Length.ToString());
				return data;
			}
		}
	}
}
