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

namespace OSMP
{
    public class LockRequestEventArgs : EventArgs
    {
        public object TargetObject;
        public bool LockGranted;
        public LockRequestEventArgs( object TargetObject, bool LockGranted )
        {
            this.TargetObject = TargetObject; this.LockGranted = LockGranted;
        }
    }
    
    public class LockReleasedEventArgs : EventArgs
    {
        public object TargetObject;
        public LockReleasedEventArgs( object TargetObject )
        {
            this.TargetObject = TargetObject;
        }
    }
    
    public interface ILockConsumer
    {
        void LockResponse( object source, LockRequestEventArgs e );
        void LockReleased( object source, LockReleasedEventArgs e );
    }
    
    public class LockRpcToServer : NetworkInterfaces.ILockRpcToServer
    {
        object connection;
        public LockRpcToServer(object connection) { this.connection = connection; }
        public void RequestLock(object netconnection, NetworkInterfaces.IReplicated targetobject)
        {
            LockController.GetInstance().RequestLock(netconnection, targetobject);
        }
        public void RenewLock(object netconnection, NetworkInterfaces.IReplicated targetobject)
        {
            LockController.GetInstance().RenewLock(netconnection, targetobject);
        }
        public void ReleaseLock(object netconnection, NetworkInterfaces.IReplicated targetobject)
        {
            LockController.GetInstance().ReleaseLock(netconnection, targetobject);
        }
    }

    public class LockRpcToClient : NetworkInterfaces.ILockRpcToClient
    {
        object connection;
        public LockRpcToClient(object connection) { this.connection = connection; }
        public void LockRequestResponse(object netconnection, NetworkInterfaces.IReplicated targetobject, bool LockCreated)
        {
            LockController.GetInstance().LockRequestResponse(netconnection, targetobject, LockCreated);
        }
        public void LockReleased(object netconnection, NetworkInterfaces.IReplicated targetobject)
        {
            LockController.GetInstance().LockReleased(netconnection, targetobject);
        }
    }

    // To keep replication and synchronization as simple as possible, the world is run on the server and the
    // only object modified by clients is the client's avatar
    // When someone wants to edit an object, they request a lock, using this LockController class
    // Once they have a lock on an object they can do what they want with it, and stream their changes to the server
    //
    // keeping the interface as simple as possible
    // lockrequesters can check if they own a lock ("MeOwnsLock"), and if they havent they can request it
    //
    // LockController can run as master or not
    // if it is master, it is running on server, serving lock requests from clients
    // if it is not master, it is running on a remote client, and defers its decisions to the server, via the Rpc calls
    //
    // we assume only one object in any single osmp process can request a lock
    // so we dont need to pass the requester through to the server and back
    //
    // Note that client classes should enter via the non-rpc functions; rpc functions are for the lockcontrolleritself
    // note to self: some way of enforcing this? put the rpc calls on a second class? use reflection to make the rpc calls non-public???
    public class LockController
    {
        public int LockTimeoutSeconds = 10; // time after which lock times out if not renewed
        
        // if ismaster is true we are the reference,
        // otherwise we have to defer to server, via rpc functions
        bool ismaster;
        
        RpcController rpc;
        
        // locks by object, in hashtable for fast lookup
        // on server, this contains all locks
        // on client( ismaster false), only contains our locks
        //
        // on server, locks are by connection for remote requesters, and by ILockConsumer for local requesters
        // note to self: maybe we should be passing requesting object through to server, to avoid mixing types like this???
        Hashtable locks = new Hashtable();
        
        class LockInfo
        {
            public DateTime Timestamp;
            public object Requester;
            public NetworkInterfaces.IReplicated TargetObject;
            public LockInfo(object Requester, NetworkInterfaces.IReplicated TargetObject)
            {
                this.Requester = Requester; this.TargetObject = TargetObject;
                Timestamp = DateTime.Now;
            }
            public void RenewTimestamp()
            {
                Timestamp = DateTime.Now;
            }
        }
        
        static LockController instance;
        public static LockController GetInstance(){ return instance; }
        
        public LockController( RpcController rpc )
        {
            if (instance != null)
            {
                this.rpc = rpc;
                instance = this;
            }
        }
        
