﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Serialization;

#region Класс получения информации по валютам
[XmlRoot(ElementName="Item")]
public class Item {

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
public class Currency {

  [XmlElement(ElementName="Item")]
  public List<Item> Items { get; set; }

  [XmlAttribute(AttributeName="name")]
  public string Name { get; set; }

  [XmlText]
  public string Text { get; set; }
}
#endregion

#region Класс получения информации по банкам
[XmlRoot(ElementName="Record")]
public class Record {

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
public class Bank {

  [XmlElement(ElementName="Record")]
  public List<Record> Records { get; set; }

  [XmlAttribute(AttributeName="name")]
  public string Name { get; set; }

  [XmlText]
  public string Text { get; set; }
}
#endregion

namespace tanais.IntCBRF.Server
{
  
  public class ModuleFunctions
  {
    /// <summary>
    /// Записать результат интеграции в лог.
    /// </summary>
    /// <param name="result">Результат интеграции.</param>
    /// <param name="message">Текст результата.</param>
    /// <param name="sendNotice">Отправлять ли задачу ответственному или администратору.</param>
    public virtual void WriteToExecLog(string result, string message, bool sendNotice)
    {
      // Создана для переопределения в перекрытиях и записи результатов синхронизации.
      // Запись в стандартный лог.
      if (result == IntCBRF.PublicConstants.Module.EventResult.Error)
        Logger.ErrorFormat("SystemCode = {0}, EventResult = {1}, EventMessage = {2}", IntCBRF.PublicConstants.Module.SystemCode, result, message);
      else
        Logger.DebugFormat("SystemCode = {0}, EventResult = {1}, EventMessage = {2}", IntCBRF.PublicConstants.Module.SystemCode, result, message);
      
      // Отправка уведомления администратору или ответственному за интеграцию.
      if (sendNotice)
        SendNotice(message);
    }
    
    /// <summary>
    /// Отправить уведомление ответственному за интеграцию или администратору.
    /// </summary>
    /// <param name="message">Текст уведомления.</param>
    [Public]
    public static void SendNotice(string message)
    {
      // Роль ответственного за интеграцию.
      var role = Sungero.CoreEntities.Roles.GetAll(r => Equals(r.Sid, tanais.IntCBRF.PublicConstants.Module.IntegrationResponsible)).FirstOrDefault();
      var responsibleList = Sungero.CoreEntities.Roles.GetAllUsersInGroup(role).ToList();
      var subject = tanais.IntCBRF.Resources.SubjectNotice;
      
      // Если ответственный не определен, то отправляется системному администратору.
      if (responsibleList.Count() == 0)
        responsibleList.Add(Users.GetAll().Where(a => a.Login.LoginName == "Administrator").FirstOrDefault());
      
      var notice = Sungero.Workflow.SimpleTasks.CreateWithNotices(subject, responsibleList.ToArray());
      notice.ActiveText = message;
      notice.Start();
    }
    
    /// <summary>
    /// Обработка данных о банке.
    /// </summary>
    /// <param name="bank">Данные из запроса к ЦБ РФ о банке.</param>
    /// <returns>Результат выполнения.</returns>
    public virtual int ProcessBank(Record bank)
    {
      // Проверить наличие банка.
      var bankRef = Sungero.Parties.Banks.GetAll().Where(r => r.BIC == bank.Bic).FirstOrDefault();
      // Базовый результат если бы нашли запись и не изменяли ее.
      var result = IntCBRF.PublicConstants.Module.ProcessResult.NotChanged;
      
      if (bankRef == null)
      {
        // Создание записи о банке.
        bankRef = Sungero.Parties.Banks.Create();
        result = IntCBRF.PublicConstants.Module.ProcessResult.Created;
      }
      else
      {
        // Проверяем на блокировку.
        if (Locks.GetLockInfo(bankRef).IsLocked)
        {
          var resultText = IntCBRF.Resources.PrefixBanks + tanais.IntCBRF.Resources.BlockedEntityFormat(bankRef.Id);
          WriteToExecLog(IntCBRF.PublicConstants.Module.EventResult.Error, resultText, false);
          return IntCBRF.PublicConstants.Module.ProcessResult.Error;
        }
      }
      
      // Проверки для изменения только расхождений в записи и XML.
      if (bankRef.Name != bank.ShortName)
        bankRef.Name = bank.ShortName;
      if (bankRef.LegalName != bank.ShortName)
        bankRef.LegalName = bank.ShortName;
      if (bankRef.BIC != bank.Bic)
        bankRef.BIC = bank.Bic;
      
      try
      {
        if (bankRef.State.IsChanged)
        {
          bankRef.Save();
          // Если запись уже была найдена (не создана), но мы ее сохранили, то записываем как измененную.
          if (result == IntCBRF.PublicConstants.Module.ProcessResult.NotChanged)
            result = IntCBRF.PublicConstants.Module.ProcessResult.Updated;
        }
      }
      catch (Exception ex)
      {
        // Запись данных об ошибке в лог.
        result = tanais.IntCBRF.PublicConstants.Module.ProcessResult.Error;
        var resultText = IntCBRF.Resources.PrefixBanks + IntCBRF.Resources.ExecuteConnectErrorFormat(ex.Message);
        WriteToExecLog(IntCBRF.PublicConstants.Module.EventResult.Error, resultText, false);
        return result;
      }
      
      return result;
    }
    
