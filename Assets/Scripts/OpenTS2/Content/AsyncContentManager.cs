using OpenTS2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTS2.Content
{
    public class AsyncContentManager
    {
        public class AssetRequest
        {
            public bool Finished = false;
            public Results Result;
            public AbstractAsset Asset;
        }
        public enum Results
        {
            Success,
            Failed
        }
        public static AsyncContentManager Instance { get; private set; }
        private Thread _thread;
        private Queue<Action> _requests = new Queue<Action>();
        private ContentManager _contentManager;
        public AsyncContentManager()
        {
            Instance = this;
            _contentManager = ContentManager.Instance;
            _thread = new Thread(new ThreadStart(ThreadLoop));
            _thread.IsBackground = true;
            _thread.Name = "AsyncContentLoader";
            _thread.Start();
        }

        public AssetRequest RequestAsset(ResourceKey key)
        {
            var requestResult = new AssetRequest();
            _requests.Enqueue(() =>
            {
                var asset = _contentManager.GetAsset(key);
                requestResult.Asset = asset;
                requestResult.Finished = true;
                requestResult.Result = asset != null ? Results.Success : Results.Failed;
            });
            _thread.Interrupt();
            return requestResult;
        }

        private void ThreadLoop()
        {
            while (true)
            {
                if (_requests.Count > 0)
                {
                    var request = _requests.Dequeue();
                    request.Invoke();
                }
                else
                {
                    try
                    {
                        Thread.Sleep(Timeout.Infinite);
                    }
                    catch (ThreadInterruptedException) { }
                }
            }
        }
    }
}
