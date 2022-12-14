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
        public override void Assert(bool condition)
        {
            Debug.Assert(condition);
        }
    }
}
