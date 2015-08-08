/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/16/2006
 * Time: 12:35 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Metaverse.Common;
using System.Collections;

namespace Metaverse.Scripting.Testing
{
	/// <summary>
	/// Description of DummySimController.
	/// </summary>
	public class DummySimController : ISimController
	{
		public DummySimController()
		{
		}
		
		public ISim GetScriptGenericSimInterface(IScript script)
		{
			throw new NotImplementedException();
		}
		
		public ArrayList GetSimulatorScripts(ISim simulator)
		{
			throw new NotImplementedException();
		}
		
		public void InsertScript(ISim simulator, IScript script)
		{
			throw new NotImplementedException();
		}
		
		public IWorldModel GetSimulatorWorldModel(ISim simulator)
		{
			throw new NotImplementedException();
		}
	}
}
