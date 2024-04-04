using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Net;
using System.Xml.Linq;
using System.IO;
using System.Text;
namespace tanais.IntCBRF.Server
{
  public class ModuleJobs
  {

    /// <summary>
    /// Синхронизация банков
    /// </summary>
    public virtual void IntCBRSyncBanks()
    {
      IntCBRF.PublicFunctions.Module.CBRSynchronizationBanks();
    }

    /// <summary>
    /// Синхронизация валют
    /// </summary>
    public virtual void IntCBRSyncCurrencies()
    {
      IntCBRF.PublicFunctions.Module.CBRSynchronizationCurrencies();
    }
    
  }

}
