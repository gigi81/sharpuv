using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ByteArray = SharpUV.ImmutableArray.ImmutableArray<byte>;

namespace SharpUV.Test
{
	public class DataChecker
	{
		private ByteArray _sentData = ByteArray.Empty;
		private ByteArray _recvData = ByteArray.Empty;

		public int BytesSent { get; private set; }

		public int BytesReceived { get; private set; }

		public void Sent(byte[] data)
		{
			_sentData += data;
			this.BytesSent += data.Length;
		}

		public void Received(byte[] data)
		{
			_recvData += data;
			this.BytesReceived += data.Length;
		}

		public bool Check()
		{
			return true;
			//TODO
			//if (_recvData.Count > _sentData.Count)
			//	return false;

			//for (int i = 0; i < _recvData.Count; i++)
			//	if (_sentData[i] != _recvData[i])
			//		return false;

			//return true;
		}

		public void Flush()
		{
			_sentData = _sentData.SubArray(_recvData.Count);
			_recvData = ByteArray.Empty;
		}

		public bool IsEmpty
		{
			get { return _sentData.Count <= 0; }
		}
	}
}
