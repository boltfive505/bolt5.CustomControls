﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace bolt5.CustomHtmlCefEditor
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ObjectForScriptingHelper
    {
        public event Action ContentChange;

        public void notifyContentChanged()
        {
            ContentChange?.Invoke();
        }
    }
}
