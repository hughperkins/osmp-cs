// Copyright Hugh Perkins 2004,2005,2006
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
using Metaverse.Utility;
using SdlDotNet;

namespace OSMP
{
    public class HitTargetEditHandle : HitTarget
    {
        public Axis axis;
        public HitTargetEditHandle( Axis axis )
        {
            this.axis = axis;
        }
    };
        
    public class Editing3d
    {
        static Editing3d instance = new Editing3d();
        public static Editing3d GetInstance()
        {
            return instance;
        }
        
        //! Current edit type, eg none, position, constrained position (poshandle), rot, scale
        public enum EditType
        {
             None,
             PosFree,
             ScaleFree,
             RotFree,
             PosHandle,
             ScaleHandle,
             RotHandle
        }
        
        //! current edit bar type to show, ie none, pos, scale or rot
        public enum EditBarType
        {
             None,
             Pos,
             Scale,
             Rot
        }

        public int iDragStartX;  //!< mouse x at start of edit drag
        public int iDragStartY;  //!< mouse y at start of edit drag
        
        public EditBarType editbartype; //!< current edit / editbar type
        public Axis currentaxis; //!< current constraint axis, for constrained edits
        public EditType currentedittype;  //!< current edit type, ie scale, pos, or rotate
        
        public Vector3 startpos;  //!< position at start of edit drag
        public Vector3 startscale; //!< scale at start of edit drag
        public Rot startrot; //!< rotation at start of edit drag
        
        public bool bAxisModifierOn;  //!< local or global axes?
        public bool bShowEditBars;  //!< show edit bars?  This controls what happens when one calls DrawEditBarsToOpenGL()
        
        Editing3dPos editing3dpos;
        Editing3dScale editing3dscale;
        Editing3dRot editing3drot;
        
        SelectionModel selectionmodel;

        MouseCache mousefiltermousecache;

        string CMD_EDITPOSITION = "editposition";
        string CMD_EDITSCALE = "editscale";
        string CMD_EDITROTATION = "editrotation";
        
        public Editing3d()
        {
            selectionmodel = SelectionModel.GetInstance();
            mousefiltermousecache = MouseCache.GetInstance();

            CommandCombos.GetInstance().RegisterCommandGroup(
                new string[] { CMD_EDITPOSITION, CMD_EDITSCALE, CMD_EDITROTATION }, new KeyCommandHandler(EditModeKeyEvent));

            CommandCombos.GetInstance().RegisterAtLeastCommand(
                "leftmousebutton", new KeyCommandHandler(MouseDown));

            mousefiltermousecache.MouseMove += new MouseMoveHandler( MouseMove );
                
            editing3dpos = new Editing3dPos( this );
            editing3drot = new Editing3dRot( this );
            editing3dscale = new Editing3dScale( this );

            RendererFactory.GetInstance().WriteNextFrameEvent += new WriteNextFrameCallback(Editing3d_WriteNextFrameEvent);
        }

        void Editing3d_WriteNextFrameEvent(Vector3 camerapos)
        {
            DrawEditBarsToOpenGL( camerapos );
        }

        public bool IsHandleEdit()
        {
            if( currentedittype == EditType.PosHandle || currentedittype == EditType.ScaleHandle || currentedittype == EditType.RotHandle )
            {
                return true;
            }
            return false;
        }
        
        public Color GetEditHandleColor( Axis axis )
        {
            Color edithandlecolor = null;
            if( axis.IsXAxis )
            {
                if( IsHandleEdit() && mousefiltermousecache.LeftMouseDown &&
                    currentaxis.IsXAxis )
                {
                    edithandlecolor = new Color( 1.0, 0.5, 0.5, 0.5 );
                }
                else
                {
                    edithandlecolor = new Color( 1.0, 0.0, 0.0, 0.5 );
                }
            }
            else if( axis.IsYAxis )
            {
                if( IsHandleEdit() && mousefiltermousecache.LeftMouseDown &&
                    currentaxis.IsYAxis )
                {
                    edithandlecolor = new Color( 0.5, 1.0, 0.5, 0.5 );
                }
                else
                {
                    edithandlecolor = new Color( 0.0, 1.0, 0.0, 0.5 );
                }
            }
            else
            {
                if( IsHandleEdit() && mousefiltermousecache.LeftMouseDown &&
                    currentaxis.IsZAxis )
                {
                    edithandlecolor = new Color( 0.5, 0.5, 1.0, 0.5 );
                }
                else
                {
                    edithandlecolor = new Color( 0.0, 0.0, 1.0, 0.5 );
                }
            }            
            return edithandlecolor;
        }

        bool InEditDrag = false;

