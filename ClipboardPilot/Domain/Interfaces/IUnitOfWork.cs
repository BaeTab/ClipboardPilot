using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ClipboardPilot.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
