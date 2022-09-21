using OpenTS2.Unity.Core;
using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Unity.Core
{
    public static class ResourceManagement
    {
        public static void Initialize()
        {
            new UnityContentManager();
        }
    }
}
