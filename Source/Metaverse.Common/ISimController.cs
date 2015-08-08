/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/16/2006
 * Time: 12:32 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;

namespace Metaverse.Common
{
	/// <summary>
	/// Description of ISimController.
	/// </summary>
	public interface ISimController
	{				
		ISim GetScriptGenericSimInterface( IScript script );
		ArrayList GetSimulatorScripts( ISim simulator );
		void InsertScript( ISim simulator, IScript script );
		IWorldModel GetSimulatorWorldModel( ISim simulator );
	}
}
