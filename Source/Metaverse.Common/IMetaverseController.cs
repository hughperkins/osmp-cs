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
	/// Description of IMetaverseController.
	/// </summary>
	public interface IMetaverseController
	{
		void RegisterMetaverseServer( IMetaverseServer server );
		IMetaverseServer GetSimServer( ISim simulator );
		void AttachSimulator( ISim simulator );
		ArrayList GetSimulators();
	}
}
