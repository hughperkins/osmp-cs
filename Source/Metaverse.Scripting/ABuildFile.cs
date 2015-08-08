using System;
using Metaverse.Common;

namespace Metaverse.Scripting 
{
	public abstract class ABuildFile : IScriptFile 
	{
		
		public abstract string Path
	    {
	        get;
	    }
				
		public ScriptFileType	Type 
		{ 
			get{ return ScriptFileType.Build; }
		}

		public abstract byte[] GetData();
	}
}
