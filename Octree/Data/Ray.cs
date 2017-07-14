// <copyright file="Ray.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     
//     Copyright (c) 2014, Nition
//     Copyright (c) 2017, Máté Cserép
//     All rights reserved.
// </copyright>
namespace Octree
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Representation of rays.
    /// </summary>
    /// <remarks>
    /// A ray is an infinite line starting at <see cref="Origin"/> and going in some <see cref="Direction"/>.
    /// 
    /// This class was inspired by the Ray type of the Unity Engine and 
    /// designed with the exact same interface to provide maximum compatibility.
    /// </remarks>
    [DataContract]
    public struct Ray
    {
        /// <summary>
        /// Gets or sets the origin of the ray.
        /// </summary>
        [DataMember]
        public Point Origin { get; set; }

        /// <summary>
        /// The direction of the ray.
        /// </summary>
        [DataMember]
        private Point _direction;
        /// <summary>
        /// Gets or sets the direction of the ray.
        /// </summary>
        public Point Direction
        {
            get { return _direction; }
            set { _direction = value.Normalized; }
        }

        /// <summary>
        /// Creates a ray starting at origin along direction.
        /// </summary>
        /// <param name="origin">The origin of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        public Ray(Point origin, Point direction)
        {
            Origin = origin;
            _direction = direction.Normalized;
        }

        /// <summary>
        /// Returns a point at the given distance along the ray.
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <returns>The point on the ray.</returns>
        public Point GetPoint(float distance)
        {
            return Origin + Direction * distance;
        }

        /// <summary>
        /// Returns a nicely formatted string for this ray.
        /// </summary>
        public override string ToString()
        {
            return String.Format("Origin: {0}, Dir: {1}",
                Origin,
                Direction
            );
        }

        /// <summary>
        /// Returns a nicely formatted string for this ray.
        /// </summary>
        /// <param name="format">The format for the origin and direction.</param>
        public string ToString(string format)
        {
            return String.Format("Origin: {0}, Dir: {1}",
                Origin.ToString(format),
                Direction.ToString(format)
            );
        }
    }
}