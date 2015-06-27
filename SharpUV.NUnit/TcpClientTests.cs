using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace SharpUV.NUnit
{
    [TestFixture]
    public class TcpClientTests
    {
        [Test]
        public void GetGoogle()
        {
            UvWebClient.Get(new Uri("http://www.google.com"), (args) =>
            {
                Console.WriteLine("Page download result {0}\n{1}", args.IsSuccessful, args.Data);
            });

            GC.Collect();
            GC.Collect();
            Loop.Default.Run();

            Console.WriteLine("done");
        }

        private void TestInternal(string host, int port)
        {
            var client = new TcpClientSocket();

            client.Resolve(host, port.ToString(), (args1) =>
            {
                if (!args1.IsSuccesful)
                {
                    Console.WriteLine("failed to resolve host {0}", host);
                    return;
                }

                Console.WriteLine("host resolved to {0}", args1.Value);

                client.Connect(args1.Value[0], (args2) =>
                {
                    if (!args2.IsSuccesful)
                    {
                        Console.WriteLine("connection failed");
                        return;
                    }

                    Console.WriteLine("connection to {0} successful", args1.Value);

                    client.Close(false, (args3) =>
                    {
                        if (!args3.IsSuccesful)
                        {
                            Console.WriteLine("failed to close connection");
                            return;
                        }

                        Console.WriteLine("connection closed");
                    });
                });
            });
        }
    }

    public class UvWebClient : TcpClientSocket
    {
        private readonly Uri _uri;
        private readonly Action<UvWebClientGetArgs> _callback;
        private readonly Stream _data = new MemoryStream();

        private IPEndPoint[] _endpoints;
        private int _index = 0;

        public static void Get(Uri uri, Action<UvWebClientGetArgs> callback)
        {
            new UvWebClient(uri, callback).Start();
        }

        protected UvWebClient(Uri uri, Action<UvWebClientGetArgs> callback)
        {
            _uri = uri;
            _callback = callback;
        }

        private void Start()
        {
            this.Resolve(_uri.DnsSafeHost, _uri.Port.ToString());
        }

        protected override void OnResolve(UvIPEndPointArgs args)
        {
            if(!this.Check(args))
                return;

            _endpoints = args.Value;
            this.Connect(this.GetNextEndPoint());
        }

        private IPEndPoint GetNextEndPoint()
        {
            if (_index >= _endpoints.Length)
                return null;

            return _endpoints[_index++];
        }

        protected override void OnConnect(UvArgs args)
        {
            if(!args.IsSuccesful)
            {
                var endpoint = this.GetNextEndPoint();
                if(endpoint != null)
                {
                    this.Connect(endpoint);
                    return;
                }
            }

            if (!this.Check(args))
                return;

            this.Write(Encoding.UTF8.GetBytes(this.GetRequest()));
        }

        private string GetRequest()
        {
            var builder = new StringBuilder();

            builder.AppendFormat("GET {0} HTTP/1.1\r\n", _uri.PathAndQuery);
            builder.AppendFormat("Host: {0}\r\n", _uri.Host);
            builder.Append("\r\n");

            return builder.ToString();
        }

        protected override void OnWrite(UvDataArgs args)
        {
            if (!this.Check(args))
                return;

            this.ReadStart();
        }

        protected override void OnRead(UvDataArgs args)
        {
            if (!this.Check(args))
                return;

            _data.Write(args.Data, 0, args.Data.Length);
            this.Close();
        }

        protected override void OnClose(UvArgs args)
        {
            if (!this.Check(args))
                return;

            try
            {
                if (_callback != null)
                    _callback.Invoke(new UvWebClientGetArgs(null, this.GetResponse()));
            }
            finally 
            {
                this.Dispose();
            }
        }

        private string GetResponse()
        {
            _data.Position = 0;
            return new StreamReader(_data).ReadToEnd();
        }

        protected bool Check(UvArgs args)
        {
            if (args.IsSuccesful)
                return true;

            try
            {
                if (_callback != null)
                    _callback.Invoke(new UvWebClientGetArgs(args.Exception));
            }
            finally 
            {
                this.Dispose();
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            Console.WriteLine("disposing web client");
            base.Dispose(disposing);
        }
    }

    public class UvWebClientGetArgs : EventArgs
    {
        private readonly UvException _exception;
        private readonly string _data;

        public UvWebClientGetArgs(UvException exception, string data = null)
        {
            _exception = exception;
            _data = data;
        }

        public UvException Exception { get { return _exception; } }

        public bool IsSuccessful { get { return _exception == null; } }

        public string Data { get { return _data; } }
    }
}
