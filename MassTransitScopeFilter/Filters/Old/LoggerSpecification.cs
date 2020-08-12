using System.Collections.Generic;
using System.Linq;
using GreenPipes;
using MassTransit;

namespace MassTransitScopeFilter.Filters.Old
{
    public class LoggerSpecification<T> : IPipeSpecification<T>
        where T : class, ConsumeContext
    {
        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new LoggerFilter<T>());
        }
    }
}