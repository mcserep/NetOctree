// <copyright file="LogManager.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     
//     Copyright (c) 2014, Nition, http://www.momentstudio.co.nz/
//     Copyright (c) 2017, Máté Cserép, http://codenet.hu
//     All rights reserved.
// </copyright>
namespace Octree
{
    /// <summary>
    /// LogManager
    /// </summary>
    public class LogManager
    {
        /// <summary>
        /// Current <see cref="ILogger"/> implementation
        /// </summary>
        public static ILogger Current { get; set; }
        private static Logger Instance = new Logger();
        internal static Logger GetLogger()
        {
            return Instance;
        }
    }

    internal class Logger : ILogger
    {
        /// <summary>
        /// Writes the diagnostic message at the Info level.
        /// </summary>
        /// <param name="value"></param>
        public void Info(object value)
        {
            LogManager.Current?.Info(value);
        }
        /// <summary>
        /// Writes the diagnostic message at the Warn level.
        /// </summary>
        /// <param name="value"></param>
        public void Warn(object value)
        {
            LogManager.Current?.Warn(value);
        }
        /// <summary>
        /// Writes the diagnostic message at the Error level.
        /// </summary>
        /// <param name="value"></param>
        public void Error(object value)
        {
            LogManager.Current?.Error(value);
        }
    }

}
