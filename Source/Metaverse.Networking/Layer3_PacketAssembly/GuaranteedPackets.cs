// Copyright Hugh Perkins 2006
// hughperkins@gmail.com http://manageddreams.com
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
using System.Collections;
using System.Text; // for Encoding
using System.Net;
    
namespace OSMP
{
    // Use this to send guaranteed packets over network
    public class GuaranteedPackets
    {
        //static GuaranteedPackets instance = new GuaranteedPackets();
        //public static GuaranteedPackets GetInstance(){ return instance; }
        
        NetworkLevel2Controller networklevel2controller;

        public delegate void ReceivedPacketHandler( byte[] data, int offset, int length );
        public event ReceivedPacketHandler ReceivedPacket; 
        
        public GuaranteedPackets()
        {
            networklevel2controller = new NetworkLevel2Controller();
            net.RegisterPacketHandler( 'P', this );
        }
        
        void ReceivedPacket( int GlobalPacketSequence, object connection, byte[] packet, int offset )
        {
            if( ReceivedPacket != null )
            {
                //byte[]packetdata = new byte[ packet.Length - 1 ];
                //Buffer.BlockCopy( packet, 1, packetdata, 0, packet.Length - 1 );
                ReceivedPacket( connection, packet, offset + 1, packet.Length - 1 - offset );
            }
        }
    }
}
