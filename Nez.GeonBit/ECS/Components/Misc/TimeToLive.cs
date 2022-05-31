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
// A component that destroy a game object after X seconds.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion

namespace Nez.GeonBit
{
	/// <summary>
	/// This component destroy game objects after given timer.
	/// </summary>
	public class TimeToLive : GeonComponent, IUpdatable
	{
		// how long left to live
		private float _timeToLive = 0f;

		/// <summary>
		/// Create the time to live component.
		/// </summary>
		/// <param name="timeToLive">How long to wait before destroying this object.</param>
		public TimeToLive(float timeToLive) => _timeToLive = timeToLive;

		/// <summary>
		/// Clone this component.
		/// </summary>
		/// <returns>Cloned copy of this component.</returns>
		public override Component Clone() => new TimeToLive(_timeToLive);

		/// <summary>
		/// Called every frame in the Update() loop.
		/// Note: this is called only if GameObject is enabled.
		/// </summary>
		public void Update()
		{
			_timeToLive -= Time.DeltaTime;
			if (_timeToLive <= 0f)
			{
				Entity.Destroy();
			}
		}
	}
}
