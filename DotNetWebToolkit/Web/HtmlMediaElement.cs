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
        public extern TimeRanges Buffered { get; }
        public extern bool Controls { get; set; }
        public extern string CurrentSrc { get; }
        public extern float CurrentTime { get; set; }
        public extern bool DefaultMuted { get; }
        public extern float DefaultPlaybackRate { get; }
        public extern float Duration { get; }
        public extern bool Ended { get; }
        public extern MediaError Error { get; }
        public extern bool Loop { get; set; }
        public extern bool Muted { get; set; }
        public extern NetworkState NetworkState { get; }
        public extern bool Paused { get; set; }
        public extern float PlaybackRate { get; set; }
        public extern TimeRanges Played { set; }
        public extern string Preload { get; set; }
        public extern ReadyState ReadyState { get; }
        public extern TimeRanges Seekable { get; }
        public extern bool Seeking { get; }
        public extern string Src { get; set; }
        public extern float StartTime { get; }
        public extern float Volume { get; set; }

        public extern string MediaGroup { get; set; }
        public extern MediaController Controller { get; set; }

        public extern void Load();
        public extern void Pause();
        public extern void Play();

        [JsDetail(IsDomEvent = true)]
        public extern Action OnLoadStart { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnProgress { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnSuspend { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnAbort { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnError { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnEmptied { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnStalled { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnLoadedMetaData { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnLoadedData { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnCanPlay { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnCanPlayThrough { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnPlaying { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnWaiting { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnSeeking { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnSeeked { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnEnded { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnDurationChanged { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnTimeUpdate { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnPlay { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnPause { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnRateChange { set; }
        [JsDetail(IsDomEvent = true)]
        public extern Action OnVolumeChange { set; }

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

    [JsClass("TimeRanges")]
    public class TimeRanges {
        public extern uint Length { get; }
        public extern double Start(uint index);
        public extern double End(uint index);
    }

    [JsClass("MediaError")]
    public class MediaError {
        public extern MediaErrorCode Code { get; }
    }

    public enum MediaErrorCode {
        MediaErrAborted = 1,
        MediaErrNetwork = 2,
        MediaErrDecode = 3,
        MediaErrSrcNotSupported = 4,
    }

    [JsClass("MediaController")]
    public class MediaController {
        public extern TimeRanges Buffered { get; }
        public extern TimeRanges Seekable { get; }
        public extern double Duration { get; }
        public extern double CurrentTime { get; set; }
        public extern bool Paused { get; }
        public extern TimeRanges Played { get; }
        public extern void Play();
        public extern void Pause();
        public extern double DefaultPlaybackRate { get; set; }
        public extern double PlaybackRate { get; set; }
        public extern double Volume { get; set; }
        public extern bool Muted { get; set; }

        [JsDetail(Name = "onemptied")]
        public extern Action OnEmptied { set; }
        [JsDetail(Name = "onloadedmetadata")]
        public extern Action OnLoadedMetaData { set; }
        [JsDetail(Name = "onloadeddata")]
        public extern Action OnLoadedData { set; }
        [JsDetail(Name = "oncanplay")]
        public extern Action OnCanPlay { set; }
        [JsDetail(Name = "oncanplaythrough")]
        public extern Action OnCanPlayThrough { set; }
        [JsDetail(Name = "onplaying")]
        public extern Action OnPlaying { set; }
        [JsDetail(Name = "onended")]
        public extern Action OnEnded { set; }
        [JsDetail(Name = "onwaiting")]
        public extern Action OnWaiting { set; }
        [JsDetail(Name = "ondurationchange")]
        public extern Action OnDurationChange { set; }
        [JsDetail(Name = "ontimeupdate")]
        public extern Action OnTimeUpdate { set; }
        [JsDetail(Name = "onplay")]
        public extern Action OnPlay { set; }
        [JsDetail(Name = "onpause")]
        public extern Action OnPause { set; }
        [JsDetail(Name = "onratechange")]
        public extern Action OnRateChange { set; }
        [JsDetail(Name = "onvolumechange")]
        public extern Action OnVolumeChange { set; }
    }

}
