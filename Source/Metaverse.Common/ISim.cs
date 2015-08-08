/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 12/13/2006
 * Time: 11:53 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;

namespace Metaverse.Common
{
	/// <summary>
	/// Description of ISim.
	/// </summary>
	public interface ISim
	{
		void Trace( string message );
		void IncrementSimWorldData();
	}
}
