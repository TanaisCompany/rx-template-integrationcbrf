# rx-template-integrationcbrf
Репозиторий с шаблоном разработки «Интеграция с ЦБ РФ».

## Описание
Шаблон позволяет:
* настроить расписание и параметры выполнения фоновых процессов интеграции
* автоматически создавать и обновлять банки и валюты, опубликованные на сайте ЦБ РФ

Состав объектов разработки:
* Модуль «Интеграция с ЦБ РФ» с обложкой для вызова настроек
* Справочник «ЦБРФ настройки» 
* Фоновый процесс «Интеграция с ЦБ РФ. Синхронизация валют» 
* Фоновый процесс «Интеграция с ЦБ РФ. Синхронизация банков» 

Поскольку шаблон разработки не содержит перекрытий объектов коробочного решения, конфликты при публикации не возникнут. Это позволяет использовать функциональность, как при старте нового проекта, так и в ходе сопровождения существующих инсталляций системы.

## Настройка решения
1. Модуль «Интеграция с ЦБ РФ» - доступен только администраторам. Настройку могут выполнять только администраторы.
2. В записи справочника «ЦБРФ настройки» требуется выставить нужный уровень логирования.
3. При отправке задачи используется исполнитель из роли «ЦБРФ. Ответственный за обработку интеграции». Требуется указать пользователя, кому будут приходить задачи с ошибками  интеграции. Если пользователь не указан - задачи будут отправляться пользователю Administrator.

## Варианты расширения функциональности на проектах
1. Модификация функции GetConnectionSettings: Изменение способа получения адресов для подключения к сервисам ЦБ РФ.
Использовать, если настройки необходимо хранить в другом месте.

Пример:
``` C#
	public virtual string GetConnectionSettings(string Case)
	{      
      switch (Case)
      {
          // Вариант валют.
          case Tanais.IntCBRF.PublicConstants.Module.CaseCurrencies: {
			var addressCBRCurrencies = ...; // Получить строку подключения к сервису с валютами
            return addressCBRCurrencies;
          }
          // Вариант Банков.
          case Tanais.IntCBRF.PublicConstants.Module.CaseBanks:  {
            var addressCBRBanks = ...; // Получить строку подключения к сервису с банками
			return addressCBRBanks;
          }
          default: {
            break;
          }
      }
      
      return string.Empty;
	  
	}
```
Параметр Case может принимать значение Constants.Module.CaseCurrencies (для валют) и Constants.Module.CaseBanks (для банков)
В функции необходимо проверять значение Case и получить строку подключения из альтеративного места хранения. Вернуть в виде строки.

Возмжожен варинат использованияв виде перекрытия функции


2. Модификация функции WriteToExecLog: Изменение порядка и формата записи лога
Параметры:
result - одно из свойств статического класса EventResult (IntCBRF.PublicConstants.Module.EventResult)
message - строка с фиксируемым в лог текстом
sendNotice - необходимость отправки задачи ответсвенному/администратору для реакции на ошибку

Используется, если требуется фиксировать информацию в лог в другом формате или в альтернативном месте, например, в справочнике с логами или истории.

Пример:
``` C#
	public virtual void WriteToExecLog(string result, string message, bool sendNotice)
	{      
     
	  // Проверка результата
	  if (result == Tanais.IntCBRF.PublicConstants.Module.EventResult.Error)
        ... // Реализация произвольного фиксирования лога в случае ошибки ( использовать значение параметра message )
      else
        ... // Реализация произвольного фиксирования лога в остальных случах ( использовать значение параметра message )
      
      // Отправка уведомления администратору или ответственному за интеграцию.
      if (sendNotice) 
        SendNotice(message); 
	  
	}
```

Возмжожен варинат использованияв виде перекрытия функции

3. Модификация функции ProcessBank: Изменение обработки полученных данных о банках.
параметр bank - структура, содержит информацию о банке, полученную из сервиса

