// Created by Hugh Perkins 2006
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
using System.IO;
using Gtk;
using Glade;
//using Gdk;
using Metaverse.Utility;

namespace OSMP
{
    public class FeaturesDialog
    {
        int picturewidth = 96;
        int pictureheight = 96;

        [Widget]
        Table featuretable = null;

        [Widget]
        Gtk.Window featurewindow = null;

        //const uint FI_RGBA_RED_MASK = 0x00FF0000;
        //const uint FI_RGBA_GREEN_MASK = 0x0000FF00;
        //const uint FI_RGBA_BLUE_MASK = 0x000000FF;
        //const uint FI_RGBA_ALPHA_MASK = 0xFF000000;

        public void on_btnSaveFeatures_clicked(object o, EventArgs e)
        {
            MainUI.GetInstance().uiwindow.InfoMessage("Save.  We're going to ask you for two filepaths.  One is the featurelist textfile and one is the feature data binary file");
            string featurefilepath = MainUI.GetInstance().uiwindow.GetFilePath("Save.  Please enter filepath for featurelist text file:", "features.tdf");
            if (featurefilepath != "")
            {
                string featurebinfilepath = MainUI.GetInstance().uiwindow.GetFilePath("Save.  Please enter filepath for feature data binary file:", "features.features");
                if (featurebinfilepath != "")
                {
                    FeaturePersistence.GetInstance().SaveFeatures(featurefilepath, featurebinfilepath);
                }
            }
        }

        public void on_btnLoadFeatures_clicked(object o, EventArgs e)
        {
            MainUI.GetInstance().uiwindow.InfoMessage("Load.  We're going to ask you for two filepaths.  One is the featurelist textfile and one is the feature data binary file");
            string featurefilepath = MainUI.GetInstance().uiwindow.GetFilePath("Load.  Please enter filepath for featurelist text file:", "features.tdf");
            if (featurefilepath != "")
            {
                string featurebinfilepath = MainUI.GetInstance().uiwindow.GetFilePath("Load.  Please enter filepath for feature data binary file:", "features.features");
                if (featurebinfilepath != "")
                {
                    FeaturePersistence.GetInstance().LoadFeatures(featurefilepath, featurebinfilepath);
                }
            }
        }

        void AddFeature(string filename, int featurenumber)
        {
            Directory.CreateDirectory("cache");
            string cachefilename = Path.GetFileNameWithoutExtension(filename).ToLower() + ".jpg";
            string cachefilepath = Path.Combine("cache", cachefilename);
            LogFile.GetInstance().WriteLine(cachefilepath);
            if (!File.Exists(cachefilepath))
            {
                byte[] unitpicbytes = new UnitPicCreator().CreateUnitPic(filename, picturewidth, pictureheight);

                Image image = new Image(picturewidth, pictureheight);
                //System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(picturewidth, pictureheight);
                for (int x = 0; x < picturewidth; x++)
                {
                    for (int y = 0; y < pictureheight; y++)
                    {
                        image.SetPixel(x, pictureheight - y - 1,
                            unitpicbytes[y * picturewidth * 4 + x * 4 + 0],
                            unitpicbytes[y * picturewidth * 4 + x * 4 + 1], 
                            unitpicbytes[y * picturewidth * 4 + x * 4 + 2],
                            255
                            );
                    }
                }
                image.Save(cachefilepath);
                //DevIL.DevIL.SaveBitmap(cachefilepath, bitmap);
            }

            // writing direct to pixbuf sortof works but gives corruption
            //Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(unitpicbytes, true, 8, picturewidth, pictureheight, picturewidth * 4, new Gdk.PixbufDestroyNotify(pixbufdestroynotify));
//            Gdk.Pixbuf pixbuf = new Gdk.Pixbuf(unitpicbytes, true, 8, picturewidth, pictureheight, picturewidth * 4, null);
            //Image newimage = new Image(pixbuf);

            Gtk.Image newimage = new Gtk.Image(Path.Combine("cache", cachefilename));
            string unitname = Path.GetFileNameWithoutExtension(filename).ToLower();
            Label label = new Label(unitname);
            VBox vbox = new VBox(false, 0);
            vbox.PackStart(newimage);
            vbox.PackEnd(label);
            Button newbutton = new Button(vbox);
            buttons.Add(newbutton);
            newbutton.Name = unitname;
            newbutton.Clicked += new EventHandler(buttonpressed);
            Unhighlight(newbutton);

            featuretable.Attach(newbutton, 0, 1, (uint)featurenumber - 1, (uint)featurenumber);
            featuretable.ShowAll();
        }

        List<Button> buttons = new List<Button>();

        void Highlight(Button button)
        {
            button.ModifyBg(StateType.Normal, new Gdk.Color(0, 255, 0));
            button.ModifyBg(StateType.Prelight, new Gdk.Color(0, 255, 0));
        }

        void Unhighlight(Button button)
        {
            button.ModifyBg(StateType.Normal, new Gdk.Color(210, 210, 210));
            button.ModifyBg(StateType.Prelight, new Gdk.Color(210, 210, 210));
        }

        void buttonpressed(object o, EventArgs args)
        {
            Button thisbutton = o as Button;
            Highlight(thisbutton);
            foreach (Button button in buttons)
            {
                Unhighlight(button);
            }

            string unitname = thisbutton.Name;
            LogFile.GetInstance().WriteLine(o.ToString() + " " + unitname + " pressed");

            if (!UnitCache.GetInstance().UnitsByName.ContainsKey(unitname))
            {
                Unit unit = new S3oLoader().LoadS3o("objects3d" + "/" + unitname + ".s3o");
                UnitCache.GetInstance().UnitsByName.Add(unitname, unit);
            }
            ( BrushEffectController.GetInstance().brusheffects[ typeof( AddFeature ) ] as AddFeature )
                .currentfeature = UnitCache.GetInstance().UnitsByName[unitname];
        }

        public FeaturesDialog()
        {
            Glade.XML app = new Glade.XML(EnvironmentHelper.GetExeDirectory() + "/TerrainEditing.glade", "featurewindow", "");
            app.Autoconnect(this);

            int numfeatures = Directory.GetFiles("objects3d").GetLength(0);
            featuretable.Resize((uint)numfeatures,1);
            int featurenumber = 1;
            foreach (string file in Directory.GetFiles("objects3d"))
            {
                LogFile.GetInstance().WriteLine(file);
                AddFeature( file, featurenumber);
                featurenumber++;
            }
            //featuretable.ResizeMode = ResizeMode.Immediate;
            featurewindow.ShowAll();
            featurewindow.Resize(picturewidth * 2 + 20, numfeatures * (pictureheight * 2 + 20));
        }

    }
}
