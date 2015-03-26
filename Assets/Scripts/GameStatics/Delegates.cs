public static class Delegates
{
	public delegate void EventHandler();

	public static EventHandler CurrentTeamColorChanged;
	public static EventHandler GameStateChanged;
	public static EventHandler MarkPatternChanged;
	public static EventHandler MarkSizeChanged;
	public static EventHandler ScreenSizeChanged;
}