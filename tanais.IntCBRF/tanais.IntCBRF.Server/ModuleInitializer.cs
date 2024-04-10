using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace Tanais.IntCBRF.Server
{
  public partial class ModuleInitializer
  {

    public override bool IsModuleVisible()
    {
      return Users.Current.IncludedIn(Roles.Administrators);
    }

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateRoles();
      FillSettingRecord();
    }
    
    /// <summary>
    /// Создать роли.
    /// </summary>
    public virtual void CreateRoles()
    {
      InitializationLogger.Debug("Init: CBRF. Create roles.");
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(
        Tanais.IntCBRF.Resources.IntegrationResponsible, Tanais.IntCBRF.Resources.IntegrationResponsible, Tanais.IntCBRF.PublicConstants.Module.IntegrationResponsible);
    }
    
    /// <summary>
    /// Заполнить записи настроечного справочника.
    /// </summary>
    public virtual void FillSettingRecord()
    {
      var settings = IntCBRF.CBRFSettingses.GetAll();
      if (!settings.Any())
      {
        var newRecord = IntCBRF.CBRFSettingses.Create();
        newRecord.AddressCBRBanks = Constants.Module.AddressCBRBanks;
        newRecord.AddressCBRCurrencies = Constants.Module.AddressCBRCurrencies;
        newRecord.SendNotice = IntCBRF.CBRFSettings.SendNotice.Errors;
        newRecord.Save();
      }
    }
  }

}
