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
      var tableRows = new List<mtg.EntityRelationshipSearch.Structures.Module.TableRow>();
      
      report.ReportSessionId = reportSessionId;
      
      var relationships = report.Relashions.Split(';');
      
      foreach (var relationship in relationships)
      {
        var parts = relationship.Split(new[] { Constants.Module.SeparateCharacters }, StringSplitOptions.None);
        
        if (parts.Length < 4)
          continue;
        
        var tableRow = mtg.EntityRelationshipSearch.Structures.Module.TableRow.Create();
        
        tableRow.ReportSessionId = reportSessionId;
        tableRow.EntityId = long.Parse(parts[0]);
        tableRow.EntityType = parts[1];
        tableRow.Name = parts[2];
        tableRow.EntityHyperlink = parts[3];
        
        tableRows.Add(tableRow);
      }
      
      Sungero.Docflow.PublicFunctions.Module.WriteStructuresToTable(Constants.EntityRelashionshipSearchReport.SourceTableName, tableRows);
    }
  }
}