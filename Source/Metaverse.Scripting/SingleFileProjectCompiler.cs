/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/13/2006
 * Time: 11:02 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Metaverse.Common;

namespace Metaverse.Scripting
{
	/// <summary>
	/// Description of SingleFileProjectCompiler.
	/// </summary>
	public class SingleFileProjectCompiler: ACSScriptCompiler
	{
		public override IScriptGenerator Compile( IScriptPackage package ) {
			
			string compiledAssemblyPath = string.Empty;
			
			return new AssemblyScriptGenerator( compiledAssemblyPath );
		}
	}
}
