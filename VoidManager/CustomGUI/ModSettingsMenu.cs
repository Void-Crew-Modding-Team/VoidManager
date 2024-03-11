using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidManager.CustomGUI
{
    public abstract class ModSettingsMenu
    {
        public abstract string Name();
        public abstract void Draw();
        public virtual void OnOpen() { }
        public virtual void OnClose() { }
    }
}
