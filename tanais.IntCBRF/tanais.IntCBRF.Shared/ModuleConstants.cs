using System;
using Sungero.Core;

namespace Tanais.IntCBRF.Constants
{
  public static class Module
  {
    /// <summary>
    /// Валюты
    /// </summary>
    [Public]
    public const string CaseCurrencies = "Currencies";
    
    /// <summary>
    /// Банки
    /// </summary>
    [Public]
    public const string CaseBanks = "Banks";
    
    /// <summary>
    /// Наименование системы Контур Фокус.
    /// </summary>
    [Public]
    public const string SystemCode = "CBRF";

    /// <summary>
    /// Адрес для получения информации по банкам
    /// </summary>
    [Public]
    public const string AddressCBRBanks = "http://cbr.ru/scripts/XML_bic.asp";

    /// <summary>
    /// Адрес для получения информации по валютам
    /// </summary>
    [Public]
    public const string AddressCBRCurrencies = "https://www.cbr.ru/scripts/XML_valFull.asp";
    
    /// <summary>
    /// Стандарт кодировки получаемых XML от ЦБ РФ.
    /// </summary>
    [Public]
    public const string EncodingStandart = "Windows-1251";
    
    /// <summary>
    /// Guid роли Ответственный за обработку интеграции.
    /// </summary>
    [Public]
    public static readonly Guid IntegrationResponsible = Guid.Parse("17EFC61A-6CEF-4D4A-AF15-53C32A7ED334");
    
    /// <summary>
    /// Результат синхронизации.
    /// </summary>
    [Public]
    public static class EventResult
    {
      // Ошибка.
      [Public]
      public const string Error = "Error";
      
      // Успешно.
      [Public]
      public const string Success = "Success";
    }
    
    /// <summary>
    /// Результат обработки.
    /// </summary>
    [Public]
    public static class ProcessResult
    {      
      // Создана сущность.
      [Public]
      public const int Created = 1;
      
      // Изменена сущность.
      [Public]
      public const int Updated = 0;
      
      // Ошибка.
      [Public]
      public const int Error = 2;
      
      // Корректная запись, изменений не было.
      [Public]
      public const int NotChanged = 3;
    }

  }
}