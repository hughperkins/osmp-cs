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
using Metaverse.Utility;
    
namespace OSMP
{
    // sequencenumber
    // ==============
    // - we are using a sharedkey to reduce spoofing, so we just use a sequential number, starting from 1, for the sequencenumber
    // - the sequencenumber for each direction is independent.  Each will start from 1, and they can each use the same numbers as each other.
    //
    // Acking
    // ======
    // one ack packet is sent every AckPacketIntervalSeconds, to each connection
    // the ack packet code is 'A'
    // as packets are received they are added to the ReceivedPacketsNotAcked,
    // ack packet format:
    // [int32 key][short ref][char 'A'][first packet ref][second packet ref][third packet ref] ...
    //
    // note that if packet ref is 0, the packet is considered non-ackable:
    // - on the transmitting side, the packet is not stored pending potential resends
    // - on the receiving side, no ack is sent
    public class NetPacketReferenceController
    {
        public int AckPacketIntervalSeconds = 1;  // interval between sending ack packets for received packets
        public int ResendIntervalSeconds = 3;  // delay after which a non-acked packet is resent
        public int CheckResendIntervalSeconds = 1;  // interval between checks on age of non-acked packets
            
        NetworkLevel2Connection parent;
        DateTime lastackpacketsent;
        DateTime lastresendcheck;
        short nextpacketreference = 1;
        bool isserver;

        BinaryPacker binarypacker = new BinaryPacker();
        
        public Hashtable recentreceivedpackets = new Hashtable();  // ( packetref, datetime )
        public Queue receivedpacketsnotacked = new Queue();  // just contains list of packet references, no timestamp
        public Hashtable sentpacketsawaitingack = new Hashtable(); // ( packetref, new object[]{ datetime, byte[]packet ) )
        
        public NetPacketReferenceController( NetworkLevel2Connection parent, bool isserver )
        {
            this.parent = parent;
            this.isserver = isserver;
            lastresendcheck = DateTime.Now;
            lastackpacketsent = DateTime.Now;
            parent.RegisterPacketHandler( 'A', new PacketHandler( APacketHandler ) );            
        }
        
        public short NextReference
        {
            get{
                short nextreference = this.nextpacketreference;
                nextpacketreference++;
                return nextreference;
            }
        }

        public bool ValidateIncomingReference( short packetref )
        {
            //LogFile.WriteLine("Validating incoming ref " + packetref.ToString() );
            if( packetref == 0 ) // non-ackable packet
            {
                return true;
            }
            if( !recentreceivedpackets.Contains( packetref ) )
            {
                //LogFile.WriteLine("Adding " + packetref.ToString() + " to queue..." );
                recentreceivedpackets.Add( packetref, DateTime.Now );
                receivedpacketsnotacked.Enqueue( packetref );
                return true;
            }
            else
            {
                // need to add packet to receivedpacketsnotacked anyway, in case our first ack didnt make it to the other party
                if( !receivedpacketsnotacked.Contains( packetref ) )
                {
                    receivedpacketsnotacked.Enqueue( packetref );
                }
                return false;
            }
        }
        
        public void RegisterSentPacket( short packetreference, byte[] packet )
        {
            sentpacketsawaitingack.Add( packetreference, new object[]{ DateTime.Now, packet } );
        }
        
        // handles incoming ack packets ('A' packets)
        public void APacketHandler( object source, PacketHandlerArgs e )
        {
            //LogFile.WriteLine( "Processing 'A' packet: " );
            byte[] packet = e.Data;
            int nextposition = e.NextPosition; 
            while( nextposition < packet.Length )
            {
                short ackedpacketreference = (short)binarypacker.ReadValueFromBuffer(packet, ref nextposition, typeof(short));
              //  LogFile.WriteLine( "  ... Packet " + ackedpacketreference.ToString() + " acked" );
                sentpacketsawaitingack.Remove( ackedpacketreference );
            }
            DumpSentPacketsAwaitingAck();
        }
        
        void DumpSentPacketsAwaitingAck()
        {
            //foreach( DictionaryEntry dictionaryentry in sentpacketsawaitingack )
            //{
                //LogFile.WriteLine( "sent packet awaiting ack: " + dictionaryentry.Key.ToString() );
            //}
        }
        
        public void Tick()
        {
            SendAckPackets();
            ResendNonAckedPackets();
        }
        
        void ResendPacket( short packetreference )
        {            
            object[] sentpacketinfo = (object[])sentpacketsawaitingack[ packetreference ];
            byte[] data = (byte[])sentpacketinfo[1];
            LogFile.WriteLine("resending packet ref " + packetreference.ToString() );
            parent.RawSend( data );
            sentpacketsawaitingack[ packetreference ] = new object[]{ DateTime.Now, data };
        }
        
        void ResendNonAckedPackets()
        {
            if( (int)DateTime.Now.Subtract( lastresendcheck ).TotalMilliseconds < CheckResendIntervalSeconds * 1000 )
            {
                return;
            }
            lastresendcheck = DateTime.Now;
            Queue packetrefstoresend = new Queue();
            foreach( DictionaryEntry dictionaryentry in sentpacketsawaitingack )
            {
                object[]sentpacketinfo = (object[])dictionaryentry.Value;
                DateTime lastsendtime = (DateTime)sentpacketinfo[0];
                if( DateTime.Now.Subtract( lastsendtime ).TotalMilliseconds > ResendIntervalSeconds * 1000 )
                {
                    packetrefstoresend.Enqueue( (short)dictionaryentry.Key );
                }
            }
            while( packetrefstoresend.Count > 0 )
            {
                ResendPacket( (short)packetrefstoresend.Dequeue() );
            }
        }
        
        void SendAckPackets()
        {
            // LogFile.WriteLine("Checking Last ack  " + ((int)DateTime.Now.Subtract( lastackpacketsent ).TotalMilliseconds).ToString() );
            if( (int)DateTime.Now.Subtract( lastackpacketsent ).TotalMilliseconds > AckPacketIntervalSeconds * 1000 )
            {
                lastackpacketsent = DateTime.Now;
                byte[] ackpacketdata = null;
                lock( receivedpacketsnotacked )
                {
                    if( receivedpacketsnotacked.Count == 0 )
                    {
                        return;
                    }
                    //LogFile.WriteLine("Creating ack packet..." );
                    int numpacketstoack = receivedpacketsnotacked.Count;
                    ackpacketdata = new byte[ numpacketstoack * 2 ];
                    int nextposition = 0;
                    for( int i = 0; i < numpacketstoack; i++ )
                    {
                        short packettoack = (short)receivedpacketsnotacked.Dequeue();
                        binarypacker.WriteValueToBuffer(ackpacketdata, ref nextposition, packettoack);
                      //  LogFile.WriteLine("   ... acking " + packettoack.ToString() );
                    }
                }
                //LogFile.WriteLine("Sending ack packet " + Encoding.ASCII.GetString( ackpacketdata, 0, ackpacketdata.Length ) );
                parent.SendNonAckable( 'A', ackpacketdata );
            }
        }        
    }
}
