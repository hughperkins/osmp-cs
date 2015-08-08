/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/16/2006
 * Time: 12:30 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Metaverse.Common;
using System.Collections;


namespace Metaverse.Scripting.Testing
{
	/// <summary>
	/// Description of ClientController.
	/// </summary>
	public class ClientController : DummyClientController
	{
			private static ClientController _singleton = null;

		
		public static ClientController Singleton {
			get 
			{
				lock( _singleton ) {
					if( _singleton == null ) {
						_singleton = new ClientController();
					}
				}
				return _singleton;
			}
		}
			
		private ClientController() {}
		
		new public void FileInsertNewSingleFileScript(string filename, string file)
		{
			
			CSScriptFile csfile = new CSScriptFile( filename, file );
			
			SingleFileScriptPackage package = new SingleFileScriptPackage( csfile );
	    	IScriptGenerator generator = package.ExecuteCompiler( new SingleFileProjectCompiler() );
	    	
	    	ArrayList simulators = MetaverseController.Singleton.GetSimulators();
	    	ISim sim = (ISim)simulators[0];
	    	
	    	SimController.Singleton.InsertScript( sim, generator.Generate() );
	    	
	    	return;
		}
	}
}
