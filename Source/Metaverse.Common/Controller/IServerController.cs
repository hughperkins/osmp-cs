/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/28/2006
 * Time: 11:51 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Nini.Config;

namespace Metaverse.Common.Controller
{
	/// <summary>
	/// Description of IServerController.
	/// </summary>
	public interface IServerController
	{
		void Initialize( IConfigSource commandlineConfig );
		void InitializeServer();
	}
}
