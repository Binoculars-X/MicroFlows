using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Models;

public class ModelWrapper<TModel> where TModel : class, new()
{
    public TModel Model { get; set; } = new();
}
