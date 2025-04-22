using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows;

public class MsSqlFlowRepositorySettings
{
    public IdGenerationType IdGenerationType { get; set; }
    public string? TableName { get; set; } = null;
    public string? ConnectionString { get; set; } = null;
    public string? ConnectionStringKey { get; set; } = null;
    public string? DatabaseName { get; set; } = null;
    public bool? CreateDatabase { get; set; } = null;
}

public enum IdGenerationType
{
    Guid,
    Identity
}
