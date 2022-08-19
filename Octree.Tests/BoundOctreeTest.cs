// <copyright file="BoundOctreeTest.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     Copyright (c) 2022, Máté Cserép, https://codenet.hu
//     All rights reserved.
// </copyright>

namespace Octree.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using Shouldly;
    using Xunit;

    public class BoundOctreeTest
    {
        private BoundsOctree<int> _octree;

        /// <summary>
        /// Test setup.
        /// </summary>
        public BoundOctreeTest()
        {
            _octree = new BoundsOctree<int>(50, Vector3.Zero, 1, 1.0f);
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
        /// Tests the <see cref="BoundsOctree{T}.Add" /> method.
        /// </summary>
        [Fact]
        public void AddTest()
        {
            // Should add different types geometry within the tree's bound
            _octree.Add(1, new BoundingBox(new Vector3(5), Vector3.Zero));
            _octree.Count.ShouldBe(1);

            _octree.Add(2, new BoundingBox(new Vector3(5), new Vector3(10, 10, 20)));
            _octree.Count.ShouldBe(2);

            // Should add a geometry from outside the tree's bound
            _octree.Add(3, new BoundingBox(new Vector3(110, 110, 50), Vector3.Zero));
            _octree.Count.ShouldBe(3);
        }

        /// <summary>
        /// Tests the <see cref="BoundsOctree{T}.IsColliding" /> method.
        /// </summary>
        [Fact]
        public void CollisionTest()
        {
            // Add points
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new BoundingBox(new Vector3(i), Vector3.Zero));

            // Check collision
            for (int i = 1; i < 100; ++i)
                _octree.IsColliding(new BoundingBox(new Vector3(i), Vector3.Zero))
                    .ShouldBeTrue();

            // Check no collision
            _octree.IsColliding(new BoundingBox(new Vector3(100), Vector3.Zero))
                .ShouldBeFalse();

            // Add polygon
            _octree.Add(100, new BoundingBox(new Vector3(5), new Vector3(10, 10, 20)));

            // Check collision
            _octree.IsColliding(new BoundingBox(new Vector3(15), new Vector3(10)))
                .ShouldBeTrue();

            // Check no collision
            _octree.IsColliding(new BoundingBox(new Vector3(100), Vector3.One))
                .ShouldBeFalse();
        }

        /// <summary>
        /// Tests the <see cref="BoundsOctree{T}.GetColliding(List{T},Octree.BoundingBox)" /> method.
        /// </summary>
        [Fact]
        public void SearchByPositionTest()
        {
            // Add points
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new BoundingBox(new Vector3(i), Vector3.Zero));

            // Should be empty for bounding boxes that do not contain any of the geometries
            List<int> result = new List<int>();
            _octree.GetColliding(result, new BoundingBox(new Vector3(0.5f), new Vector3(0.2f)));
            result.ShouldBeEmpty();
            _octree.GetColliding(result, new BoundingBox(new Vector3(100), Vector3.One));
            result.ShouldBeEmpty();
            _octree.GetColliding(result, new BoundingBox(new Vector3(200), new Vector3(20)));
            result.ShouldBeEmpty();

            // Should find all geometries
            _octree.GetColliding(result, new BoundingBox(new Vector3(50), new Vector3(100)));
            result.Count.ShouldBe(_octree.Count);
            result.Clear();

            // Should find some geometries
            _octree.GetColliding(result, new BoundingBox(new Vector3(50), new Vector3(50)));
            result.Count.ShouldBe(51);
        }

        /// <summary>
        /// Tests the <see cref="BoundsOctree{T}.GetColliding(List{T},Octree.Ray,float)" /> method.
        /// </summary>
        [Fact]
        public void SearchByRayTest()
        {
            // Add points
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new BoundingBox(new Vector3(i), Vector3.Zero));

            // Should find some geometries (distance measured from origin of ray)
            List<int> result = new List<int>();
            _octree.GetColliding(result, new Ray(Vector3.Zero, Vector3.One), 2);
            result.Count.ShouldBe(1);
            result.Clear();
            _octree.GetColliding(result, new Ray(Vector3.Zero, Vector3.One), 5);
            result.Count.ShouldBe(2);
            result.Clear();
            _octree.GetColliding(result, new Ray(new Vector3(50), Vector3.One), 5);
            result.Count.ShouldBe(3);
            result.Clear();

            // Should find no geometries
            _octree.GetColliding(result, new Ray(new Vector3(50), Vector3.UnitX), 2);
            _octree.GetColliding(result, new Ray(new Vector3(50), Vector3.UnitY), 2);
            _octree.GetColliding(result, new Ray(new Vector3(50), Vector3.UnitZ), 2);
            result.ShouldBeEmpty();
        }

        /// <summary>
        /// Tests the <see cref="BoundsOctree{T}.Remove" /> method.
        /// </summary>
        [Fact]
        public void RemoveTest()
        {
            // Add points
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new BoundingBox(new Vector3(i), Vector3.One));

            // Should not remove geometries that are not in the tree
            _octree.Remove(10, new BoundingBox(Vector3.Zero, Vector3.One)).ShouldBeFalse();
            _octree.Remove(10, new BoundingBox(Vector3.One, new Vector3(5))).ShouldBeFalse();

            // Should remove geometries based on object
            for (int i = 1; i < 100; ++i)
                _octree.Remove(i).ShouldBeTrue();
            _octree.Count.ShouldBe(0);

            // Re-add points.
            for (int i = 1; i < 100; ++i)
                _octree.Add(i, new BoundingBox(new Vector3(i), Vector3.One));

            // Should remove geometries based on object and bounding box
            for (int i = 1; i < 100; ++i)
                _octree.Remove(i, new BoundingBox(new Vector3(i), new Vector3(0.5f))).ShouldBeTrue();
            _octree.Count.ShouldBe(0);
        }
    }
}
