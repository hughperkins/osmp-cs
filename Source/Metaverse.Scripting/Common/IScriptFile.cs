using System;

namespace Metaverse.Scripting 
{
	public interface IScriptFile
	{
		ScriptFileType	Type { get; }
		string			Path { get; }

		byte[] GetData();
	}
}
