/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/14/2006
 * Time: 12:22 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;

namespace Metaverse.Common
{
	/// <summary>
	/// Description of IGlobalProtocol.
	/// </summary>
	public interface IMetaverseServer
	{
		ArrayList GetScripts();
		void InsertScript( IScript script );
		IWorldModel GetWorldModel( ISim simulator );
	}
}
