using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroFlows.MsSqlRepo;

internal class OptimisticLockMsSqlFlowRepositoryException : Exception
{
    public OptimisticLockMsSqlFlowRepositoryException() : base()
    {
    }

    public OptimisticLockMsSqlFlowRepositoryException(string message) : base(message)
    {
    }
}
