using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetWebToolkit.WebGL {
    public enum TextureTarget {
        Texture2D = 0x0DE1,
        Texture= 0x1702,
        TextureCubeMap = 0x8513,
        TextureBindingCubeMap = 0x8514,
        TextureCubeMapPositiveX = 0x8515,
        TextureCubeMapNegativeX = 0x8516,
        TextureCubeMapPositiveY = 0x8517,
        TextureCubeMapNegativeY = 0x8518,
        TextureCubeMapPositiveZ = 0x8519,
        TextureCubeMapNegativeZ = 0x851A,
        MaxCubeMapTextureSize = 0x851C,
    }
}