Может использоваться, если базовая сущность Sungero.Parties.Banks перекрыта и для правильного создания записи нужен нестандартный порядок действий, в т.ч. дозаполнение каких-то полей

Пример при использовании перекрытого справочника Банки:
``` C#
	public virtual int ProcessBank(Record bank)
	{      
      // Проверить наличие банка.
      var bankRef = Sungero.Parties.Banks.GetAll().Where(r => r.BIC == bank.Bic).FirstOrDefault();   // использовать вместо Sungero.Parties.Banks перекрытый на прикладном уровне справочник Банки
      // Базовый результат если бы нашли запись и не изменяли ее.
      var result = Tanais.IntCBRF.PublicConstants.Module.ProcessResult.NotChanged;
      
      if (bankRef == null)
      {
        // Создание заTanais.писи о банке.
        bankRef = Sungero.Parties.Banks.Create();  // использовать вместо Sungero.Parties.Banks перекрытый на прикладном уровне справочник Банки
        result = Tanais.IntCBRF.PublicConstants.Module.ProcessResult.Created;
      }
      else
      {
        // Проверяем на блокировку.
        if (Locks.GetLockInfo(bankRef).IsLocked)
        {
          var resultText = Tanais.IntCBRF.Resources.PrefixBanks + Tanais.IntCBRF.Resources.BlockedEntityFormat(bankRef.Id);
          WriteToExecLog(Tanais.IntCBRF.PublicConstants.Module.EventResult.Error, resultText, false);
          return Tanais.IntCBRF.PublicConstants.Module.ProcessResult.Error;
        }
      }
      
      // Проверки для изменения только расхождений в записи и XML.
      if (bankRef.Name != bank.ShortName)
        bankRef.Name = bank.ShortName;
      if (bankRef.LegalName != bank.ShortName)
        bankRef.LegalName = bank.ShortName;
      if (bankRef.BIC != bank.Bic)
        bankRef.BIC = bank.Bic;
      
	  ... // Дополнить код дозаполнения записи перекрытого справочника Банки
	  
      try
      {
        if (bankRef.State.IsChanged)
        {
          bankRef.Save();
          // Если запись уже была найдена (не создана), но мы ее сохранили, то записываем как измененную.
          if (result == Tanais.IntCBRF.PublicConstants.Module.ProcessResult.NotChanged)
            result = Tanais.IntCBRF.PublicConstants.Module.ProcessResult.Updated;
        }
      }
      catch (Exception ex)
      {
        // Запись данных об ошибке в лог.
        result = Tanais.IntCBRF.PublicConstants.Module.ProcessResult.Error;
        var resultText = Tanais.IntCBRF.Resources.PrefixBanks + Tanais.IntCBRF.Resources.ExecuteConnectErrorFormat(ex.Message);
        WriteToExecLog(Tanais.IntCBRF.PublicConstants.Module.EventResult.Error, resultText, false);
        return result;
      }
      
      return result;
	  
	}
```

4. Модификация функции ProcessCurrency: Изменение обработки полученных данных о валютах.
параметр curr - структура, содержит информацию о валюте, полученную из сервиса

Может использоваться, если базовая сущность Sungero.Commons.Currencies перекрыта и для правильного создания записи нужен нестандартный порядок действий, в т.ч. дозаполнение каких-то полей

