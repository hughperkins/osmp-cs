// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
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

namespace NetworkInterfaces
{
    public interface IObjectReferenceRpc
    {
        // client to server
        void RequestNewReferenceRpc(object connection, Type targettype, RefToLocalObject requester);

        // This returns a new reference; it is the client's responsibility to ensure this is only applied to a brand-new object        
        // server to client
        void ReferenceResponse(object connection, int reference, Type targettype, RefToLocalObject requesterref);
    }
}
