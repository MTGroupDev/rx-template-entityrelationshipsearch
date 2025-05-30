# rx-template-entityrelationshipsearch
Шаблон разработки "Поиск связей сущности".
Решение для Directum RX 4.12 и выше
____
# Описание шаблона

Решение предоставляет отдельный отчет, который наглядно отображает связи выбранной сущности с другими объектами системы. Это позволяет анализировать взаимосвязи и обеспечивает безопасное удаление объектов без нарушения структуры данных.
____
# Состав решения
 
-Модуль «Поиск связей сущностей».

-Отчет «Поиск связей сущностей» (EntityRelashionshipSearchReport).

-Перекрытие справочника «Контрагенты» (Counterparty).

-Локализация новых элементов разработки.
____
# Варианты расширения функциональности на проектах.

-Добавление отдельных справочников для отображения наименований свойств-коллекций в объектах системы;

-Поиск связанных документов с использованием механизма «Связи» (например, Приложение, Основание и т.д.);

-Расширение круга пользователей, имеющих доступ к отчету, за счет доработки проверки прав в SQL-запросах.
____
# Архитектурно неочевидные моменты

-Доступ к отчету рекомендуется предоставлять только администраторам системы, поскольку SQL-запросы выполняются без проверки прав доступа.

-Для работы с функционалом в перекрытых объектах системы (например, документы и справочники) необходимо использовать клиентскую функцию OpenEntityRelationshipSearchReport, передавая в нее объект.


## Порядок установки

Для работы требуется установленный Directum RX версии 4.12.

### Установка для ознакомления
1. Склонировать репозиторий rx-template-entityrelationshipsearch в папку.
2. Указать в _ConfigSettings.xml DDS:
``` xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.1>" solutionType="Work" 
     url="https://github.com/DirectumCompany/rx-template-entityrelationshipsearch" />
</block>
```
### Установка для использования на проекте
Возможные варианты:

#### A. Fork репозитория.
1. Сделать fork репозитория rx-template-entityrelationshipsearch для своей учетной записи.
2. Склонировать созданный в п. 1 репозиторий в папку.
3. Указать в _ConfigSettings.xml DDS:
``` xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.2>" solutionType="Work" 
     url="<Адрес репозитория gitHub учетной записи пользователя из п. 1>" />
</block>
```
#### B. Подключение на базовый слой.
Вариант не рекомендуется, так как при выходе версии шаблона разработки не гарантируется обратная совместимость.
1. Склонировать репозиторий rx-template-entityrelationshipsearch в папку.
2. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.1>" solutionType="Base" 
     url="https://github.com/DirectumCompany/rx-template-entityrelationshipsearch" />
  <repository folderName="<Папка для рабочего слоя>" solutionType="Work" 
     url="<Адрес репозитория для рабочего слоя>" />
</block>
```
#### C. Копирование репозитория в систему контроля версий.
Рекомендуемый вариант для проектов внедрения.
1. В системе контроля версий с поддержкой git создать новый репозиторий.
2. Склонировать репозиторий rx-template-entityrelationshipsearch в папку с ключом `--mirror`.
3. Перейти в папку из п. 2.
4. Импортировать клонированный репозиторий в систему контроля версий командой:
`git push –mirror <Адрес репозитория из п. 1>`

> [!NOTE]
> Замечания и пожеланию по развитию шаблона разработки фиксируйте через [Issues](https://github.com/MTGroupDev/rx-template-entityrelationshipsearch/issues).  
> При оформлении ошибки, опишите сценарий для воспроизведения. Для пожеланий приведите обоснование для описываемых изменений - частоту использования, бизнес-ценность, риски и/или эффект от реализации.  
> Внимание! Изменения будут вноситься только в новые версии. 

