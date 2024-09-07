using System;
using System.Collections.Generic;
using System.Text;

namespace MicroFlows.Domain.Interfaces;
public interface IFlowParams
{
    string this[string index] { get; set; }
    string AssignedUser { get; set; }
    string AssignedTeam { get; set; }
}
