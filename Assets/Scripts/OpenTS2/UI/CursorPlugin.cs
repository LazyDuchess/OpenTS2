//-----------------------------------------------------------------
//	Hardware Cursor Plugin for Unity3d. Version 0.1 (11.11.2010)
//  Copyright (c) 2010 Maciej Czerwonka and Sleepwalker Games
//	All rights reserved
//-----------------------------------------------------------------

using System.Runtime.InteropServices;

namespace OpenTS2.UI
{
	public class HardwareCursors
	{
		[DllImport("CursorPlugin")]
		public static extern void SetCurrentCursor(int id);

		[DllImport("CursorPlugin")]
		public static extern void InitializeCursor(int id, string path);
	}
}


