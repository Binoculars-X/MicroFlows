using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Models;

public class ModelSnapshot
{
    public Dictionary<string, string> Values { get; set; } = [];
}