        public void MouseDown( string command, bool down )
        {
            if (ViewerState.GetInstance().CurrentViewerState == ViewerState.ViewerStateEnum.Edit3d)
            {
                InEditDrag = down;
                if (down)
                {
                    if (editbartype == EditBarType.Pos)
                    {
                        InitiateTranslateEdit(mousefiltermousecache.MouseX, mousefiltermousecache.MouseY);
                    }
                    else if (editbartype == EditBarType.Rot)
                    {
                        InitiateRotateEdit(mousefiltermousecache.MouseX, mousefiltermousecache.MouseY);
                    }
                    else if (editbartype == EditBarType.Scale)
                    {
                        InitiateScaleEdit(mousefiltermousecache.MouseX, mousefiltermousecache.MouseY);
                    }
                }
                else
                {
                    if (editbartype != EditBarType.None)
                    {
                        EditDone();
                    }
                }
            }
            else
            {
                if (editbartype != EditBarType.None)
                {
                    EditDone();
                }
                InEditDrag = false;
            }
        }
                        
        public void MouseMove()
        {
            if (InEditDrag)
            {
                //if( e.Button == MouseButtons.Left )
                //{
                if (editbartype == EditBarType.Pos)
                {
                    UpdateTranslateEdit(false, mousefiltermousecache.MouseX, mousefiltermousecache.MouseY);
                }
                else if (editbartype == EditBarType.Rot)
                {
                    UpdateRotateEdit(false, mousefiltermousecache.MouseX, mousefiltermousecache.MouseY);
                }
                else if (editbartype == EditBarType.Scale)
                {
                    UpdateScaleEdit(false, mousefiltermousecache.MouseX, mousefiltermousecache.MouseY);
                }
            }
            //}
        }

        //public void MouseUp(object source, MouseButtonEventArgs e)
        //{
            //if( e.Button == MouseButtons.Left )
            //{
                //if( editbartype != EditBarType.None )
                //{
              //      EditDone();
            //    }
          //  }
        //}

        public void EditModeKeyEvent(string command, bool down)
        {
            //if (down)
            //{
              //  ViewerState.GetInstance().CurrentViewerState = ViewerState.ViewerStateEnum.Edit3d;
            //}
            //else
            //{
              //  ViewerState.GetInstance().CurrentViewerState = ViewerState.ViewerStateEnum.None;
            //}
            if (command == CMD_EDITPOSITION)
            {
                PosEditModeKeyEvent(command, down);
            }
            else if (command == CMD_EDITROTATION)
            {
                RotEditModeKeyEvent(command, down);
            }
            else if (command == CMD_EDITSCALE)
            {
                ScaleEditModeKeyEvent(command, down);
            }
        }

        public void PosEditModeKeyEvent( string command, bool down )
        {
            Test.Debug("PosEditModeKeyEvent " + down);
            if (down)
            {
                ShowEditPosBars();
            }
            else
            {
                HideEditBars();
            }
        }

        public void RotEditModeKeyEvent(string command, bool down)
        {
            Test.Debug("RotEditModeKeyEvent " + down);
            if (down)
            {
                ShowEditRotBars();
            }
            else
            {
                HideEditBars();
            }
        }

        public void ScaleEditModeKeyEvent(string command, bool down)
        {
            Test.Debug("ScaleEditModeKeyEvent " + down);
            if (down)
            {
                ShowEditScaleBars();
            }
            else
            {
                HideEditBars();
            }
        }
        
        public void EditingPreliminaries()
        {
            // selectionmodel.bSendObjectMoveForSelection = true;
        }
        
        public void InitiateScaleEdit( int mousex, int mousey )
        {
            EditingPreliminaries();
        
            HitTarget hittarget =RendererFactory.GetPicker3dModel().GetClickedHitTarget( mousex, mousey );
            //Test.Debug(  "Clicked target type: " + hittarget.TargetType ); // Test.Debug
        
            if( hittarget is HitTargetEditHandle )
            {
                Test.Debug(  "It's an edit handle :-O" ); // Test.Debug
                editing3dscale.InitiateHandleEdit( mousex, mousey, ( ( HitTargetEditHandle)hittarget ).axis );
            }
            else
            {
                Test.Debug(  "Not an edit handle" ); // Test.Debug
                editing3dscale.InitiateFreeEdit( mousex, mousey );
            }
        }
        
        public void InitiateTranslateEdit( int mousex, int mousey )
        {
            EditingPreliminaries();
        
            //  mvKeyboardAndMouse::bDragging = true;
            
            HitTarget hittarget =RendererFactory.GetPicker3dModel().GetClickedHitTarget( mousex, mousey );
            //Test.Debug(  "Clicked target type: " + hittarget.TargetType ); // Test.Debug
        
            if( hittarget is HitTargetEditHandle )
            {
                Test.Debug(  "It's an edit handle :-O" ); // Test.Debug
                editing3dpos.InitiateHandleEdit( mousex, mousey, ( ( HitTargetEditHandle)hittarget ).axis );
            }
            else
            {
                Test.Debug(  "Not an edit handle" ); // Test.Debug
                editing3dpos.InitiateFreeEdit( mousex, mousey );
            }
        }
        
