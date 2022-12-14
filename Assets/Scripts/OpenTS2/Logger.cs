using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2
{
    /// <summary>
    /// Debugging class.
    /// </summary>
    public abstract class Logger
    {
        /// <summary>
        /// Get current Logger Singleton.
        /// </summary>
        /// <returns>Logger Singleton.</returns>
        public static Logger Get()
        {
            return s_instance;
        }

        /// <summary>
        /// Static Logger Singleton instance.
        /// </summary>
        static Logger s_instance;

        /// <summary>
        /// Construct a new Logger, make it the new Singleton.
        /// </summary>
        public Logger()
        {
            s_instance = this;
        }

        /// <summary>
        /// Assert a condition.
        /// </summary>
        /// <param name="condition">Condition to assert.</param>
        protected abstract void Assert_Impl(bool condition);

        /// <summary>
        /// Assert a condition.
        /// </summary>
        /// <param name="condition">Condition to assert.</param>
        [Conditional("DEBUG")] 
        public static void Assert(bool condition)
        {
            Get().Assert_Impl(condition);
        }
    }
}
