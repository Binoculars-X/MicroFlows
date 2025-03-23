using MicroFlows.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;

public record SearchFlowDetails(string RefId, string? ExternalId, string? Tag, FlowStateEnum? State, 
    ResultStateEnum? Result
    );
