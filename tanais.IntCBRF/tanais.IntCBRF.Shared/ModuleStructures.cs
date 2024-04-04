using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace tanais.IntCBRF.Structures.Module
{

  /// <summary>
  /// Счетчик результатов обработки.
  /// </summary>
  partial class CounterProcess
  {
    /// <summary>
    /// кол-во обновленных.
    /// </summary>
    public int Updated { get; set; }
    
    /// <summary>
    /// Кол-во созданных.
    /// </summary>
    public int Created { get; set; }
    
    /// <summary>
    /// Кол-во ошибок.
    /// </summary>
    public int Error { get; set; }
    
    /// <summary>
    /// Кол-во записей без изменений.
    /// </summary>
    public int NotChanged { get; set; }
    
    /// <summary>
    /// Общее кол-во.
    /// </summary>
    public int Total { get; set; }
  }

  /// <summary>
  /// Настройки синхронизации ЦБ РФ.
  /// </summary>
  partial class Settings
  {
    /// <summary>
    /// Ссылка для получения данных.
    /// </summary>
    public string Address { get; set; }
    
    /// <summary>
    /// Префикс логирования.
    /// </summary>
    public string CaseLog { get; set; }
    
    /// <summary>
    /// Стандарт кодировки XML.
    /// </summary>
    public string EncodingStandart { get; set; }
  }

}