using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsAbstractClass]
    public abstract class HtmlMediaElement : HtmlElement {

        public extern bool AutoPlay { get; set; }
        // Buffered
        public extern bool Controls { get; set; }
        public extern string CurrentSrc { get; }
        public extern float CurrentTime { get; set; }
        public extern bool DefaultMuted { get; }
        public extern float DefaultPlaybackRate { get; }
        public extern float Duration { get; }
        public extern bool Ended { get; }
        // Error
        public extern bool Loop { get; set; }
        public extern bool Muted { get; set; }
        public extern NetworkState NetworkState { get; }
        public extern bool Paused { get; set; }
        public extern float PlaybackRate { get; set; }
        // Played
        public extern string Preload { get; set; }
        public extern ReadyState ReadyState { get; }
        // Seekable
        public extern bool Seeking { get; }
        public extern string Src { get; set; }
        public extern float StartTime { get; }
        public extern float Volume { get; set; }

        public extern string MediaGroup { get; set; }
        public extern MediaController Controller { get; set; }

        public extern void Load();
        public extern void Pause();
        public extern void Play();

        [JsDetail(Name = "onloadstart")]
        public extern Action OnLoadStart { set; }

    }

    public enum NetworkState {
        Empty = 0,
        Loading = 1,
        LoadedMetadata = 2,
        LoadedFirstFrame = 3,
        Loaded = 4,
    }

    public enum ReadyState {
        HaveNothing = 0,
        HaveMetadata = 1,
        HaveCurrentData = 2,
        HaveFutureData = 3,
        HaveEnoughData = 4,
    }

}
