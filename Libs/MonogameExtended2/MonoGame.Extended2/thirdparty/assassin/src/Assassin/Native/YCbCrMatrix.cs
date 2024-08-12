using System.Diagnostics.CodeAnalysis;

namespace Assassin.Native;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum YCbCrMatrix
{

    Default = 0,

    Unknown = 1,

    None = 2,

    BT601_TV,

    BT601_PC,

    BT709_TV,

    BT709_PC,

    Smpte240M_TV,

    Smpte240M_PC,

    Fcc_TV,

    Fcc_PC

}
