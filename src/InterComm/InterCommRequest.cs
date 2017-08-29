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

namespace Y56380X.InterComm
{
	public class InterCommRequest
	{
		[Newtonsoft.Json.JsonProperty(Required = Newtonsoft.Json.Required.Always)]
		public Enum Action { get; protected set; }
		public object RequestValue { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public object ResponseValue { get; protected set; }

		public InterCommRequest(Enum action, object requestValue)
		{
			Action = action;
			RequestValue = requestValue;

			ResponseValue = new InterCommJsonMessage<object>(InterCommClient.Current.Value.SendMessage(new InterCommJsonMessage<InterCommRequest>(this))).Data;
		}

		public InterCommRequest() { }
	}

	public class InterCommRequest<TAction, TValue> : InterCommRequest
	{
		[Newtonsoft.Json.JsonIgnore]
		public new TValue ResponseValue { get; private set; }

		[Newtonsoft.Json.JsonProperty(Required = Newtonsoft.Json.Required.Always)]
		public new TAction Action { get; private set; }

		public InterCommRequest(TAction action, object requestValue)
		{
			Action = action;
			RequestValue = requestValue;

			ResponseValue = new InterCommJsonMessage<TValue>(InterCommClient.Current.Value.SendMessage(new InterCommJsonMessage<InterCommRequest>(this))).Data;
		}

		public InterCommRequest() { }
	}
}
