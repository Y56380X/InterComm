/*
	Copyright (c) 2016-2017 Y56380X

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Y56380X.InterComm
{
	public sealed class InterCommServer
	{
		#region Events

		private event EventHandler<InterCommEventArgs> messageReceived;
		public event EventHandler<InterCommEventArgs> MessageReceived
		{
			add
			{
				if (messageReceived == null)
					Start();

				messageReceived += value;
			}
			remove
			{
				messageReceived -= value;

				if (messageReceived == null)
					Stop();
			}
		}

		#endregion

		#region Fields

		public readonly static Lazy<InterCommServer> Current = new Lazy<InterCommServer>();

		private readonly Thread serverThread;
		private readonly TcpListener tcpListener;
		private bool isRunning;

		#endregion

		public InterCommServer()
		{
			serverThread = new Thread(ServerThreadMethod);
			tcpListener = new TcpListener(IPAddress.Loopback, GlobalConstants.InterCommPort);
		}

		private void Start()
		{
			isRunning = true;

			// Start InterCommServer-Thread
			serverThread.Start();
		}

		private void Stop()
		{
			isRunning = false;
		}

		private void ServerThreadMethod()
		{
#if DEBUG
			Console.WriteLine("InterComm Server started.");
#endif

			// Start TcpListener
			tcpListener.Start();

			var tcpConnections = new Dictionary<TcpClient, NetworkStream>();

			while (isRunning)
			{
				// Establish connection when pending
				if (tcpListener.Pending())
				{
					var tcpConnection = tcpListener.AcceptTcpClientAsync().Result;
					tcpConnections.Add(tcpConnection, tcpConnection.GetStream());
				}

				// Get data from clients
				if (tcpConnections.Any(tc => tc.Key.Connected && tc.Value.DataAvailable))
				{
					foreach (var connection in tcpConnections.Where(tc => tc.Key.Connected && tc.Value.DataAvailable))
					{
						// Get InterCommMessage
						var message = (InterCommMessage)connection.Value;

						// Throw "MessageReceived" event
						var eventArgument = new InterCommEventArgs(message);
						messageReceived?.Invoke(this, eventArgument);

						// Send result
						var resultBuffer = (byte[])(eventArgument.Result ?? new InterCommMessage());
						connection.Value.Write(resultBuffer, 0, resultBuffer.Length);
					}
				}
				else
				{
					Thread.Sleep(50);
				}

				// Clear disconnected clients
				foreach (var connection in tcpConnections.Where(tc => !tc.Key.Connected))
					tcpConnections.Remove(connection.Key);
			}
		}
	}

	public class InterCommMessage
	{
		internal int Lenght
		{
			get { return (Data?.Length) ?? 0; }
		}

		public byte[] Data { get; private set; }

		public InterCommMessage() { } // Use constructor only internal
		public InterCommMessage(byte[] data)
		{
			Data = data;
		}

		public static explicit operator InterCommMessage(NetworkStream stream)
		{
			var message = new InterCommMessage();

			var lengthBuffer = new byte[4];
			stream.Read(lengthBuffer, 0, lengthBuffer.Length);

			var lenght = BitConverter.ToInt32(lengthBuffer, 0);

			message.Data = new byte[lenght];
			stream.Read(message.Data, 0, lenght);

			return message;
		}

		public static explicit operator byte[] (InterCommMessage message)
		{
			// Get data lenght buffer
			var lenghtBuffer = BitConverter.GetBytes(message.Lenght);

			// Create message buffer
			var messageBuffer = new byte[lenghtBuffer.Length + message.Lenght];

			lenghtBuffer.CopyTo(messageBuffer, 0);
			message.Data?.CopyTo(messageBuffer, lenghtBuffer.Length);

			return messageBuffer;
		}
	}
}
