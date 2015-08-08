// Copyright Hugh Perkins 2004,2005,2006
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

namespace OSMP
{
    // holds viewer state, ie are we editing 3d, or mouse look, etc
    // we dont want mouse look running whilst we're editing 3d, etc
    public class ViewerState
    {
        static ViewerState instance = new ViewerState();
        public static ViewerState GetInstance() { return instance; }

        public delegate void StateChangedHandler(
            ViewerStateEnum neweditstate,
            ViewerStateEnum newviewstate);
        public event StateChangedHandler StateChanged;

        const string CMD_MOUSELOOK = "mouselook";
        const string CMD_ZOOM = "camerazoom";
        const string CMD_PAN = "camerapan";
        const string CMD_ORBIT = "cameraorbit";

        ViewerStateEnum currentcamerastate = ViewerStateEnum.None;
        ViewerStateEnum currenteditstate = ViewerStateEnum.None;

        ViewerState()
        {
            CommandCombos.GetInstance().RegisterCommandGroup(
                new string[] { CMD_MOUSELOOK, CMD_ZOOM, CMD_PAN, CMD_ORBIT }, new KeyCommandHandler(CameraModeHandler));
        }

        public enum ViewerStateEnum
        {
            None,
            MouseLook,
            CameraZoom,
            CameraPan,
            CameraOrbit,
            Terrain,
            Edit3d
        };

        public ViewerStateEnum CurrentViewerState
        {
            get
            {
                if (currentcamerastate != ViewerStateEnum.None)
                {
                    return currentcamerastate;
                }
                return currenteditstate;
            }
        }

        void onStateChanged()
        {
            if (StateChanged != null)
            {
                StateChanged( currenteditstate, CurrentViewerState);
            }
        }

        public void ActivateEditTerrain()
        {
            currenteditstate = ViewerStateEnum.Terrain;
            onStateChanged();
        }

        public void ActivateEdit3d()
        {
            currenteditstate = ViewerStateEnum.Edit3d;
            onStateChanged();
        }

        public void FinishEdit3d()
        {
            currenteditstate = ViewerStateEnum.None;
            onStateChanged();
        }

        public void FinishEditTerrain()
        {
            currenteditstate = ViewerStateEnum.None;
            onStateChanged();
        }

        public void CameraModeHandler(string command, bool down)
        {
            if (down)
            {
                if (command == CMD_MOUSELOOK)
                {
                    currentcamerastate = ViewerStateEnum.MouseLook;
                }
                else if (command == CMD_ORBIT)
                {
                    currentcamerastate = ViewerStateEnum.CameraOrbit;
                }
                else if (command == CMD_PAN)
                {
                    currentcamerastate = ViewerStateEnum.CameraPan;
                }
                else if (command == CMD_ZOOM)
                {
                    currentcamerastate = ViewerStateEnum.CameraZoom;
                }
            }
            else
            {
                currentcamerastate = ViewerStateEnum.None;
            }
            onStateChanged();
        }
    }
}
