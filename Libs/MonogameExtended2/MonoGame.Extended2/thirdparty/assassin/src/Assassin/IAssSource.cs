namespace Assassin;

/// <summary>
/// A source that provides ASS subtitle data.
/// </summary>
public interface IAssSource
{

    AssTrack CreateTrack(AssLibrary library);

}
