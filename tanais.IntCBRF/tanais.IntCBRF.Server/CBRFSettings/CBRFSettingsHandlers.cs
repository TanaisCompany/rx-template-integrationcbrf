using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Tanais.IntCBRF.CBRFSettings;

namespace Tanais.IntCBRF
{
  partial class CBRFSettingsServerHandlers
  {

    public override void BeforeSave(Sungero.Domain.BeforeSaveEventArgs e)
    {
      _obj.Name = _obj.AddressCBRBanks + _obj.AddressCBRCurrencies;
    }
  }


}