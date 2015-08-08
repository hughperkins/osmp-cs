using System;
namespace Metaverse.Scripting
{
		
	public interface IScriptCompiler 
	{
		IScriptGenerator Compile( IScriptPackage package );
	}

}
