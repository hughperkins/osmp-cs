using System;

namespace Metaverse.Common 
{
	
	public interface IScript
	{
		bool Active { get; }

		void Run( float delta );
	}

}
