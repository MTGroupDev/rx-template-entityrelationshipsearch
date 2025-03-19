using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace mtg.EntityRelationshipSearch.Structures.Module
{

  /// <summary>
  /// Структура отчета.
  /// </summary>
  partial class TableRow
  {
    public string ReportSessionId { get; set; }
    
    public long EntityId { get; set; }
    
    public string EntityType { get; set; }
    
    public string Name { get; set; }
    
    public string EntityHyperlink { get; set; }
  }
  
  /// <summary>
  /// Структура с информацией о связях объектов.
  /// </summary>
  partial class EntityInfo
  {
    public string TableName { get; set; }
    
    public long EntityId { get; set; }
    
    public string EntityGuid { get; set; }
    
    public string EntityType { get; set; }
    
    public string EntityName { get; set; }
    
    public bool IsCollection { get; set; }
    
    public long ParentEntityId { get; set; }
    
    public string ParentEntityGuid { get; set; }
  }

}