using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("WebSocket")]
    public class WebSocket {

        public extern WebSocket(string url);
        public extern WebSocket(string url, string protocols);
        public extern WebSocket(string url, string[] protocols);

        public extern string Url { get; }

        public extern WebSocketState ReadyState { get; }
        public extern int BufferedAmount { get; }

        public extern string Extensions { get; }
        public extern string Protocol { get; }
        public extern void Close();
        public extern void Close(ushort code);
        public extern void Close(ushort code, string reason);
        public extern string BinaryType { get; set; }
        public extern void Send(string data);
        public extern void Send(ArrayBuffer data);
        //public extern void Send(Blob data);

        [JsDetailAttribute(Name = "onmessage")]
        public extern Action<MessageEvent> OnMessage { set; }

    }

    public enum WebSocketState {
        Connecting = 0,
        Open = 1,
        Closing = 2,
        Closed = 3,
    }

}
