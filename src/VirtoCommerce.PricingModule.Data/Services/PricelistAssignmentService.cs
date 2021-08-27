using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PricingModule.Core.Events;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Services;
using VirtoCommerce.PricingModule.Data.Model;
using VirtoCommerce.PricingModule.Data.Repositories;

namespace VirtoCommerce.PricingModule.Data.Services
{
    public class PricelistAssignmentService : CrudService<PricelistAssignment, PricelistAssignmentEntity, PricelistAssignmentChangingEvent, PricelistAssignmentChangedEvent>
                                            , IPricelistAssignmentService
    {
        public PricelistAssignmentService(Func<IPricelistAssignmentRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        protected override Task<IEnumerable<PricelistAssignmentEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((IPricelistAssignmentRepository)repository).GetByIdsAsync(ids);
        }
    }
}
