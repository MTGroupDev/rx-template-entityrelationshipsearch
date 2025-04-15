using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Metadata;
using Sungero.Domain.Shared;

namespace mtg.EntityRelationshipSearch
{
  partial class EntityRelashionshipSearchReportServerHandlers
  {

    public override void AfterExecute(Sungero.Reporting.Server.AfterExecuteEventArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.DeleteReportData(Constants.EntityRelashionshipSearchReport.SourceTableName, EntityRelashionshipSearchReport.ReportSessionId);
    }

    public override void BeforeExecute(Sungero.Reporting.Server.BeforeExecuteEventArgs e)
    {
      var reportSessionId = Guid.NewGuid().ToString();
      var report = EntityRelashionshipSearchReport;
      var dbTableName = report.DbTableName;
      var entityGuid = report.EntityGuid;
      var hyperlinkTemplate = report.HyperlinkTemplate;
      var entityId = report.EntityId;
      var tableRows = new List<mtg.EntityRelationshipSearch.Structures.Module.TableRow>();
      var relationshipsInfo = new List<Structures.Module.EntityInfo>();
      
      report.ReportSessionId = reportSessionId;
      
      // Первый запрос в БД, для получения всех связей сущности.
      GetAllRelationsFromDB(relationshipsInfo, dbTableName, entityId.Value);
      
      // Второй запрос в БД, для определения родителей свойств-коллекций.
      FillParentCollectionProperties(relationshipsInfo, entityGuid, entityId.Value);
      
      report.HasRelationships = relationshipsInfo.Any();
      
      foreach (var info in relationshipsInfo)
      {
        var tableRow = mtg.EntityRelationshipSearch.Structures.Module.TableRow.Create();
        
        var hyperlink = info.IsCollection
          ? string.Format(hyperlinkTemplate, info.ParentEntityGuid, info.ParentEntityId)
          : string.Format(hyperlinkTemplate, info.EntityGuid, info.EntityId);
        
        tableRow.ReportSessionId = reportSessionId;
        tableRow.EntityId = info.EntityId;
        tableRow.EntityType = info.EntityType;
        tableRow.Name = info.EntityName;
        tableRow.EntityHyperlink = hyperlink;
        
        tableRows.Add(tableRow);
      }
      
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.EntityRelashionshipSearchReport.SourceTableName, tableRows);
    }
    
    /// <summary>
    /// Получить все связи сущности из базы данных.
    /// </summary>
    /// <param name="relationshipsInfo">Структура связей.</param>
    /// <param name="dbTableName">Наименование таблицы в БД основной сущности.</param>
    /// <param name="entityId">ИД сущности.</param>
    /// <returns>Структура со связями.</returns>
    public virtual List<Structures.Module.EntityInfo> GetAllRelationsFromDB(List<Structures.Module.EntityInfo> relationshipsInfo, string dbTableName, long entityId)
    {
      using (var connection = SQL.GetCurrentConnection())
      {
        using (var command = connection.CreateCommand())
        {
          command.CommandText = string.Format(Queries.EntityRelashionshipSearchReport.GetRelationship, dbTableName, entityId);
          
          using (var reader = command.ExecuteReader())
          {
            while (reader.Read())
            {
              var entityInfo = Structures.Module.EntityInfo.Create();
              
              entityInfo.TableName = GetSafeString(reader, 0);
              // Здесь может быть NULL.
              entityInfo.EntityId = GetSafeLong(reader, 1);
              // Здесь может быть NULL.
              entityInfo.EntityGuid = GetSafeString(reader, 2);
              // Здесь может быть NULL.
              entityInfo.EntityName = GetSafeString(reader, 3);
              entityInfo.IsCollection = IsCollectionEntity(entityInfo.EntityGuid);
              entityInfo.EntityType = GetEntityDisplayName(entityInfo.EntityGuid, entityInfo.IsCollection);
              
              // Если не нашли GUID записи, то не обрабатываем. Например, таблица связей (sungero_content_relation) не влияет на удаление объектов.
              if (!string.IsNullOrWhiteSpace(entityInfo.EntityGuid))
                relationshipsInfo.Add(entityInfo);
              
            }
          }
        }
      }
      
      return relationshipsInfo;
    }
    
