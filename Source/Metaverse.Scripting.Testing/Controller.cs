/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/16/2006
 * Time: 1:02 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Metaverse.Common;

namespace Metaverse.Scripting.Testing
{
	/// <summary>
	/// Description of Controller.
	/// </summary>
	public class Controller
	{
		public static IMetaverseController Metaverse = MetaverseController.Singleton;
		public static ISimController Sim = SimController.Singleton;
		public static IClientController Client = ClientController.Singleton;
	}
}
