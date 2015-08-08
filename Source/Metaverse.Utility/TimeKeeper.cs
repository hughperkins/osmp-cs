// Copyright Hugh Perkins 2006
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License version 2 as published by the
// Free Software Foundation;
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
//  more details.
//
// You should have received a copy of the GNU General Public License along
// with this program in the file licence.txt; if not, write to the
// Free Software Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-
// 1307 USA
// You can find the licence also on the web at:
// http://www.opensource.org/licenses/gpl-license.php
//

using System;

namespace Metaverse.Utility
{
    public class TimeKeeper
    {
        System.DateTime starttime;
        System.DateTime lastresettime;
        System.DateTime lastelapsedtime;
        
        public TimeKeeper()
        {
            starttime = lastresettime = lastelapsedtime = System.DateTime.Now;
        }
        
        public int TimeSinceStart
        {
            get{
                int iElapsedTime = (int)System.DateTime.Now.Subtract( starttime ).TotalMilliseconds;
                return iElapsedTime;
            }
        }
        
        public int PrintTimeSinceStart()
        {
            return PrintTimeSinceStart( "Time since start" );
        }
    
        public int PrintTimeSinceStart( string text )
        {
            int iElapsedTime = (int)System.DateTime.Now.Subtract( starttime ).TotalMilliseconds;
            Test.WriteOut( text + ": " + iElapsedTime.ToString() );
            return iElapsedTime;
        }
        
        public int PrintTimeSinceReset()
        {
            int iElapsedTime = TimeSinceReset;
            Test.WriteOut("Time since reset: " + iElapsedTime.ToString() );
            return iElapsedTime;
        }
        
        public int TimeSinceReset
        {
            get{
                int iElapsedTime = (int)System.DateTime.Now.Subtract( lastresettime ).TotalMilliseconds;
                return iElapsedTime;
            }
        }
        
        public void ResetTimer()
        {
            lastelapsedtime = lastresettime = System.DateTime.Now;
        }
        
        public int ElapsedTime{
            get{
                int iElapsedTime = (int)System.DateTime.Now.Subtract( lastelapsedtime ).TotalMilliseconds;
                lastelapsedtime = System.DateTime.Now;
                return iElapsedTime;
            }
        }
        
        public int PrintElapsedTime()
        {
            return PrintElapsedTime( "Elapsed time" );
        }
    
        public int PrintElapsedTime( string text )
        {
            int iElapsedTime = (int)System.DateTime.Now.Subtract( lastelapsedtime ).TotalMilliseconds;
            Test.WriteOut( text + ": " + iElapsedTime.ToString() );
            lastelapsedtime = System.DateTime.Now;
            return iElapsedTime;
        }
    }
}
