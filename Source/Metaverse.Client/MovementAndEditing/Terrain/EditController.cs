// Copyright Hugh Perkins 2006
// hughperkins at gmail http://hughperkins.com
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
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

namespace OSMP
{
    // manages editing
    public class EditController
    {
        static EditController instance = new EditController();
        public static EditController GetInstance() { return instance; }

        public EditController()
        {
            CommandCombos.GetInstance().RegisterAtLeastCommand("increaseheight", new KeyCommandHandler(handler_IncreaseHeight));
            CommandCombos.GetInstance().RegisterAtLeastCommand("decreaseheight", new KeyCommandHandler(handler_DecreaseHeight));
            MetaverseClient.GetInstance().Tick += new MetaverseClient.TickHandler(EditController_Tick);
            ViewerState.GetInstance().StateChanged += new ViewerState.StateChangedHandler(EditController_StateChanged);
        }

        void EditController_StateChanged(ViewerState.ViewerStateEnum neweditstate, ViewerState.ViewerStateEnum newviewstate)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IsInitialMouseclick">When mouse button is initially pressed, this is true.</param>
        void ApplyBrush(bool IsInitialMouseclick)
        {
            if (!(increaseheight || decreaseheight))
            {
                return;
            }

            if (ViewerState.GetInstance().CurrentViewerState != ViewerState.ViewerStateEnum.Terrain)
            {
                return;
            }

            if (CurrentEditBrush.GetInstance().BrushEffect == null ||
                CurrentEditBrush.GetInstance().BrushShape == null)
            {
                return;
            }

            if (!(IsInitialMouseclick || CurrentEditBrush.GetInstance().BrushEffect.Repeat))
            {
                return;
            }

            Vector3 intersectpoint = EditingHelper.GetIntersectPoint();
            if (intersectpoint == null)
            {
                return;
            }

            double x = intersectpoint.x;
            double y = intersectpoint.y;
            if (x >= 0 && y >= 0 &&
                x < (MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapWidth) &&
                y < (MetaverseClient.GetInstance().worldstorage.terrainmodel.HeightMapHeight))
            {
                double milliseconds = DateTime.Now.Subtract(LastDateTime).TotalMilliseconds;
                LastDateTime = DateTime.Now;
                CurrentEditBrush.GetInstance().BrushEffect.ApplyBrush(
                    CurrentEditBrush.GetInstance().BrushShape, CurrentEditBrush.GetInstance().BrushSize,
                    x, y, increaseheight, milliseconds);
            }
        }

        void EditController_Tick()
        {
            ApplyBrush(false);
        }

        bool increaseheight = false;
        bool decreaseheight = false;
        DateTime LastDateTime;

        void handler_IncreaseHeight(string command, bool down)
        {
            //LogFile.WriteLine("EditController.handler_increaseheight");
            //if (ViewerState.GetInstance().CurrentViewerState == ViewerState.ViewerStateEnum.Terrain)
            //{
            if (down)
            {
                if (ViewerState.GetInstance().CurrentViewerState == ViewerState.ViewerStateEnum.Terrain)
                {
                    LastDateTime = DateTime.Now;
                    increaseheight = true;
                    ApplyBrush(true);
                }
            }
            else
            {
                increaseheight = false;
            }
            //}
        }

        void handler_DecreaseHeight(string command, bool down)
        {
            //LogFile.WriteLine("EditController.handler_decreaseheight");
            if (down)
            {
                if (ViewerState.GetInstance().CurrentViewerState == ViewerState.ViewerStateEnum.Terrain)
                {
                    LastDateTime = DateTime.Now;
                    decreaseheight = true;
                    ApplyBrush(true);
                }
            }
            else
            {
                decreaseheight = false;
            }
        }
    }
}
