/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/13/2006
 * Time: 11:36 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Metaverse.Common;

namespace Metaverse.Scripting.Testing
{
	/// <summary>
	/// Description of AGenericScript.
	/// </summary>
	public abstract class AGenericScript : IScript
	{
		public ISim Sim 
		{
			get { return SimController.Singleton.GetScriptGenericSimInterface( this ); }
		}
		
		public abstract bool Active { get; }

		public abstract void Run( float delta );
	}
}
