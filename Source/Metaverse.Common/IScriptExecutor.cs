using System;

namespace Metaverse.Common 
{
	public interface IScriptExecutor
	{
		void Initialize();
		void Start();
		void Stop();
		void Run();
	}
}
