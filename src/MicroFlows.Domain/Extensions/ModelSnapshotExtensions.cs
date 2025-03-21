using JsonPathToModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows;

public static class ModelSnapshotExtensions
{
    public static T? Deserialize<T>(this ModelSnapshot modelSnapshot) where T: class
    {
        return modelSnapshot.Records["$.Model"].Deserialize() as T;
    }
}
