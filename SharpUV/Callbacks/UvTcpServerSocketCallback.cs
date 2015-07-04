using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpUV.Callbacks
{
    internal class UvTcpServerSocketCallback : UvCallback<UvArgs<TcpServerSocket>>
    {
        internal UvTcpServerSocketCallback(object sender, Action<UvArgs<TcpServerSocket>> callback)
            : base(sender, callback)
        {
        }

        public void Invoke(TcpServerSocket value, Action<UvArgs<TcpServerSocket>> callback, EventHandler<UvArgs<TcpServerSocket>> handler)
        {
			base.Invoke(new UvArgs<TcpServerSocket>(0, value), callback, handler);
        }
    }
}
