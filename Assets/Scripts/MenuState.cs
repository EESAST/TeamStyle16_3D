//#define FINAL

public enum MenuState
{
	None,
	Default,
#if FINAL
	StipulateTeams,
#else
	BrowsingFile,
#endif
	Options,
	About,
	Back,
	Quit
}