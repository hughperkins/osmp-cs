using System;
using Metaverse.Common;

namespace Metaverse.Scripting 
{
	public abstract class ACSScriptCompiler : IScriptCompiler 
	{
		//C# compiler related functions
		
		
		public abstract IScriptGenerator Compile( IScriptPackage package );
	}
}
