using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetWebToolkit.Attributes;

#pragma warning disable 0626, 0824

namespace DotNetWebToolkit.Web {

    [JsClass("VIDEO")]
    public class HtmlVideoElement : HtmlMediaElement {

        private HtmlVideoElement() { }

        /// <summary>
        /// Reflects the poster HTML attribute, which specifies an image to show while no video data is available.
        /// </summary>
        public extern string Poster { get; set; }

        /// <summary>
        /// The intrinsic height of the resource in CSS pixels, taking into account the dimensions, aspect ratio, clean aperture, resolution, and so forth, as defined for the format used by the resource. If the element's ready state is HAVE_NOTHING, the value is 0.
        /// </summary>
        public extern uint VideoHeight { get; }

        /// <summary>
        /// The intrinsic width of the resource in CSS pixels, taking into account the dimensions, aspect ratio, clean aperture, resolution, and so forth, as defined for the format used by the resource. If the element's ready state is HAVE_NOTHING, the value is 0.
        /// </summary>
        public extern uint VideoWidth { get; }

    }

}
