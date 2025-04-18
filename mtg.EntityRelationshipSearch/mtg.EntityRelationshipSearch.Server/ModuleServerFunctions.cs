using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Metadata;
using Sungero.Domain.Shared;
using Sungero.Docflow;

namespace mtg.EntityRelationshipSearch.Server
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Получить имя таблицы в БД по сущности.
    /// </summary>
    /// <param name="entity">Сущность</param>
    /// <returns>Имя таблицы в БД.</returns>
    [Public]
    public virtual string GetDbTableName(Sungero.Domain.Shared.IEntity entity)
    {
      return entity?.GetEntityMetadata().DBTableName ?? string.Empty;
    }
    
    /// <summary>
    /// Получить GUID сущности.
    /// </summary>
    /// <param name="entity">Сущность.</param>
    /// <returns>GUID сущности (строка).</returns>
    [Public]
    public virtual string GetEntityGuid(Sungero.Domain.Shared.IEntity entity)
    {
      // Для документов необходимо получать GUID именно текущего типа документа.
      if (Sungero.Docflow.OfficialDocuments.Is(entity))
        return entity?.GetEntityMetadata().NameGuid.ToString();
      
      return entity?.GetEntityMetadata().BaseRootEntityMetadataGuid.ToString() ?? string.Empty;
    }
    
    /// <summary>
    /// Получить шаблон ссылки на сущность, чтобы впоследствии подставить свой GUID и ИД сущности.
    /// </summary>
    /// <param name="entity">Сущность.</param>
    /// <returns>Шаблонная строка гиперссылки.</returns>
    [Public]
    public virtual string GetTemplateUrl(Sungero.Domain.Shared.IEntity entity)
    {
      var url = Hyperlinks.Get(entity);
      var typeGuid = entity.GetEntityMetadata().NameGuid;
      
      return url.Replace(typeGuid.ToString(), "{0}").Replace(entity.Id.ToString(), "{1}");
    }
    
    /// <summary>
    /// Получить тип сущности для построения заголовка отчета.
    /// </summary>
    /// <param name="entity">Сущность.</param>
    /// <returns>Строка с наименованием сущности (документа, задачи, задания, записи справочника).</returns>
    [Public]
    public static string GetSourceEntityTypeName(Sungero.Domain.Shared.IEntity entity)
    {
      if (Sungero.Docflow.OfficialDocuments.Is(entity))
        return mtg.EntityRelationshipSearch.Resources.DocumentEntityName;
      
      if (Sungero.Workflow.Tasks.Is(entity))
        return mtg.EntityRelationshipSearch.Resources.TaskEntityName;
      
      if (Sungero.Workflow.Assignments.Is(entity))
        return mtg.EntityRelationshipSearch.Resources.AssignmentEntityName;
      
      return mtg.EntityRelationshipSearch.Resources.DatabookRecordEntityName;
    }
  }
}