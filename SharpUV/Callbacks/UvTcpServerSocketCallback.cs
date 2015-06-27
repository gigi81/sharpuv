using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpUV.Callbacks
{
    internal class UvTcpServerSocketCallback : UvCallback<UvArgs<TcpServerSocket>>
    {
        private TcpServerSocket _value;

        internal UvTcpServerSocketCallback(object sender, Action<UvArgs<TcpServerSocket>> callback, TcpServerSocket value = null)
            : base(sender, callback)
        {
            _value = value;
        }

        protected override UvArgs<TcpServerSocket> CreateArgs(int code)
        {
            return new UvArgs<TcpServerSocket>(code, _value);
        }

        public void Invoke(TcpServerSocket value, Action<UvArgs<TcpServerSocket>> callback, EventHandler<UvArgs<TcpServerSocket>> handler)
        {
            _value = value;
            base.Invoke(0, callback, handler);
        }
    }
}
