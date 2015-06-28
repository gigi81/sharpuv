using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpUV.Callbacks
{
    internal class UvEndPointsCallback : UvCallback<UvArgs<IPEndPoint[]>>
    {
        private IPEndPoint[] _value;

        internal UvEndPointsCallback(object sender, Action<UvArgs<IPEndPoint[]>> callback, IPEndPoint[] value = null)
            : base(sender, callback)
        {
            _value = value;
        }

        protected override UvArgs<IPEndPoint[]> CreateArgs(int code)
        {
            return new UvArgs<IPEndPoint[]>(code, _value);
        }

        public void Invoke(int code, IPEndPoint[] value, Action<UvArgs<IPEndPoint[]>> callback, EventHandler<UvArgs<IPEndPoint[]>> handler)
        {
            _value = value;
            base.Invoke(code, callback, handler);
        }
    }
}