    /// <summary>
    /// Синхронизация банков с сайта ЦБ РФ.
    /// </summary>
    [Public]
    public void CBRSynchronizationBanks()
    {
      // Счетчик для результата обработки записи.
      var counter = Structures.Module.CounterProcess.Create();
      
      // Получить xml с сайта ЦБ РФ с информацией по валютам.
      var xml = GetXMLFromCBR(IntCBRF.Resources.CaseBanks);
      
      if (!string.IsNullOrEmpty(xml))
      {
        // Дессереализация XML.
        var serializer = new XmlSerializer(typeof(Bank));
        Bank result;
        
        using (TextReader reader = new StringReader(xml))
        {
          result = (Bank)serializer.Deserialize(reader);
        }

        if (result.Records.Count > 0)
        {
          // Обработка полученных записей.
          foreach (var bankRecord in result.Records)
          {
            var processResult = ProcessBank(bankRecord);
            // Запись результата в счетчик.
            counter.Total++;
            switch (processResult)
            {
              case Constants.Module.ProcessResult.Created:
                counter.Created++;
                break;
              case Constants.Module.ProcessResult.Updated:
                counter.Updated++;
                break;
              case Constants.Module.ProcessResult.Error:
                counter.Error++;
                break;
              case Constants.Module.ProcessResult.NotChanged:
                counter.NotChanged++;
                break;
              default:
                break;
            }
          }
        }
        
        // Запись в лог. Уведомление администратору или ответственному только если были ошибки синхронизации.
        var SendNotice = counter.Error > 0;
        
        var message = IntCBRF.Resources.PrefixBanks + tanais.IntCBRF.Resources.ProcessImportCountFormat(counter.Total, counter.Created, counter.Updated, counter.NotChanged, counter.Error);
        WriteToExecLog(IntCBRF.PublicConstants.Module.EventResult.Success, message, SendNotice);
        
      }
    }
    
    /// <summary>
    /// Обработка данных о валюте.
    /// </summary>
    /// <param name="bank">Данные из запроса к ЦБ РФ о банке.</param>
    /// <returns>Результат выполнения.</returns>
    public virtual int ProcessCurrency(Item curr)
    {
      var numericCode = curr.ISONumCode;
      // Скорректировать номер до 3 знаков
      if (curr.ISONumCode != null && curr.ISONumCode.Length == 2)
        numericCode = string.Format("0{0}", curr.ISONumCode);
      
      // Проверить наличие валюты
      var currency = Sungero.Commons.Currencies.GetAll().Where(r => r.AlphaCode == curr.ISOCharCode && r.NumericCode == numericCode).FirstOrDefault();
      // Базовый результат если бы нашли запись и не изменяли ее.
      var result = IntCBRF.PublicConstants.Module.ProcessResult.NotChanged;
      
      if (currency == null)
      {
        // Создание записи о валюте.
        currency = Sungero.Commons.Currencies.Create();
        result = IntCBRF.PublicConstants.Module.ProcessResult.Created;
      }
      else
      {
        // Проверяем на блокировку.
        if (Locks.GetLockInfo(currency).IsLocked)
        {
          var resultText = IntCBRF.Resources.PrefixBanks + tanais.IntCBRF.Resources.BlockedEntityFormat(currency.Id);
          WriteToExecLog(IntCBRF.PublicConstants.Module.EventResult.Error, resultText, false);
          return IntCBRF.PublicConstants.Module.ProcessResult.Error;
        }
      }
      
      // Проверки для изменения только расхождений в записи и XML.
      if (currency.Name != curr.Name)
        currency.Name = curr.Name;
      if (currency.AlphaCode != curr.ISOCharCode)
        currency.AlphaCode = curr.ISOCharCode;
      if (currency.ShortName != curr.Name)
        currency.ShortName = curr.Name;
      if (currency.Status != Sungero.Commons.Currency.Status.Active)
        currency.Status = Sungero.Commons.Currency.Status.Active;
      if (currency.NumericCode != numericCode)
        currency.NumericCode = numericCode;
      if (currency.FractionName != "*")
        currency.FractionName = "*";
      
      try
      {
        if (currency.State.IsChanged)
        {
          currency.Save();
          // Если запись уже была найдена (не создана), но мы ее сохранили, то записываем как измененную.
          if (result == IntCBRF.PublicConstants.Module.ProcessResult.NotChanged)
            result = IntCBRF.PublicConstants.Module.ProcessResult.Updated;
        }
      }
      catch (Exception ex)
      {
        // Запись данных об ошибке в лог.
        result = tanais.IntCBRF.PublicConstants.Module.ProcessResult.Error;
        var resultText = IntCBRF.Resources.PrefixCurrencies + IntCBRF.Resources.ExecuteConnectErrorFormat(ex.Message);
        WriteToExecLog(IntCBRF.PublicConstants.Module.EventResult.Error, resultText, false);
        return result;
      }
      
      return result;
    }
    
