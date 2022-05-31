using Microsoft.Xna.Framework;

namespace Nez.GeonBit
{
	public enum NodeType
	{
		/// <summary>
		/// A simple node without any culling (will always draw, unless parent is culled).
		/// </summary>
		Simple,

		/// <summary>
		/// Scene node that cull using bounding-box and camera frustom.
		/// </summary>
		BoundingBoxCulling,

		/// <summary>
		/// Scene node that cull using bounding-sphere and camera frustom. TBD
		/// </summary>
		BoundingSphereCulling,

		/// <summary>
		/// Scene node to use for particles. TBD
		/// </summary>
		ParticlesNode,

		/// <summary>
		/// Scene node with octree culling. TBD
		/// </summary>
		OctreeCulling,
	}

	public class GeonScene : Scene
	{
		public new Camera3D Camera;


		/// <summary>
		/// Bounding box to use for octree culling.
		/// If you use octree culling scene node, objects that exceed this bounding box won't be culled properly.
		/// </summary>
		public static BoundingBox OctreeSceneBoundaries = new BoundingBox(Vector3.One * -1000, Vector3.One * 1000);

		/// <summary>
		/// How many times we can divide octree nodes, until reaching the minimum bounding box size.
		/// </summary>
		public static uint OctreeMaxDivisions = 5;


		public GeonScene() : base(false)
		{
			//Add camera
			var cameraEntity = CreateEntity("camera");
			Camera = cameraEntity.AddComponent(new Camera3D());

			Initialize();
		}

		public GeonEntity CreateGeonEntity(string name, Vector3 position, NodeType nodeType = NodeType.Simple)
		{
			var entity = AddEntity(new GeonEntity(name));

			switch (nodeType)
			{

				// scene node with bounding-box culling
				case NodeType.BoundingBoxCulling:
					entity.Node = new BoundingBoxCullingNode();
					break;

				// scene node with bounding-sphere culling
				case NodeType.BoundingSphereCulling:
					entity.Node = new BoundingSphereCullingNode();
					break;

				// scene node optimized for particles
				case NodeType.ParticlesNode:
					entity.Node = new ParticleNode();
					break;

				// scene node with octree-based culling
				case NodeType.OctreeCulling:
					entity.Node = new OctreeCullingNode(OctreeSceneBoundaries, OctreeMaxDivisions);
					break;
				// a simple scene node without culling
				default:
					entity.Node = new Node();
					break;
			}

			entity.Node.Position = position;

			entity.AddComponent(entity.Node);
			return entity;
		}

		public Entity CreateGeonEntity(string name, NodeType nodeType = NodeType.Simple) => CreateGeonEntity(name, Vector3.Zero, nodeType);
	}
}
