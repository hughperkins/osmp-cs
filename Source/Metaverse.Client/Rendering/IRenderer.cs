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
using SdlDotNet;

namespace OSMP
{
    public interface IRenderable
    {
        void Render();
    }

    //public delegate void MainLoopDelegate();
    public delegate void WriteNextFrameCallback(Vector3 camerapos);
    public delegate void PreDrawCallback();
    public delegate void TickHandler();
    public delegate void MainMenuCallback();

    public interface IRenderer
    {
        int OuterWindowWidth { get; }
        int OuterWindowHeight { get; }

        /// <summary>
        /// inner window width
        /// </summary>
        int WindowWidth { get; }
        /// <summary>
        /// inner window height
        /// </summary>
        int WindowHeight { get; }

        int ScreenDistanceScreenCoords { get; }
        double FieldOfView { get; }
        double FarClip { get; }
        double NearClip { get; }

        //void SetupAxes( Entity entity );
        //void ApplyViewingMatrixes();

        // void RegisterMainMenu( string[] contextmenupath, MainMenuCallback callback );

        event TickHandler Tick;
        event WriteNextFrameCallback WriteNextFrameEvent;
        event WriteNextFrameCallback WriteAlpha;
        event PreDrawCallback PreDrawEvent;

        event MouseMotionEventHandler MouseMotion;
        event MouseButtonEventHandler MouseDown;
        event MouseButtonEventHandler MouseUp;
        event KeyboardEventHandler KeyUp;
        event KeyboardEventHandler KeyDown;

        void DrawWorld();

        void Init();
        void Shutdown();

        void ApplyViewingMatrices();
        //void RegisterMainLoopCallback(MainLoopDelegate mainloop );

        void StartMainLoop();
        //void DrawWorld();
    }
}
