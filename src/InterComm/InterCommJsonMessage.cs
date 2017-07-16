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

using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Y56380X.InterComm
{
	public static class InterCommJsonMessage
	{
		#region Properties

		private static readonly System.Collections.Generic.List<JsonConverter> customConverters = new System.Collections.Generic.List<JsonConverter>();
		internal static System.Collections.Generic.List<JsonConverter> CustomConverters
		{
			get
			{
				return customConverters;
			}
		}

		#endregion

		#region Methods

		public static void RegisterCustomConverter(JsonConverter customConverter)
		{
			// Add only new custom converters
			if (!customConverters.Exists(cjc => cjc.GetType() == customConverter.GetType()))
				customConverters.Add(customConverter);
		}

		public static T JConvert<T>(this object jObject)
		{
			if (jObject == null || !(jObject is JObject))
				return default(T);

			return ((JObject)jObject).ToObject<T>(JsonSerializer.Create(new JsonSerializerSettings { Converters = CustomConverters }));
		}

		#endregion
	}

	public class InterCommJsonMessage<T> : InterCommMessage
	{
		#region Fields

		private bool isDataCreated = false; // Flag to prevent deserializing everytime

		#endregion

		#region Properties

		private T data;
		public new T Data
		{
			get
			{
#if DEBUG
				System.Console.WriteLine("Deserialized: " + Encoding.Unicode.GetString(base.Data));
#endif

				// Deserialize only when neccessary
				return isDataCreated ? Data : (Data = JsonConvert.DeserializeObject<T>(Encoding.Unicode.GetString(base.Data), new JsonSerializerSettings { Converters = InterCommJsonMessage.CustomConverters }));
			}
			private set
			{
				data = value;
				isDataCreated = true;
			}
		}

		#endregion

		#region Constructors

		public InterCommJsonMessage() { }
		public InterCommJsonMessage(T data) : base(Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(data, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore, Converters = InterCommJsonMessage.CustomConverters })))
		{
#if DEBUG
			System.Console.WriteLine("Serialized: " + Encoding.Unicode.GetString(base.Data));
#endif

			Data = data;
		}
		public InterCommJsonMessage(InterCommMessage message) : base(message.Data) { }

		#endregion
	}
}
