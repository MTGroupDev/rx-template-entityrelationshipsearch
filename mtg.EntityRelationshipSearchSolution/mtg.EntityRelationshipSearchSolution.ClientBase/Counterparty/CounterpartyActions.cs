using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using mtg.EntityRelationshipSearchSolution.Counterparty;

namespace mtg.EntityRelationshipSearchSolution.Client
{
  partial class CounterpartyActions
  {
    public virtual void OpenEntityRelashionshipSearchReport(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      mtg.EntityRelationshipSearch.PublicFunctions.Module.OpenEntityRelashionshipSearchReport(_obj);
    }

    public virtual bool CanOpenEntityRelashionshipSearchReport(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return !_obj.State.IsInserted;
    }

  }

}