        public void InitiateRotateEdit( int mousex, int mousey )
        {
            EditingPreliminaries();
        
            //  mvKeyboardAndMouse::bDragging = true;
        
            HitTarget hittarget =RendererFactory.GetPicker3dModel().GetClickedHitTarget( mousex, mousey );
            //Test.Debug(  "Clicked target type: " + hittarget.TargetType ); // Test.Debug
        
            if( hittarget is HitTargetEditHandle )
            {
                Test.Debug(  "It's an edit handle :-O" ); // Test.Debug
                editing3drot.InitiateHandleEdit( mousex, mousey, ( ( HitTargetEditHandle) hittarget ).axis );
            }
            else
            {
                Test.Debug(  "Not an edit handle" ); // Test.Debug
                editing3drot.InitiateFreeEdit( mousex, mousey );
            }
        }
        
        public void UpdateTranslateEdit( bool bAltAxes, int mousex, int mousey )
        {
            EditingPreliminaries();
        
            if( currentedittype == EditType.PosFree && bAxisModifierOn == bAltAxes )
            {
                editing3dpos.InteractiveFreeEdit( bAltAxes, mousex, mousey );
            }
            else if( currentedittype == EditType.PosHandle )
            {
                editing3dpos.InteractiveHandleEdit( currentaxis, mousex, mousey );
            }
            else
            {
                InitiateTranslateEdit( mousex, mousey );
                bAxisModifierOn = bAltAxes;
            }
        }
        
        public void UpdateScaleEdit( bool bAltAxes, int mousex, int mousey )
        {
            EditingPreliminaries();
        
            if( currentedittype == EditType.ScaleFree && bAxisModifierOn == bAltAxes )
            {
                editing3dscale.InteractiveFreeEdit( mousex, mousey );
            }
            else if( currentedittype == EditType.ScaleHandle )
            {
                editing3dscale.InteractiveHandleEdit( currentaxis, mousex, mousey );
            }
            else
            {
                InitiateScaleEdit( mousex, mousey );
                bAxisModifierOn = bAltAxes;
            }
        }
        
        public void UpdateRotateEdit( bool bAltAxes, int mousex, int mousey )
        {
            EditingPreliminaries();
        
            if( currentedittype == EditType.RotFree && bAxisModifierOn == bAltAxes )
            {
                editing3drot.InteractiveFreeEdit( mousex, mousey );
            }
            else if( currentedittype == EditType.RotHandle )
            {
                editing3drot.InteractiveHandleEdit( currentaxis, mousex, mousey );
            }
            else
            {
                InitiateRotateEdit( mousex, mousey );
                bAxisModifierOn = bAltAxes;
            }
        }
        
        public void EditDone()
        {
            currentedittype = EditType.None;
        }

        public void DrawEditBarsToOpenGL(Vector3 camerapos)
        {
            if( bShowEditBars )
            {
                Entity entity = selectionmodel.GetFirstSelectedEntity();
                if( entity == null )
                {
                    return;
                }
                
                IGraphicsHelper graphics = GraphicsHelperFactory.GetInstance();
                
                graphics.PushMatrix();
    
                if( entity.Parent == null )
                {
                    //entity.DrawSelected();
                }
                else
                {
                    ( entity.Parent as EntityGroup ).ApplyTransforms();
                }
    
                graphics.Translate( entity.pos );
                graphics.Rotate( entity.rot );
    
               // Vector3[] FaceCentreOffsets = new Vector3[6];

                double distance = (entity.pos - camerapos).Det();
    
                Vector3 ScaleToUse = entity.scale;
                
                switch( editbartype )
                {
                    case EditBarType.Pos:
                        editing3dpos.DrawEditHandles( ScaleToUse, distance );
                        //editing3dscale.DrawEditHandles(ScaleToUse, distance);
                        break;
    
                    case EditBarType.Scale:
                        editing3dscale.DrawEditHandles( ScaleToUse, distance );
                        break;
    
                    case EditBarType.Rot:
                        editing3drot.DrawEditHandles( ScaleToUse );
                        break;
    
                }
    
                graphics.PopMatrix();
            }
        }
        
        public void ShowEditPosBars()
        {
            editbartype = EditBarType.Pos;
            bShowEditBars = true;
        }
        
        public void ShowEditScaleBars()
        {
            editbartype = EditBarType.Scale;
            bShowEditBars = true;
        }
        
        public void ShowEditRotBars()
        {
            editbartype = EditBarType.Rot;
            bShowEditBars = true;
        }
        
        public void HideEditBars()
        {
            editbartype = EditBarType.None;
            bShowEditBars = false;
        }        
    }
}
