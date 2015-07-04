using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SharpUV.Callbacks
{
    internal class UvEndPointsCallback : UvCallback<UvArgs<IPEndPoint[]>>
    {
        internal UvEndPointsCallback(object sender, Action<UvArgs<IPEndPoint[]>> callback)
            : base(sender, callback)
        {
        }

        public void Invoke(int code, IPEndPoint[] value, Action<UvArgs<IPEndPoint[]>> callback, EventHandler<UvArgs<IPEndPoint[]>> handler)
        {
			base.Invoke(new UvArgs<IPEndPoint[]>(code, value), callback, handler);
        }
    }
}
