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
	/// Description of DummyMetaverseController.
	/// </summary>
	public class DummyMetaverseController : IMetaverseController
	{
		public DummyMetaverseController()
		{
		}
		
		public void RegisterMetaverseServer(IMetaverseServer server)
		{
			throw new NotImplementedException();
		}
		
		public IMetaverseServer GetSimServer(ISim simulator)
		{
			throw new NotImplementedException();
		}
		
		public void AttachSimulator(ISim simulator)
		{
			throw new NotImplementedException();
		}
		
		public ArrayList GetSimulators()
		{
			throw new NotImplementedException();
		}
	}
}
