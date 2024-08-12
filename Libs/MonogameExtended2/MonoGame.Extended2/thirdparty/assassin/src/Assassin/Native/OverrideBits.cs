using System;

namespace Assassin.Native;

[Flags]
public enum OverrideBits
{

    Default = 0,

    Style = 1 << 0,

    SelectiveFontScale = 1 << 1,

    FontSize = 1 << 1,

    FontSizeFields = 1 << 2,

    FontName = 1 << 3,

    Colors = 1 << 4,

    Attributes = 1 << 5,

    Border = 1 << 6,

    Alignment = 1 << 7,

    Margins = 1 << 8,

    FullStyle = 1 << 9,

    Justify = 1 << 10

}
