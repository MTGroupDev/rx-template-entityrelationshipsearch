using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Domain.Initialization;

namespace mtg.EntityRelationshipSearch.Server
{
  public partial class ModuleInitializer
  {

    public override void Initializing(Sungero.Domain.ModuleInitializingEventArgs e)
    {
      GrandAcessRightsOnReports();
      CreateReportsTables();
    }
    
    /// <summary>
    /// Выдать права на отчеты.
    /// </summary>
    public void GrandAcessRightsOnReports()
    {
      InitializationLogger.DebugFormat("GrandAcessRightsOnReports.");
      
      var allUsers = Roles.AllUsers;
      
      if (allUsers != null)
        Reports.AccessRights.Grant(Reports.GetEntityRelashionshipSearchReport().Info, allUsers, DefaultReportAccessRightsTypes.Execute);
    }
    
    /// <summary>
    /// Создать таблицы для отчетов.
    /// </summary>
    public static void CreateReportsTables()
    {
      var entityRelashionSearchTableName = Constants.EntityRelashionshipSearchReport.SourceTableName;
      
      Sungero.Docflow.PublicFunctions.Module.DropReportTempTables(new[] {
                                                                    entityRelashionSearchTableName
                                                                  });
      
      Sungero.Docflow.PublicFunctions.Module.ExecuteSQLCommandFormat(Queries.EntityRelashionshipSearchReport.CreateTableData, new[] { entityRelashionSearchTableName });
    }
  }
}
