using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Shouldly;
using Xunit;

namespace Octree.Tests
{
    public class PointOctreeTest
    {
        private PointOctree<int> _octree;

        /// <summary>
        /// Test setup.
        /// </summary>
        public PointOctreeTest()
        {
            _octree = new PointOctree<int>(50, Vector3.Zero, 1);
        }

        /// <summary>
        /// Constructor test.
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            _octree.Count.ShouldBe(0);
        }

        /// <summary>
        /// Tests the <see cref="PointOctree{T}.Add" /> method.
        /// </summary>
        [Fact]
        public void AddTest()
        {
            // Should add points within the tree's bound
            _octree.Add(1, new Vector3(5));
            _octree.Count.ShouldBe(1);

            _octree.Add(2, new Vector3(15));
            _octree.Count.ShouldBe(2);

            // Should add a point from outside the tree's bound
            _octree.Add(3, new Vector3(110, 110, 50));
            _octree.Count.ShouldBe(3);
        }

        /// <summary>
        /// Tests the <see cref="PointOctree{T}.GetNearby(Vector3,float)" /> method.
        /// </summary>
        [Fact]
        public void SearchByPositionTest()
        {
            // Add points
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new Vector3(i));

            // Get single point
            for (int i = 1; i < 100; ++i)
                _octree.GetNearby(new Vector3(i), 0).Length.ShouldBe(1);
            _octree.GetNearby(new Vector3(100), 0).Length.ShouldBe(0);

            // Should be empty for bounding boxes that do not contain any of the geometries
            _octree.GetNearby(new Vector3(0.5f), 0.2f).Length.ShouldBe(0);
            _octree.GetNearby(new Vector3(100), 1).Length.ShouldBe(0);
            _octree.GetNearby(new Vector3(200), 20).Length.ShouldBe(0);

            // Should find all geometries
            _octree.GetNearby(new Vector3(50), 100).Length.ShouldBe(_octree.Count);

            // Should find some geometries
            _octree.GetNearby(new Vector3(50), 10).Length.ShouldBe(11);
        }

        /// <summary>
        /// Tests the <see cref="PointOctree{T}.GetNearby(Octree.Ray,float)" /> method.
        /// </summary>
        [Fact]
        public void SearchByRayTest()
        {
            // Add points
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new Vector3(i));

            // Should find all geometries
            _octree.GetNearby(new Ray(Vector3.Zero, Vector3.One), 0).Length.ShouldBe(_octree.Count);
            _octree.GetNearby(new Ray(new Vector3(0, 0, 1), Vector3.One), 1).Length.ShouldBe(_octree.Count);

            // Should find no geometries
            _octree.GetNearby(new Ray(Vector3.Zero, Vector3.UnitX), 0).ShouldBeEmpty();
            _octree.GetNearby(new Ray(Vector3.Zero, Vector3.UnitY), 0).ShouldBeEmpty();
            _octree.GetNearby(new Ray(Vector3.Zero, Vector3.UnitZ), 0).ShouldBeEmpty();
            _octree.GetNearby(new Ray(new Vector3(0, 0, 2), Vector3.One), 1).ShouldBeEmpty();

            // Should find a single geometry
            _octree.GetNearby(new Ray(new Vector3(100, 0, 0), new Vector3(-1, 1, 1)), 0).Length.ShouldBe(1);
        }

        /// <summary>
        /// Tests the <see cref="PointOctree{T}.Remove(T)" /> method.
        /// </summary>
        [Fact]
        public void RemoveTest()
        {
            // Add points
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new Vector3(i));

            // Should not remove geometries that are not in the tree
            _octree.Remove(10, Vector3.Zero).ShouldBeFalse();
            _octree.Remove(10, new Vector3(5)).ShouldBeFalse();

            // Should remove geometries based on object
            for (int i = 1; i < 100; ++i)
                _octree.Remove(i).ShouldBeTrue();
            _octree.Count.ShouldBe(0);

            // Re-add points.
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new Vector3(i));

            // Should remove geometries based on object and bounding box
            for (int i = 1; i < 100; ++i)
                _octree.Remove(i, new Vector3(i)).ShouldBeTrue();
            _octree.Count.ShouldBe(0);
        }
    }
}
