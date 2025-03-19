using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace mtg.EntityRelationshipSearch.Client
{
  public class ModuleFunctions
  {

    /// <summary>
    /// Сформировать отчет "Поиск связей сущностей".
    /// </summary>
    /// <param name="entity">Сущность.</param>
    [Public]
    public virtual void OpenEntityRelashionshipSearchReport(Sungero.Domain.Shared.IEntity entity)
    {
      if (entity == null)
      {
        Dialogs.ShowMessage(mtg.EntityRelationshipSearch.Resources.EntityNotFound, MessageType.Error);
        return;
      }
      
      try
      {
        string relations = mtg.EntityRelationshipSearch.PublicFunctions.Module.GetEntityRelationship(entity);
        
        if (string.IsNullOrWhiteSpace(relations))
        {
          Dialogs.ShowMessage(mtg.EntityRelationshipSearch.Resources.NotFoundRelationships, MessageType.Information);
          return;
        }
        
        var report = mtg.EntityRelationshipSearch.Reports.GetEntityRelashionshipSearchReport();
        var sourceEntityTypeName = mtg.EntityRelationshipSearch.PublicFunctions.Module.GetSourceEntityTypeName(entity);
        
        report.EntityId = entity.Id;
        report.SourceEntityName = string.Format("{0} \"{1}\"", sourceEntityTypeName, entity.DisplayValue);
        report.Relashions = relations;
        
        report.Open();
      }
      catch (Exception ex)
      {
        Logger.ErrorFormat("OpenEntityRelashionshipSearchReport. Error when generating the report: {0}", ex.ToString());
        Dialogs.ShowMessage(mtg.EntityRelationshipSearch.Resources.ReportCouldNotGenerated, MessageType.Error);
      }
    }
  }
}