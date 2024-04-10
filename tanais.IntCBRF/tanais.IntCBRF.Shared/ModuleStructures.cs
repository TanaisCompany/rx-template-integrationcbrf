using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace Tanais.IntCBRF.Structures.Module
{

  /// <summary>
  /// Структура для лога.
  /// </summary>
  partial class Log
  {    
    /// <summary>
    /// Код системыы.
    /// </summary>
    public string SystemCode { get; set; }
    
    /// <summary>
    /// Результат.
    /// </summary>
    public string EventResult { get; set; }
    
    /// <summary>
    /// сообщение
    /// </summary>
    public string EventMessage { get; set; }
  }

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

}