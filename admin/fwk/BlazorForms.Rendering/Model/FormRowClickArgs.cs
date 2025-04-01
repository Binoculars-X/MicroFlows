using BlazorForms.Flows.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorForms.Rendering.State
{
    public class FormRowClickArgs
    {
        public IFlowModel Model { get; set; }
        public string Pk { get; set; }
    }
}
