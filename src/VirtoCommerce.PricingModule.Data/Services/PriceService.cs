using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    public class PriceService : CrudService<Price, PriceEntity, PriceChangingEvent, PriceChangedEvent>, IPriceService
    {
        private readonly ILogger<PricingServiceImpl> _logger;
        private readonly IPricingPriorityFilterPolicy _pricingPriorityFilterPolicy;

        public PriceService(Func<IPriceRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IEventPublisher eventPublisher,
            ILogger<PricingServiceImpl> logger, IPricingPriorityFilterPolicy pricingPriorityFilterPolicy)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _logger = logger;
            _pricingPriorityFilterPolicy = pricingPriorityFilterPolicy;
        }

        protected override Task<IEnumerable<PriceEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((IPriceRepository)repository).GetByIdsAsync(ids);
        }
    }
}
