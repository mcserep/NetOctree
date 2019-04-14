// <copyright file="PointOctree.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     
//     Copyright (c) 2014, Nition, http://www.momentstudio.co.nz/
//     Copyright (c) 2017, Máté Cserép, http://codenet.hu
//     All rights reserved.
// </copyright>
namespace Octree
{
    using System.Collections.Generic;
    using NLog;

    /// <summary>
    /// A Dynamic Octree for storing any objects that can be described as a single point
    /// </summary>
    /// <seealso cref="BoundsOctree{T}"/>
    /// <remarks>
    /// Octree:	An octree is a tree data structure which divides 3D space into smaller partitions (nodes) 
    /// and places objects into the appropriate nodes. This allows fast access to objects
    /// in an area of interest without having to check every object.
    /// 
    /// Dynamic: The octree grows or shrinks as required when objects as added or removed.
    /// It also splits and merges nodes as appropriate. There is no maximum depth.
    /// Nodes have a constant - <see cref="PointOctree{T}.Node.NumObjectsAllowed"/> - which sets the amount of items allowed in a node before it splits.
    /// 
    /// See also BoundsOctree, where objects are described by AABB bounds.
    /// </remarks>
    /// <typeparam name="T">The content of the octree can be anything, since the bounds data is supplied separately.</typeparam>
    public partial class PointOctree<T> where T : class
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetLogger("octree");

        /// <summary>
        /// Root node of the octree
        /// </summary>
        private Node _rootNode;

        /// <summary>
        /// Size that the octree was on creation
        /// </summary>
        private readonly float _initialSize;

        /// <summary>
        /// Minimum side length that a node can be - essentially an alternative to having a max depth
        /// </summary>
        private readonly float _minSize;

	    /// <summary>
	    /// The total amount of objects currently in the tree
	    /// </summary>
	    public int Count { get; private set; }

	    /// <summary>
	    /// Gets the bounding box that represents the whole octree
	    /// </summary>
	    /// <value>The bounding box of the root node.</value>
	    public BoundingBox MaxBounds
	    {
		    get { return new BoundingBox(_rootNode.Center, new Point(_rootNode.SideLength, _rootNode.SideLength, _rootNode.SideLength)); }
	    }

		/// <summary>
		/// Constructor for the point octree.
		/// </summary>
		/// <param name="initialWorldSize">Size of the sides of the initial node. The octree will never shrink smaller than this.</param>
		/// <param name="initialWorldPos">Position of the centre of the initial node.</param>
		/// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this.</param>
		public PointOctree(float initialWorldSize, Point initialWorldPos, float minNodeSize)
        {
            if (minNodeSize > initialWorldSize)
            {
                Logger.Warn(
                    "Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize
                    + " Adjusted to: " + initialWorldSize);
                minNodeSize = initialWorldSize;
            }
            Count = 0;
            _initialSize = initialWorldSize;
            _minSize = minNodeSize;
            _rootNode = new Node(_initialSize, _minSize, initialWorldPos);
        }

        // #### PUBLIC METHODS ####

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objPos">Position of the object.</param>
        public void Add(T obj, Point objPos)
        {
            // Add object or expand the octree until it can be added
            int count = 0; // Safety check against infinite/excessive growth
            while (!_rootNode.Add(obj, objPos))
            {
                Grow(objPos - _rootNode.Center);
                if (++count > 20)
                {
                    Logger.Error(
                        "Aborted Add operation as it seemed to be going on forever (" + (count - 1)
                        + ") attempts at growing the octree.");
                    return;
                }
            }
            Count++;
        }

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj)
        {
            bool removed = _rootNode.Remove(obj);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <param name="objPos">Position of the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj, Point objPos)
        {
            bool removed = _rootNode.Remove(obj, objPos);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Return objects that are within <paramref name="maxDistance"/> of the specified ray.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="ray">The ray. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <returns>Objects within range.</returns>
        public T[] GetNearby(Ray ray, float maxDistance)
        {
            List<T> collidingWith = new List<T>();
            _rootNode.GetNearby(ref ray, ref maxDistance, collidingWith);
            return collidingWith.ToArray();
        }

        /// <summary>
        /// Return objects that are within <paramref name="maxDistance"/> of the specified position.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="position">The position. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <returns>Objects within range.</returns>
        public T[] GetNearby(Point position, float maxDistance)
        {
            List<T> collidingWith = new List<T>();
            _rootNode.GetNearby(ref position, ref maxDistance, collidingWith);
            return collidingWith.ToArray();
        }

        /// <summary>
        /// Return all objects in the tree.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <returns>All objects.</returns>
        public ICollection<T> GetAll()
        {
            List<T> objects = new List<T>(Count);
            _rootNode.GetAll(objects);
            return objects;
        }

        // #### PRIVATE METHODS ####

        /// <summary>
        /// Grow the octree to fit in all objects.
        /// </summary>
        /// <param name="direction">Direction to grow.</param>
        private void Grow(Point direction)
        {
            int xDirection = direction.X >= 0 ? 1 : -1;
            int yDirection = direction.Y >= 0 ? 1 : -1;
            int zDirection = direction.Z >= 0 ? 1 : -1;
            Node oldRoot = _rootNode;
            float half = _rootNode.SideLength / 2;
            float newLength = _rootNode.SideLength * 2;
            Point newCenter = _rootNode.Center + new Point(xDirection * half, yDirection * half, zDirection * half);

            // Create a new, bigger octree root node
            _rootNode = new Node(newLength, _minSize, newCenter);

            // Create 7 new octree children to go with the old root as children of the new root
            int rootPos = GetRootPosIndex(xDirection, yDirection, zDirection);
            Node[] children = new Node[8];
            for (int i = 0; i < 8; i++)
            {
                if (i == rootPos)
                {
                    children[i] = oldRoot;
                }
                else
                {
                    xDirection = i % 2 == 0 ? -1 : 1;
                    yDirection = i > 3 ? -1 : 1;
                    zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                    children[i] = new Node(
                        _rootNode.SideLength,
                        _minSize,
                        newCenter + new Point(xDirection * half, yDirection * half, zDirection * half));
                }
            }

            // Attach the new children to the new root node
            _rootNode.SetChildren(children);
        }

        /// <summary>
        /// Shrink the octree if possible, else leave it the same.
        /// </summary>
        private void Shrink()
        {
            _rootNode = _rootNode.ShrinkIfPossible(_initialSize);
        }

        /// <summary>
        /// Used when growing the octree. Works out where the old root node would fit inside a new, larger root node.
        /// </summary>
        /// <param name="xDir">X direction of growth. 1 or -1.</param>
        /// <param name="yDir">Y direction of growth. 1 or -1.</param>
        /// <param name="zDir">Z direction of growth. 1 or -1.</param>
        /// <returns>Octant where the root node should be.</returns>
        private static int GetRootPosIndex(int xDir, int yDir, int zDir)
        {
            int result = xDir > 0 ? 1 : 0;
            if (yDir < 0) result += 4;
            if (zDir > 0) result += 2;
            return result;
        }
    }
}