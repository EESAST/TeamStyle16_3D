namespace GameStatics
{
	public static class Delegates
	{
		public delegate void EventHandler();

		public static EventHandler ScreenSizeChanged;
		public static EventHandler TeamColorChanged;
	}
}