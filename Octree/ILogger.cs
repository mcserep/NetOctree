// <copyright file="ILogger.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     
//     Copyright (c) 2014, Nition, http://www.momentstudio.co.nz/
//     Copyright (c) 2017, Máté Cserép, http://codenet.hu
//     All rights reserved.
// </copyright>
namespace Octree
{
    /// <summary>
    /// Simple ILogger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes the diagnostic message at the Info level.
        /// </summary>
        /// <param name="value"></param>
        void Info(object value);
        /// <summary>
        /// Writes the diagnostic message at the Warn level.
        /// </summary>
        /// <param name="value"></param>
        void Warn(object value);
        /// <summary>
        /// Writes the diagnostic message at the Error level.
        /// </summary>
        /// <param name="value"></param>
        void Error(object value);
    }

}