Пример при использовании перекрытого справочника Валюты:
``` C#
	public virtual int ProcessCurrency(Item curr)
    {
      var numericCode = curr.ISONumCode;
      // Скорректировать номер до 3 знаков
      if (curr.ISONumCode != null && curr.ISONumCode.Length == 2)
        numericCode = string.Format("0{0}", curr.ISONumCode);
      
      // Проверить наличие валюты
      var currency = Sungero.Commons.Currencies.GetAll().Where(r => r.AlphaCode == curr.ISOCharCode && r.NumericCode == numericCode).FirstOrDefault();  // использовать вместо Sungero.Commons.Currencies перекрытый на прикладном уровне справочник Валюты
      // Базовый результат если бы нашли запись и не изменяли ее.
      var result = Tanais.IntCBRF.PublicConstants.Module.ProcessResult.NotChanged;
      
      if (currency == null)
      {
        // Создание записи о валюте.
        currency = Sungero.Commons.Currencies.Create(); // использовать вместо Sungero.Commons.Currencies перекрытый на прикладном уровне справочник Валюты
        result = Tanais.IntCBRF.PublicConstants.Module.ProcessResult.Created;
      }
      else
      {
        // Проверяем на блокировку.
        if (Locks.GetLockInfo(currency).IsLocked)
        {
          var resultText = Tanais.IntCBRF.Resources.PrefixBanks + Tanais.IntCBRF.Resources.BlockedEntityFormat(currency.Id);
          WriteToExecLog(Tanais.IntCBRF.PublicConstants.Module.EventResult.Error, resultText, false);
          return Tanais.IntCBRF.PublicConstants.Module.ProcessResult.Error;
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
      
	  ... // Дополнить код дозаполнения записи перекрытого справочника Валюты
	  
      try
      {
        if (currency.State.IsChanged)
        {
          currency.Save();
          // Если запись уже была найдена (не создана), но мы ее сохранили, то записываем как измененную.
          if (result == Tanais.IntCBRF.PublicConstants.Module.ProcessResult.NotChanged)
            result = Tanais.IntCBRF.PublicConstants.Module.ProcessResult.Updated;
        }
      }
      catch (Exception ex)
      {
        // Запись данных об ошибке в лог.
        result = Tanais.Tanais.IntCBRF.PublicConstants.Module.ProcessResult.Error;
        var resultText = Tanais.IntCBRF.Resources.PrefixCurrencies + IntCBRF.Resources.ExecuteConnectErrorFormat(ex.Message);
        WriteToExecLog(Tanais.IntCBRF.PublicConstants.Module.EventResult.Error, resultText, false);
        return result;
      }
      
      return result;
    }
```

## Порядок установки
Для работы требуется установленный Directum RX версии 4.5 и выше. 

### Установка для ознакомления
1. Склонировать репозиторий rx-template-integrationcbrf в папку.
2. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" />
  <repository folderName="RX" solutionType="Base" url="<адрес локального репозитория>" />
  <repository folderName="<Папка из п.1>" solutionType="Work" 
     url="https://github.com/TanaisCompany/rx-template-integrationcbrf" />
</block>
```

### Установка для использования на проекте
Возможные варианты:

**A. Fork репозитория**
1. Сделать fork репозитория rx-template-integrationcbrf для своей учетной записи.
2. Склонировать созданный в п. 1 репозиторий в папку.
3. Указать в _ConfigSettings.xml DDS:
``` xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.2>" solutionType="Work" 
     url="<Адрес репозитория gitHub учетной записи пользователя из п. 1>" />
</block>
```

**B. Подключение на базовый слой.**

Вариант не рекомендуется, так как при выходе версии шаблона разработки не гарантируется обратная совместимость.
1. Склонировать репозиторий rx-template-integrationcbrf в папку.
2. Указать в _ConfigSettings.xml DDS:
``` xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.1>" solutionType="Base" 
     url="<Адрес репозитория gitHub>" />
  <repository folderName="<Папка для рабочего слоя>" solutionType="Work" 
     url="https://github.com/TanaisCompany/rx-template-integrationcbrf" />
</block>
```

**C. Копирование репозитория в систему контроля версий.**

Рекомендуемый вариант для проектов внедрения.
1. В системе контроля версий с поддержкой git создать новый репозиторий.
2. Склонировать репозиторий rx-template-integrationcbrf в папку с ключом `--mirror`.
3. Перейти в папку из п. 2.
4. Импортировать клонированный репозиторий в систему контроля версий командой:

`git push –mirror <Адрес репозитория из п. 1>`