        // checks for old locks, and removes them
        public void Tick()
        {
            ArrayList RemovedLocks = new ArrayList();
            foreach( DictionaryEntry entry in locks )
            {
                LockInfo lockinfo = (LockInfo)entry.Value;
                if( lockinfo.Timestamp.Subtract( DateTime.Now ).Milliseconds > LockTimeoutSeconds * 1000 )
                {
                    NetworkInterfaces.IReplicated targetobject = lockinfo.TargetObject;
                    if( IsMaster )
                    {
                        object clientconnection = lockinfo.Requester;
                        new LockRpcToClient( clientconnection ).LockReleased(
                            clientconnection, targetobject );
                    }
                    else
                    {
                        ILockConsumer requester = (ILockConsumer)lockinfo.Requester;
                        new LockRpcToServer(null).ReleaseLock(
                            this, targetobject );
                    }
                    RemovedLocks.Add( lockinfo.TargetObject );
                }
            }
            for( int i = 0; i < RemovedLocks.Count; i++ )
            {
                locks.Remove( RemovedLocks[i] );
            }
        }
        
        public bool IsMaster
        {
            get{ return ismaster; }
            set{ ismaster = value; Console.WriteLine( this.GetType().ToString() + " IsMaster set to: " + ismaster.ToString() ); }
        }
        
        // runs on master, called by rpc call or non-rpc call
        // returns true if lock granted successfully, or already owned by requester
        bool _MasterRequestLock(object requester, NetworkInterfaces.IReplicated targetobject)
        {
            if( !locks.Contains( targetobject ) )
            {
                locks.Add( targetobject, new LockInfo( requester, targetobject ) );
                return true;
            }
            else if( ((LockInfo)locks[ targetobject ]).Requester == requester )
            {
                return true;
            }
            return false;
        }
        
        // used on client to store pending requests
        ArrayList pendingrpcrequests = new ArrayList();
        
        // if we are master, we can grant lock if available, then signal requester immediately
        // otherwise we makes async rpc request to server
        public void RequestLock(ILockConsumer requester, NetworkInterfaces.IReplicated targetobject)
        {
            if( ismaster )
            {
                bool lockgranted = _MasterRequestLock( requester, targetobject);
                requester.LockResponse( this, new LockRequestEventArgs( targetobject, lockgranted ) );
            }
            else
            {
                if( ((LockInfo)locks[ targetobject ]).Requester == requester )
                {
                    requester.LockResponse( this, new LockRequestEventArgs( targetobject, true ) );
                }
                else if( locks[ targetobject ] != null )
                {
                    requester.LockResponse( this, new LockRequestEventArgs( targetobject, false ) );
                }
                else
                {
                    pendingrpcrequests.Add( new object[]{ requester, targetobject } );
                    new LockRpcToServer( null ).ReleaseLock( null, targetobject );
                }
            }
        }

        public bool MeOwnsLock(NetworkInterfaces.IReplicated requester, NetworkInterfaces.IReplicated targetobject)
        {
            if( locks[ targetobject ] != null && ((LockInfo)locks[ targetobject ]).Requester == requester )
            {
                return true;
            }
            return false;
        }
        
        // client to server
        public void RequestLock(object netconnection, NetworkInterfaces.IReplicated targetobject)
        {
            bool lockgranted = _MasterRequestLock( netconnection, targetobject);
            new LockRpcToClient( netconnection ).LockRequestResponse( netconnection, targetobject, lockgranted );
        }

        // renew lock; no response required; if the lock gets released, we get the response in LockReleased anyway
        // client to server
        public void RenewLock(object netconnection, NetworkInterfaces.IReplicated targetobject)
        {
            if( locks.ContainsKey( targetobject ) && ((LockInfo)locks[ targetobject ]).Requester == netconnection )
            {
                ((LockInfo)locks[ targetobject ]).RenewTimestamp();
            }
        }

        // client to server
        public void ReleaseLock(object netconnection, NetworkInterfaces.IReplicated targetobject)
        {
        }

        // send result to ILockConsumer, and add lock to locks
        // server to client
        public void LockRequestResponse(object netconnection, NetworkInterfaces.IReplicated targetobject, bool LockCreated)
        {
            for( int i = 0; i < pendingrpcrequests.Count; i++ )
            {
                object[]pendingrequest = (object[])pendingrpcrequests[i];
                if( pendingrequest[1] == targetobject )
                {
                    ( (ILockConsumer)pendingrequest[0] ).LockResponse( this, new LockRequestEventArgs( targetobject, LockCreated ) );
                    if( locks.Contains( targetobject ) )
                    {
                        locks.Remove( targetobject );
                    }
                    locks.Add( targetobject, new LockInfo( pendingrequest[0], targetobject ) );
                    pendingrpcrequests.RemoveAt( i );
                    i--;
                }
            }
        }        
        
        // if this is one of our locks, signal the ILockConsumer, then mark lock released
        // server to client
        public void LockReleased(object netconnection, NetworkInterfaces.IReplicated targetobject)
        {
            if( locks.Contains( targetobject ) )
            {
                ( (ILockConsumer)locks[ targetobject ] ).LockReleased( this, new LockReleasedEventArgs( targetobject ) );
                locks.Remove( targetobject );
            }
        }        
    }
}
