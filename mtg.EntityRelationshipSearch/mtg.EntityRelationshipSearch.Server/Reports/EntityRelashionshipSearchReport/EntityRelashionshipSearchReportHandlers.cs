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
      var hyperlinkTemplate = report.HyperlinkTemplate;
      var entityId = report.EntityId;
      var tableRows = new List<mtg.EntityRelationshipSearch.Structures.Module.TableRow>();
      var relationshipsInfo = new List<Structures.Module.EntityInfo>();
      
      report.ReportSessionId = reportSessionId;
      
      // Первый запрос в БД, для получения всех связей сущности.
      GetAllRelationsFromDB(relationshipsInfo, report.DbTableName, entityId.Value);
      
      // Второй запрос в БД, для определения родителей свойств-коллекций.
      FillParentCollectionProperties(relationshipsInfo, report.EntityGuid, entityId.Value);
      
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
              
              entityInfo.TableName = reader.GetString(0);
              entityInfo.EntityId = reader.GetInt64(1);
              entityInfo.EntityGuid = reader.GetString(2);
              entityInfo.EntityName = reader.GetString(3);
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
                collection.ParentEntityId = readerCollection.GetInt64(0);
                collection.ParentEntityGuid = readerCollection.GetGuid(1).ToString();
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