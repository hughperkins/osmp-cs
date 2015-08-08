using System;
using System.Collections.Generic;
using System.Text;
using SdlDotNet;

namespace OSMP
{
    // in progress; the idea is that this can be used for
    // entities, like draw handles, to be clickable, in a publisher-subscriber
    // type way; as for standard GUI technologies
    // not implemented yet
    public class Layer2MouseEvents
    {
        static Layer2MouseEvents instance = new Layer2MouseEvents();
        public static Layer2MouseEvents GetInstance() { return instance; }

        Layer2MouseEvents()
        {
            MouseCache.GetInstance().MouseDown += new SdlDotNet.MouseButtonEventHandler(Layer2MouseEvents_MouseDown);
            MouseCache.GetInstance().MouseUp += new SdlDotNet.MouseButtonEventHandler(Layer2MouseEvents_MouseUp);
            MouseCache.GetInstance().MouseMove += new MouseMoveHandler(Layer2MouseEvents_MouseMove);
        }

        Entity targetentity = null;

        void Layer2MouseEvents_MouseMove()
        {
        }

        void Layer2MouseEvents_MouseUp(object sender, SdlDotNet.MouseButtonEventArgs e)
        {
        }

        void Layer2MouseEvents_MouseDown(object sender, SdlDotNet.MouseButtonEventArgs e)
        {
            targetentity = Picker3dController.GetInstance().GetClickedEntity(e.X, e.Y);
        }
    }
}
