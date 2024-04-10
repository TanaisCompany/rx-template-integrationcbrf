using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.IO;
using System.Text;

namespace Tanais.IntCBRF.Client
{
  public class ModuleFunctions
  { 

    /// <summary>
    /// 
    /// </summary>
    public virtual void ShowSettings()
    {
      CBRFSettingses.GetAll().First().Show();
    }

  }
}