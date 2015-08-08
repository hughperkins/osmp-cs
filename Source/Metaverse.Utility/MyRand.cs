using System;
using System.Collections;

namespace Metaverse.Utility {

public class MyRand
{
	Random rand;
	
	public MyRand( int seed )
	{
		rand = new Random(seed);
	}
	public double GetRandomFloat( int min, int max )
	{
		return rand.NextDouble() * ( max - min ) + min;
	}
	public double GetRandomFloat( double min, double max )
	{
		return rand.NextDouble() * ( max - min ) + min;
	}
	public int GetRandomInt( int min, int max )
	{
		return rand.Next( min, max + 1 );
	}
}

}
