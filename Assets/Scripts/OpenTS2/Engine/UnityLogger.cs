using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Engine
{
    /// <summary>
    /// Unity implementation of Debug Logging.
    /// </summary>
    public class UnityLogger : Logger
    {
        protected override void Assert_Impl(bool condition)
        {
            Debug.Assert(condition);
        }
    }
}
