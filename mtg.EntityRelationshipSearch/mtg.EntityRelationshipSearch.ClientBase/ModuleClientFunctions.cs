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
        var report = mtg.EntityRelationshipSearch.Reports.GetEntityRelashionshipSearchReport();
        
        report.EntityId = entity.Id;
        report.DbTableName = mtg.EntityRelationshipSearch.PublicFunctions.Module.GetDbTableName(entity);
        report.EntityGuid = mtg.EntityRelationshipSearch.PublicFunctions.Module.GetEntityGuid(entity);
        report.HyperlinkTemplate = mtg.EntityRelationshipSearch.PublicFunctions.Module.GetTemplateUrl(entity);
        report.SourceEntityName = string.Format("{0} \"{1}\"", mtg.EntityRelationshipSearch.PublicFunctions.Module.GetSourceEntityTypeName(entity), entity.DisplayValue);
        
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