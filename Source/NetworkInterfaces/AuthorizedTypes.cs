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
using System.Collections.Generic;
using System.Text;

namespace OSMP.NetworkInterfaces
{
    // Add this to authorized osmp RPC interfaces
    // This is a security measure,to prevent clients instantiating objects like Activator etc
    public class AuthorizedRpcInterface : Attribute
    //public interface IIsOsmpRpcInterface
    {
    }

    public class AuthorizedTypes
    {
        static AuthorizedTypes instance = new AuthorizedTypes();
        public static AuthorizedTypes GetInstance() { return instance; }

        /*
        List<Type> authorizedtypes = new List<Type>();

        AuthorizedTypes()
        {
            authorizedtypes.Add(typeof(Testing.ITestInterface));

            authorizedtypes.Add(typeof(OSMP.NetworkInterfaces.ILockRpcToClient));
            authorizedtypes.Add(typeof(OSMP.NetworkInterfaces.ILockRpcToServer));
            authorizedtypes.Add(typeof(OSMP.NetworkInterfaces.IObjectReplicationClientToServer));
            authorizedtypes.Add(typeof(OSMP.NetworkInterfaces.IObjectReplicationServerToClient));
        }

        // This is for security, to prevent clients instantiating objects like Activator, etc...
        // RpcController consults this list, and only instantiates classes it finds here
        public List<Type> AuthorizedTypeList
        {
            get
            {
                return authorizedtypes;
            }
        }
        */
        public bool IsAuthorized(string typename)
        {
            foreach (object attribute in Type.GetType(typename).GetCustomAttributes(false))
            {
                if (attribute.GetType() == typeof(AuthorizedRpcInterface))
                {
                    return true;
                }
            }
            //if (Type.GetType(typename) is IIsOsmpRpcInterface)
            //{
              //  return true;
            //}
            return false;
        }
    }
}
