using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;


namespace Nez
{
    /// <summary>
    /// provides frame timing information
    /// </summary>
    public static class Time
    {
        private static TimeMode _mode = TimeMode.LockedFramerate;
        public static TimeMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                switch (value)
                {
                    case TimeMode.Unlocked:
                    case TimeMode.LockedTimestep:
                        Core.Instance.IsFixedTimeStep = false;
                        break;
                    case TimeMode.LockedFramerate:
                        Core.Instance.IsFixedTimeStep = true;
                        break;
                }
            }
        }

        public static bool FirstUpdateInFrame;

        /// <summary>
        /// total time the game has been running
        /// </summary>
        public static float TotalTime;

        /// <summary>
        /// delta time from the previous frame to the current, scaled by timeScale(if mode is LockedFramerate)
        /// </summary>
        public static float DeltaTime;

        /// <summary>
        /// unscaled version of deltaTime. Not affected by timeScale
        /// </summary>
        public static float UnscaledDeltaTime;

        /// <summary>
        /// total time since the Scene was loaded
        /// </summary>
        public static float TimeSinceSceneLoad;

        /// <summary>
        /// time scale of deltaTime/TargetTimeStep
        /// </summary>
        public static float TimeScale = 1f;

        private static float _TargetTimeStep = 1 / 60F;
        public static float TargetTimeStep
        {
            get => _TargetTimeStep;
            set
            {
                if (Mode == TimeMode.LockedFramerate) Core.Instance.TargetElapsedTime = TimeSpan.FromSeconds(value);
                _TargetTimeStep = value;
            }
        }

        public static float Alpha;

        public static float MaxDeltaTime = 1F;

        /// <summary>
        /// total number of frames that have passed
        /// </summary>
        public static uint FrameCount;
        private static float lastTotalGameTime;
        private static float ScaledTimeStep;
        private static float accumulator;



        internal static void Prepare(GameTime gt)
        {
            TotalTime = (float)gt.TotalGameTime.TotalSeconds;
            UnscaledDeltaTime = (float)gt.ElapsedGameTime.TotalSeconds;
            lastTotalGameTime = TotalTime;
            DeltaTime = UnscaledDeltaTime * TimeScale;
            Alpha = 1;

            TimeSinceSceneLoad += UnscaledDeltaTime;
            FrameCount++;
        }

        internal static void Update(Action fixedUpdate, Action varUpdate, Action varSingle)
        {
            switch (Mode)
            {
                case TimeMode.Unlocked:
                case TimeMode.LockedFramerate:
                    DeltaTime = UnscaledDeltaTime * TimeScale;
                    FirstUpdateInFrame = true;
                    varSingle();
                    fixedUpdate();
                    varUpdate();
                    break;
                case TimeMode.LockedTimestep:
                    DeltaTime = _TargetTimeStep * TimeScale;
                    ScaledTimeStep = TargetTimeStep * TimeScale;
                    accumulator += Math.Min(UnscaledDeltaTime, MaxDeltaTime);
                    FirstUpdateInFrame = true;

                    varSingle();

                    bool DidUpdateHappen = accumulator >= ScaledTimeStep;
                    while (accumulator >= ScaledTimeStep)
                    {
                        fixedUpdate();
                        accumulator -= ScaledTimeStep;
                        FirstUpdateInFrame = false;
                    }
                    Alpha = (accumulator / ScaledTimeStep);

                    FirstUpdateInFrame = true;
                    varUpdate();
                    break;
            }
        }


        internal static void SceneChanged() => TimeSinceSceneLoad = 0f;


        /// <summary>
        /// Allows to check in intervals. Should only be used with interval values above deltaTime,
        /// otherwise it will always return true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckEvery(float interval) =>
            // we subtract deltaTime since timeSinceSceneLoad already includes this update ticks deltaTime
            (int)(TimeSinceSceneLoad / interval) > (int)((TimeSinceSceneLoad - DeltaTime) / interval);
    }

    public enum TimeMode
    {
        Unlocked, LockedFramerate, LockedTimestep
    }
}