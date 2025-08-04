using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Xml.Serialization;

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

  #region Класс получения информации по валютам
  [XmlRoot(ElementName="Item")]
  partial class Item {

    [XmlElement(ElementName="Name")]
    public string Name { get; set; }

    [XmlElement(ElementName="EngName")]
    public string EngName { get; set; }

    [XmlElement(ElementName="Nominal")]
    public string Nominal { get; set; }

    [XmlElement(ElementName="ParentCode")]
    public string ParentCode { get; set; }

    [XmlElement(ElementName="ISO_Num_Code")]
    public string ISONumCode { get; set; }

    [XmlElement(ElementName="ISO_Char_Code")]
    public string ISOCharCode { get; set; }

    [XmlAttribute(AttributeName="ID")]
    public string ID { get; set; }

    [XmlText]
    public string Text { get; set; }
  }

  [XmlRoot(ElementName="Valuta")]
  partial class Currency {

    [XmlElement(ElementName="Item")]
    public List<Tanais.IntCBRF.Structures.Module.Item> Items { get; set; }

    [XmlAttribute(AttributeName="name")]
    public string Name { get; set; }

    [XmlText]
    public string Text { get; set; }
  }
  #endregion

  #region Класс получения информации по банкам
  [XmlRoot(ElementName="Record")]
  partial class Record {

    [XmlElement(ElementName="ShortName")]
    public string ShortName { get; set; }

    [XmlElement(ElementName="Bic")]
    public string Bic { get; set; }

    [XmlAttribute(AttributeName="ID")]
    public string ID { get; set; }

    [XmlAttribute(AttributeName="DU")]
    public string DU { get; set; }

    [XmlText]
    public string Text { get; set; }
  }

  [XmlRoot(ElementName="BicCode")]
  partial class Bank {

    [XmlElement(ElementName="Record")]
    public List<Tanais.IntCBRF.Structures.Module.Record> Records { get; set; }

    [XmlAttribute(AttributeName="name")]
    public string Name { get; set; }

    [XmlText]
    public string Text { get; set; }
  }
  #endregion
}