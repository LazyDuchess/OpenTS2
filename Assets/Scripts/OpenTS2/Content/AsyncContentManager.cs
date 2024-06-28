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
        public class Request
        {
            public bool Finished = false;
            public Results Result;
            public ResourceKey Key;
            public AbstractAsset Asset;
        }
        public enum Results
        {
            Success,
            Failed
        }
        public static AsyncContentManager Instance { get; private set; }
        private Thread _thread;
        private Queue<Request> _requests = new Queue<Request>();
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

        public Request RequestAsset(ResourceKey key)
        {
            var request = new Request();
            request.Key = key;
            _requests.Enqueue(request);
            return request;
        }

        private void ThreadLoop()
        {
            while (true)
            {
                if (_requests.Count > 0)
                {
                    var request = _requests.Peek();
                    request.Asset = _contentManager.GetAsset(request.Key);
                    request.Result = request.Asset != null ? Results.Success : Results.Failed;
                    request.Finished = true;
                    _requests.Dequeue();
                }
                else
                    Thread.Sleep(1);
            }
        }
    }
}
