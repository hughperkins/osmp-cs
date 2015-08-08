/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/14/2006
 * Time: 12:29 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Metaverse.Common;
using System.Collections;


namespace Metaverse.Scripting.Testing
{
	/// <summary>
	/// Description of SimController.
	/// </summary>
	public class SimController : DummySimController
	{
		private static SimController _singleton = null;
		
		public static ISimController Singleton {
			get 
			{
				lock( _singleton ) {
					if( _singleton == null ) {
						_singleton = new SimController();
					}
				}
				return _singleton;
			}
		}
		
		new public ISim GetScriptGenericSimInterface( IScript script ) {
			
			ArrayList simulators = Controller.Metaverse.GetSimulators();
			
			ISim sim = (ISim)simulators[0];
			
			if( sim == null )  {
				throw new Exception( "Simulator is not attached" );
			}
						
			return sim;
		}
		
		
		new public ArrayList GetSimulatorScripts( ISim simulator ) 
		{
			return Controller.Metaverse.GetSimServer( simulator ).GetScripts();
		}
		
		new public void InsertScript( ISim simulator, IScript script )
		{
			MetaverseController.Singleton.GetSimServer( simulator ).InsertScript( script );
		}
		
		new public IWorldModel GetSimulatorWorldModel( ISim simulator ) {
			return MetaverseController.Singleton.GetSimServer( simulator ).GetWorldModel( simulator );
		}
	}
}
