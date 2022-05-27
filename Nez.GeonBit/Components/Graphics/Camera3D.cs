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
// A 3d camera component.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Nez.GeonBit
{
    /// <summary>
    /// Camera types.
    /// </summary>
    public enum CameraType
    {
        /// <summary>
        /// Perspective camera.
        /// </summary>
        Perspective,

        /// <summary>
        /// Orthographic camera.
        /// </summary>
        Orthographic,
    };

    /// <summary>
    /// This component implements a 3d camera.
    /// </summary>
    public class Camera3D : Component, IUpdatable
    {

        protected GeonNode _camNode = new GeonNode();

        /// <summary>
        /// Default field of view.
        /// </summary>
        public static readonly float DefaultFieldOfView = MathHelper.PiOver4;

        // projection params
        float _fieldOfView = MathHelper.PiOver4;
        float _nearClipPlane = 1.0f;
        float _farClipPlane = 950.0f;
        float _aspectRatio = 1.0f;

        // current camera type
        CameraType _cameraType = CameraType.Perspective;

        // camera screen size
        Point? _altScreenSize = null;

        /// <summary>
        /// If defined, this will be used as screen size (affect aspect ratio in perspective camera,
        /// and view size in Orthographic camera). If not set, the actual screen resolution will be used.
        /// </summary>
        public Point? ForceScreenSize
        {
            get { return _altScreenSize; }
            set { _altScreenSize = value; }
        }

        /// <summary>
        /// Set / get camera type.
        /// </summary>
        public CameraType CameraType
        {
            set { _cameraType = value; _needUpdateProjection = true; }
            get { return _cameraType; }
        }

        /// <summary>
        /// Set / Get camera field of view.
        /// </summary>
        public float FieldOfView
        {
            get { return _fieldOfView; }
            set { _fieldOfView = value; _needUpdateProjection = true; }
        }

        /// <summary>
        /// Set / Get camera near clip plane.
        /// </summary>
        public float NearClipPlane
        {
            get { return _nearClipPlane; }
            set { _nearClipPlane = value; _needUpdateProjection = true; }
        }

        /// <summary>
        /// Set / Get camera far clip plane.
        /// </summary>
        public float FarClipPlane
        {
            get { return _farClipPlane; }
            set { _farClipPlane = value; _needUpdateProjection = true; }
        }

        // true if we need to update projection matrix next time we try to get it
        private bool _needUpdateProjection = true;

        // current view matrix
        Matrix _view;

        // current projection matrix
        Matrix _projection;

        // current world position
        Vector3 _position;

        /// <summary>
        /// Get camera position.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Return the current camera projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get { UpdateProjectionIfNeeded(); return _projection; }
        }

        /// <summary>
        /// Get / Set the current camera view matrix.
        /// </summary>
        public Matrix View
        {
            get { return _view; }
        }

        /// <summary>
        /// Get camera forward vector.
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                Vector3 ret = Vector3.Transform(Vector3.Forward, Matrix.Invert(View));
                ret.Normalize();
                return -ret;
            }
        }

        /// <summary>
        /// Get camera backward vector.
        /// </summary>
        public Vector3 Backward
        {
            get
            {
                Vector3 ret = Vector3.Transform(Vector3.Forward, Matrix.Invert(View));
                ret.Normalize();
                return ret;
            }
        }

        /// <summary>
        /// Get camera bounding frustum.
        /// </summary>
        public BoundingFrustum ViewFrustum
        {
            get
            {
                UpdateProjectionIfNeeded();
                return new BoundingFrustum(_view * _projection);
            }
        }

        /// <summary>
        /// Store camera world position.
        /// </summary>
        /// <param name="view">Current view matrix</param>
        /// <param name="position">Camera world position.</param>
        public void UpdateViewPosition(Matrix view, Vector3 position)
        {
            _view = view;
            _position = position;
        }

        /// <summary>
        /// Update projection matrix after changes.
        /// </summary>
        private void UpdateProjectionIfNeeded()
        {
            // if don't need update, skip
            if (!_needUpdateProjection)
            {
                return;
            }

            // screen width and height
            float width; float height;

            // if we have alternative screen size defined, use it
            if (ForceScreenSize != null)
            {
                width = ForceScreenSize.Value.X;
                height = ForceScreenSize.Value.Y;
            }
            // if we don't have alternative screen size defined, get current backbuffer size
            else
            {
                GraphicsDeviceManager deviceManager = GeonBitRenderer.GraphicsDeviceManager;
                width = deviceManager.PreferredBackBufferWidth;
                height = deviceManager.PreferredBackBufferHeight;
            }

            // calc aspect ratio
            _aspectRatio = width / height;

            // create view and projection matrix
            switch (_cameraType)
            {
                case CameraType.Perspective:
                    _projection = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearClipPlane, _farClipPlane);
                    break;

                case CameraType.Orthographic:
                    _projection = Matrix.CreateOrthographic(width, height, _nearClipPlane, _farClipPlane);
                    break;
            }

            // no longer need projection update
            _needUpdateProjection = false;
        }

        public override void OnAddedToEntity()
        {
            Entity.AddComponent(_camNode);

            // if there's no active camera, set self as the active camera
            if (GeonBitRenderer.ActiveCamera == null) SetAsActive();
        }

        /// <summary>
        /// Return a ray starting from the camera and pointing directly at mouse position (translated to 3d space).
        /// This is a helper function that help to get ray collision based on camera and mouse.
        /// </summary>
        /// <returns>Ray from camera to mouse.</returns>
        public Ray RayFromMouse()
        {
            MouseState mouseState = Mouse.GetState();
            return RayFrom2dPoint(new Vector2(mouseState.X, mouseState.Y));
        }

        /// <summary>
        /// Return a ray starting from the camera and pointing directly at a 3d position.
        /// </summary>
        /// <param name="point">Point to send ray to.</param>
        /// <returns>Ray from camera to given position.</returns>
        public Ray RayFrom3dPoint(Vector3 point)
        {
            return new Ray(Position, point - Position);
        }

        /// <summary>
        /// Return a ray starting from the camera and pointing directly at a 2d position translated to 3d space.
        /// This is a helper function that help to get ray collision based on camera and position on screen.
        /// </summary>
        /// <param name="point">Point to send ray to.</param>
        /// <returns>Ray from camera to given position.</returns>
        public Ray RayFrom2dPoint(Vector2 point)
        {
            // get graphic device
            GraphicsDevice device = Core.GraphicsDevice;

            // convert point to near and far points as 3d vectors
            Vector3 nearsource = new Vector3(point.X, point.Y, 0f);
            Vector3 farsource = new Vector3(point.X, point.Y, 1f);

            // create empty world matrix
            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            // convert near point to world space
            Vector3 nearPoint = device.Viewport.Unproject(nearsource,
                _projection, _view, world);

            // convert far point to world space
            Vector3 farPoint = device.Viewport.Unproject(farsource,
                _projection, _view, world);

            // get direction
            Vector3 dir = farPoint - nearPoint;
            dir.Normalize();

            // return ray
            return new Ray(nearPoint, dir);
        }

        /// <summary>
        /// If set, camera will always look at this point, regardless of scene node rotation.
        /// </summary>
        public Vector3? LookAt = null;

        /// <summary>
        /// Set a target that the camera will always look at, regardless of scene node rotation.
        /// Note: this override the LookAt position, even if set.
        /// </summary>
        public Entity LookAtTarget = null;

        /// <summary>
        /// If 'LookAtTarget' is used, this vector will be offset from target position.
        /// For example, if you want the camera to look at 5 units above target, set this to Vector3(0, 5, 0).
        /// </summary>
        public Vector3 LookAtTargetOffset = Vector3.Zero;

        /// <summary>
        /// Does this camera auto-update on update loop?
        /// Note: if you turn this false you must call Update() manually.
        /// </summary>
        public bool AutoUpdate = true;

        /// <summary>
        /// Get if this camera is the active camera in its scene.
        /// Note: it doesn't mean that the scene this camera belongs to is currently active.
        /// </summary>
        public bool IsActiveCamera
        {
            get
            {
                return GeonBitRenderer.ActiveCamera == this;
            }
        }

        /// <summary>
        /// Clone this component.
        /// </summary>
        /// <returns>Cloned copy of this component.</returns>
        override public Component Clone()
        {
            Camera3D ret = (Camera3D)Clone();
            ret.LookAt = LookAt;
            ret.LookAtTarget = LookAtTarget;
            ret.LookAtTargetOffset = LookAtTargetOffset;
            ret.CameraType = CameraType;
            ret.ForceScreenSize = ForceScreenSize;
            ret.FarPlane = FarPlane;
            ret.NearPlane = NearPlane;
            ret.FieldOfView = FieldOfView;
            ret.AutoUpdate = AutoUpdate;
            return ret;
        }

        /// <summary>
        /// Get the 3d ray that starts from camera position and directed at current mouse position.
        /// </summary>
        /// <returns>Ray from camera to mouse position.</returns>
        public Ray GetMouseRay()
        {
            return RayFromMouse();
        }

        /// <summary>
        /// Get the 3d ray that starts from camera position and directed at a given 2d position.
        /// </summary>
        /// <param name="position">Position to get ray to.</param>
        /// <returns>Ray from camera to given position.</returns>
        public Ray GetRay(Vector2 position)
        {
            return RayFrom2dPoint(position);
        }

        /// <summary>
        /// Get the 3d ray that starts from camera position and directed at a given 3d position.
        /// </summary>
        /// <param name="position">Position to get ray to.</param>
        /// <returns>Ray from camera to given position.</returns>
        public Ray GetRay(Vector3 position)
        {
            return RayFrom3dPoint(position);
        }


        /// <summary>
        /// Set this camera as the currently active camera.
        /// </summary>
        public void SetAsActive()
        {
            // if not in scene, throw exception
            //if (_GameObject == null || _GameObject.ParentScene == null)
            //{
            //    throw new System.InvalidOperationException("Cannot make a camera active when its not under any scene!");
            //}

            // update core graphics about new active camera
            GeonBitRenderer.ActiveCamera = this;
        }

        /// <summary>
        /// Called every frame in the Update() loop.
        /// Note: this is called only if GameObject is enabled.
        /// </summary>
        public void Update()
        {
            // if we are the currently active camera, update view matrix
            if (IsActiveCamera && AutoUpdate)
            {
                // update camera view
                UpdateCameraView();
            }
        }

        /// <summary>
        /// Update camera view matrix.
        /// </summary>
        public void UpdateCameraView()
        {
            // if there's a lookat target, override current LookAt
            if (LookAtTarget != null)
            {
                LookAt = LookAtTarget.GetComponent<GeonNode>().WorldPosition + LookAtTargetOffset;
            }

            // new view matrix
            Matrix view;

            // get current world position (of the camera)
            Vector3 worldPos = _camNode.WorldPosition;

            // if we have lookat-target, create view from look-at matrix.
            if (LookAt != null)
            {
                view = Matrix.CreateLookAt(worldPos, (Vector3)LookAt, Vector3.Up);
            }
            // if we don't have a look-at target, create view matrix from scene node transformations
            else
            {
                Vector3 target = worldPos + Vector3.Transform(Vector3.Forward, _camNode.WorldRotation);
                view = Matrix.CreateLookAt(worldPos, target, Vector3.Up);
            }

            // update the view matrix of the graphic camera component
            UpdateViewPosition(view, worldPos);
        }
    }
}