    /// <summary>
    /// Заполнить родителей у свойств-коллекций.
    /// </summary>
    /// <param name="relationshipsInfo">Структура связей.</param>
    /// <param name="entityGuid">GUID основного типа сущности.</param>
    /// <param name="entityId">ИД основной сущности.</param>
    /// <returns>Структура со связями.</returns>
    public virtual List<Structures.Module.EntityInfo> FillParentCollectionProperties(List<Structures.Module.EntityInfo> relationshipsInfo, string entityGuid, long entityId)
    {
      var collections = relationshipsInfo.Where(x => x.IsCollection);
      
      if (!collections.Any())
        return relationshipsInfo;
      
      foreach (var collection in collections)
      {
        var entityType = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(Guid.Parse(collection.EntityGuid));
        var collectionMetadata = Sungero.Domain.Shared.TypeExtension.GetEntityMetadata(entityType);
        // Метаданные родителя коллекции.
        var parentMetadata = collectionMetadata.MainEntityMetadata;
        
        if (parentMetadata == null)
          continue;
        
        // Наименование таблицы коллекции в БД. Например: sungero_docflow_outaddressees
        var collectionDbTableName = collectionMetadata.DBTableName;
        // Наименование таблицы родителя коллекции в БД. Например: sungero_content_edoc
        var parentDbTableName = parentMetadata.DBTableName;
        // Наименование столбца родителя в БД. Например: edoc
        var parentCode = parentMetadata.Code;
        
        // Получить ссылочное свойство из свойства-коллекции, по идентификатору основной сущности (откуда нажимаем кнопку).
        var navigationProperty = collectionMetadata.Properties
          .Where(p => p.PropertyType == PropertyType.Navigation)
          .OfType<Sungero.Metadata.NavigationPropertyMetadata>()
          .FirstOrDefault(x => x.EntityGuid == Guid.Parse(entityGuid));
        
        if (navigationProperty == null)
          continue;
        
        // Наименование столбца свойства-коллекции в БД. Например: correspondent
        var codeCollect = navigationProperty.Code;
        var collectionQuery = string.Format(Queries.EntityRelashionshipSearchReport.GetCollectionParent, collectionDbTableName, parentDbTableName, parentCode, entityId, codeCollect);
        
        using (var connection = SQL.GetCurrentConnection())
        {
          using (var command = connection.CreateCommand())
          {
            command.CommandText = collectionQuery;
            
            using (var readerCollection = command.ExecuteReader())
            {
              while (readerCollection.Read())
              {
                collection.ParentEntityId = GetSafeLong(readerCollection, 0);
                collection.ParentEntityGuid = GetSafeGuid(readerCollection, 1).ToString();
                collection.EntityName = parentMetadata.GetDisplayName() + mtg.EntityRelationshipSearch.Resources.CollectionPart;
              }
            }
          }
        }
      }
      
      return relationshipsInfo;
    }
    
    /// <summary>
    /// Определить, является ли сущность коллекцией (по GUID'у).
    /// </summary>
    /// <param name="entityGuid">GUID сущности.</param>
    /// <returns>True - если сущность является коллекцией, иначе False.</returns>
    public static bool IsCollectionEntity(string entityGuid)
    {
      if (string.IsNullOrWhiteSpace(entityGuid))
        return false;
      
      var entityType = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(Guid.Parse(entityGuid));
      
      // TODO: Протестировать typeof() при адаптации на новые версии.
      return typeof(Sungero.Domain.Shared.IChildEntity).IsAssignableFrom(entityType);
    }
    
    /// <summary>
    /// Безопасно получить строку из DataReader.
    /// </summary>
    /// <param name="reader">DataReader.</param>
    /// <param name="columnNumber">Номер столбца.</param>
    /// <returns>Строка из DataReader. Если не удалось получить строку, возвращается пустая строка.</returns>
    public static string GetSafeString(System.Data.IDataReader reader, int columnNumber)
    {
      return reader.IsDBNull(columnNumber) ? string.Empty : reader.GetString(columnNumber);
    }
    
    /// <summary>
    /// Безопасно получить число (long) из DataReader.
    /// </summary>
    /// <param name="reader">DataReader.</param>
    /// <param name="columnNumber">Номер столбца.</param>
    /// <returns>Число из DataReader. Если не удалось получить число, возвращается 0.</returns>
    public static long GetSafeLong(System.Data.IDataReader reader, int columnNumber)
    {
      return reader.IsDBNull(columnNumber) ? 0 : reader.GetInt64(columnNumber);
    }
    
    /// <summary>
    /// Безопасно получить Guid из DataReader.
    /// </summary>
    /// <param name="reader">DataReader.</param>
    /// <param name="columnNumber">Номер столбца.</param>
    /// <returns>Guid из DataReader. Если не удалось получить число, возвращается Guid.Empty.</returns>
    public static Guid GetSafeGuid(System.Data.IDataReader reader, int columnNumber)
    {
      return reader.IsDBNull(columnNumber) ? Guid.Empty : reader.GetGuid(columnNumber);
    }
    
    /// <summary>
    /// Получить наименование типа сущности.
    /// </summary>
    /// <param name="entityGuid">GUID сущности.</param>
    /// <param name="isCollectionEntity">Признак, является ли сущность коллекцией.</param>
    /// <returns>Наименование типа сущности.</returns>
    public static string GetEntityDisplayName(string entityGuid, bool isCollectionEntity)
    {
      if (string.IsNullOrWhiteSpace(entityGuid))
        return string.Empty;
      
      var entityType = Sungero.Domain.Shared.TypeExtension.GetTypeByGuid(Guid.Parse(entityGuid));
      var metadata = Sungero.Domain.Shared.TypeExtension.GetEntityMetadata(entityType);
      
      if (isCollectionEntity)
      {
        var mainMetadata = metadata.MainEntityMetadata;
        
        if (mainMetadata != null)
          return mainMetadata.GetDisplayName();
      }
      
      return metadata.GetDisplayName();
    }
  }
}