    /// <summary>
    /// Синхронизация валют с сайта ЦБ РФ
    /// </summary>
    [Public]
    public void CBRSynchronizationCurrencies()
    {
      // Счетчик для результата обработки записи.
      var counter = Structures.Module.CounterProcess.Create();
      
      // Получить xml с сайта ЦБ РФ с информацией по валютам
      string xml = GetXMLFromCBR(IntCBRF.Resources.CaseCurrencies);
      
      if (!string.IsNullOrEmpty(xml))
      {
        // Дессереализация XML.
        var serializer = new XmlSerializer(typeof(Currency));
        Currency result;
        
        using (TextReader reader = new StringReader(xml))
        {
          result = (Currency)serializer.Deserialize(reader);
        }

        if (result.Items.Count > 0)
        {
          // Обработка полученных записей.
          foreach (var curr in result.Items.Where(f => !string.IsNullOrEmpty(f.ISOCharCode) && !string.IsNullOrEmpty(f.ISONumCode)))
          {
            var processResult = ProcessCurrency(curr);
            // Запись результата в счетчик.
            counter.Total++;
            switch (processResult)
            {
              case Constants.Module.ProcessResult.Created:
                counter.Created++;
                break;
              case Constants.Module.ProcessResult.Updated:
                counter.Updated++;
                break;
              case Constants.Module.ProcessResult.Error:
                counter.Error++;
                break;
              case Constants.Module.ProcessResult.NotChanged:
                counter.NotChanged++;
                break;
              default:
                break;
            }
          }
        }
        
        // Запись в лог. Уведомление администратору или ответственному только если были ошибки синхронизации.
        var SendNotice = counter.Error > 0;
        
        var message = IntCBRF.Resources.PrefixCurrencies + tanais.IntCBRF.Resources.ProcessImportCountFormat(counter.Total, counter.Created, counter.Updated, counter.NotChanged, counter.Error);
        WriteToExecLog(IntCBRF.PublicConstants.Module.EventResult.Success, message, SendNotice);
      }
    }
    
    /// <summary>
    /// Получить настройки синхронизации.
    /// </summary>
    /// <param name="Case">Вариант получения данных.</param>
    /// <returns>Структура с настройками с префексом для логирования, ссылкой подключения и типом кодировки.</returns>
    public virtual Structures.Module.Settings GetSettings(string Case)
    {
      // Получаение настроек подключения.
      var settings = Structures.Module.Settings.Create();
      settings.EncodingStandart = Constants.Module.EncodingStandart;
      
      switch (Case)
      {
          // Вариант валют.
          case Constants.Module.CaseCurrencies: {
            settings.Address = IntCBRF.PublicConstants.Module.AddressCBRCurrencies;
            settings.CaseLog = IntCBRF.Resources.PrefixCurrencies;
            break;
          }
          // Вариант Банков.
          case Constants.Module.CaseBanks:  {
            settings.Address = IntCBRF.PublicConstants.Module.AddressCBRBanks;
            settings.CaseLog = IntCBRF.Resources.PrefixBanks;
            break;
          }
          default: {
            break;
          }
      }
      
      return settings;
    }

    /// <summary>
    /// Получить данные с сайта ЦБ РФ
    /// </summary>
    /// <param name="Case">Вариант получения данных (для какой сущности).</param>
    /// <returns>XML с сайта.</returns>
    public string GetXMLFromCBR(string Case)
    {
      var xml = string.Empty;
      var client = new System.Net.WebClient();
      
      // Получить информацию по банкам/валютам
      var settings = GetSettings(Case);
      
      // Получить данные с сайта ЦБ РФ
      try
      {
        xml = Encoding.GetEncoding(settings.EncodingStandart).GetString(client.DownloadData(settings.Address));
      }
      catch (Exception ex)
      {
        var resultText = IntCBRF.Resources.ExecuteConnectErrorFormat(ex.Message);
        var eventResult = IntCBRF.PublicConstants.Module.EventResult.Error;
        WriteToExecLog(eventResult, resultText, false);
      }
      
      return xml;
    }
    
  }
}