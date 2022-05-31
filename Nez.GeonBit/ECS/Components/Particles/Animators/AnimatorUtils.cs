#region LICENSE
//-----------------------------------------------------------------------------
// For the purpose of making video games, educational projects or gamification,
// GeonBit is distributed under the MIT license and is totally free to use.
// To use this source code or GeonBit as a whole for other purposes, please seek 
// permission from the library author, Ronen Ness.
// 
// Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
// Do not remove this license notice.
//-----------------------------------------------------------------------------
#endregion
#region File Description
//-----------------------------------------------------------------------------
// Help functions and utilities for animators.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using System;

namespace Nez.GeonBit.Particles.Animators
{
	/// <summary>
	/// Misc animator related utilities.
	/// </summary>
	public static class AnimatorUtils
	{
		/// <summary>
		/// Random vector direction.
		/// </summary>
		public static Vector3 RandDirection(Vector3 baseVector, Vector3 randDir)
		{
			float originalLen = baseVector.Length();
			if (originalLen == 0f)
			{
				originalLen = 1f;
			}

			var newVelocity = baseVector;
			newVelocity.X += (Random.NextFloat() * (randDir.X * 2) - randDir.X);
			newVelocity.Y += (Random.NextFloat() * (randDir.Y * 2) - randDir.Y);
			newVelocity.Z += (Random.NextFloat() * (randDir.Z * 2) - randDir.Z);
			newVelocity.Normalize();
			return newVelocity * originalLen;
		}

		/// <summary>
		/// Random a vector from min and max.
		/// </summary>
		public static Vector3 RandVector(Vector3 minVector, Vector3 maxVector) => new Vector3(
					minVector.X + (Random.NextFloat() * (maxVector.X - minVector.X)),
					minVector.Y + (Random.NextFloat() * (maxVector.Y - minVector.Y)),
					minVector.Z + (Random.NextFloat() * (maxVector.Z - minVector.Z)));

		/// <summary>
		/// Random a vector from max vector only.
		/// </summary>
		public static Vector3 RandVector(Vector3 maxVector) => new Vector3(
					-maxVector.X + (Random.NextFloat() * (maxVector.X * 2f)),
					-maxVector.Y + (Random.NextFloat() * (maxVector.Y * 2f)),
					-maxVector.Z + (Random.NextFloat() * (maxVector.Z * 2f)));

		/// <summary>
		/// Random color value from base and rand color.
		/// </summary>
		public static Color RandColor(Color baseColor, Color colorJitter) => new Color(
					(byte)Math.Min(255, baseColor.R + Random.Range(0, colorJitter.R)),
					(byte)Math.Min(255, baseColor.G + Random.Range(0, colorJitter.G)),
					(byte)Math.Min(255, baseColor.B + Random.Range(0, colorJitter.B)));

		/// <summary>
		/// Random color value from min and max color values.
		/// </summary>
		public static Color RandColor2(Color minColor, Color maxColor) => new Color(
					Random.Range(minColor.R, maxColor.R),
					Random.Range(minColor.G, maxColor.G),
					Random.Range(minColor.B, maxColor.B));

		/// <summary>
		/// Calculate transition percent from current time and max time (return values from 0f to 1f).
		/// </summary>
		public static float CalcTransitionPercent(float timeAnimated, float maxTime) => Math.Min(timeAnimated / maxTime, 1f);
	}
}
