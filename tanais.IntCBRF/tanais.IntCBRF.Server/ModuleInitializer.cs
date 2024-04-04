using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace tanais.IntCBRF.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      CreateRoles();
    }
    
    /// <summary>
    /// Создать роли.
    /// </summary>
    public virtual void CreateRoles()
    {
      InitializationLogger.Debug("Init: CBRF. Create roles.");
      
      Sungero.Docflow.PublicInitializationFunctions.Module.CreateRole(
        tanais.IntCBRF.Resources.IntegrationResponsible, tanais.IntCBRF.Resources.IntegrationResponsible, tanais.IntCBRF.PublicConstants.Module.IntegrationResponsible);
    }
  }

}
