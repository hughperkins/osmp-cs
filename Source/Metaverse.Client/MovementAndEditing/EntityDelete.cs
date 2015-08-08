using System;
using System.Collections.Generic;
using System.Text;
using Metaverse.Utility;

namespace OSMP
{
    public class EntityDelete
    {
        static EntityDelete instance = new EntityDelete();
        public static EntityDelete GetInstance() { return instance; }

        EntityDelete()
        {
            ContextMenuController.GetInstance().ContextMenuPopup += new ContextMenuHandler(ContextMenuPopup);
        }

        Entity entity;

        public void ContextMenuPopup(object source, ContextMenuArgs e)
        {
            entity = e.Entity;
            if (entity != null)
            {
                LogFile.WriteLine("EntityDelete registering in contextmenu");
                ContextMenuController.GetInstance().RegisterContextMenu(new string[] { "Delete" }, new ContextMenuHandler(DeleteClick));
            }
        }

        public void DeleteClick(object source, ContextMenuArgs e)
        {
            MetaverseClient.GetInstance().worldstorage.DeleteEntity(entity);
        }
    }
}
