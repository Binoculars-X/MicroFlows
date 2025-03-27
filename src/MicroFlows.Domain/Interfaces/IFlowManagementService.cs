using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.Domain.Interfaces;

public interface IFlowManagementService
{
    Task RerunFailedFlows();
}
