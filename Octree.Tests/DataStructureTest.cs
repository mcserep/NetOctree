// <copyright file=" DataStructureTest.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     Copyright (c) 2022, Máté Cserép, https://codenet.hu
//     All rights reserved.
// </copyright>

namespace Octree.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Text;
    using Shouldly;
    using Xunit;

    public class DataStructureTest
    {
        /// <summary>
        /// Tests methods <see cref="Ray"/> class.
        /// </summary>
        [Fact]
        public void RayTest()
        {
            Ray ray = new Ray(Vector3.UnitX, Vector3.One);
            ray.Origin.ShouldBe(Vector3.UnitX);
            ray.Direction.ShouldBe(Vector3.Normalize(Vector3.One));
            ray.GetPoint(2).ShouldBe(Vector3.UnitX + Vector3.Normalize(Vector3.One) * 2);
        }

        /// <summary>
        /// Tests methods <see cref="BoundingBox"/> class.
        /// </summary>
        [Fact]
        public void BoundingBoxTest()
        {
            // Test ctor + getters
            BoundingBox box = new BoundingBox(Vector3.One, new Vector3(1, 2, 3));
            box.Center.ShouldBe(Vector3.One);
            box.Extents.ShouldBe(new Vector3(0.5f, 1f, 1.5f));
            box.Min.ShouldBe(new Vector3(0.5f, 0f, -0.5f));
            box.Max.ShouldBe(new Vector3(1.5f, 2f, 2.5f));

            // Test Encapsulate()
            box.Encapsulate(new Vector3(5, 0, 0));
            box.Center.ShouldBe(new Vector3(2.75f, 1f, 1f));
            box.Extents.ShouldBe(new Vector3(2.25f, 1f, 1.5f));
            box.Min.ShouldBe(new Vector3(0.5f, 0f, -0.5f));
            box.Max.ShouldBe(new Vector3(5f, 2f, 2.5f));

            // Test Expand()
            box.Expand(1);
            box.Center.ShouldBe(new Vector3(2.75f, 1f, 1f));
            box.Extents.ShouldBe(new Vector3(2.75f, 1.5f, 2f));
            box.Min.ShouldBe(new Vector3(0f, -0.5f, -1f));
            box.Max.ShouldBe(new Vector3(5.5f, 2.5f, 3f));

            // Test SetMinMax()
            box.SetMinMax(new Vector3(-1), new Vector3(3));
            box.Center.ShouldBe(Vector3.One);
            box.Extents.ShouldBe(new Vector3(2));
            box.Min.ShouldBe(new Vector3(-1));
            box.Max.ShouldBe(new Vector3(3));

            // Test Contains()
            box.Contains(Vector3.Zero).ShouldBeTrue();
            box.Contains(new Vector3(3)).ShouldBeTrue();
            box.Contains(new Vector3(4)).ShouldBeFalse();
            box.Contains(new Vector3(3, 3, 3.1f)).ShouldBeFalse();

            // Test Intersects()
            box.Intersects(new BoundingBox(new Vector3(4), Vector3.One)).ShouldBeFalse();
            box.Intersects(new BoundingBox(new Vector3(4), new Vector3(2))).ShouldBeTrue(); // touches
            box.Intersects(new BoundingBox(new Vector3(4), new Vector3(3))).ShouldBeTrue();
        }

        /// <summary>
        /// Tests the <see cref="BoundingBox.IntersectRay(Octree.Ray)"/> method.
        /// </summary>
        [Fact]
        public void BoundingBoxRayIntersectionTest()
        {
            Ray ray = new Ray(Vector3.UnitX, Vector3.One);
            new BoundingBox(new Vector3(3), new Vector3(0.5f)).IntersectRay(ray).ShouldBeFalse();
            new BoundingBox(new Vector3(3), new Vector3(0.9f)).IntersectRay(ray).ShouldBeFalse();
            new BoundingBox(new Vector3(3), new Vector3(1f)).IntersectRay(ray).ShouldBeTrue(); // touches
            new BoundingBox(new Vector3(3), new Vector3(2f)).IntersectRay(ray).ShouldBeTrue();
        }
    }
}
