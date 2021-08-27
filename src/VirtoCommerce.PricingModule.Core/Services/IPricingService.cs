using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.PricingModule.Core.Model;

namespace VirtoCommerce.PricingModule.Core.Services
{
    public interface IPricingService
    {
        [Obsolete(@"Need to remove after inheriting IPriceService, IpricelistService, and IPricelistAssignment from ICrudService.")]
        Task<Price[]> GetPricesByIdAsync(string[] ids);
        Task<Pricelist[]> GetPricelistsByIdAsync(string[] ids);
        Task<PricelistAssignment[]> GetPricelistAssignmentsByIdAsync(string[] ids);

        Task SavePricesAsync(Price[] prices);
        Task SavePricelistsAsync(Pricelist[] priceLists);
        Task SavePricelistAssignmentsAsync(PricelistAssignment[] assignments);    

        Task DeletePricelistsAsync(string[] ids);
        Task DeletePricesAsync(string[] ids);
        Task DeletePricelistsAssignmentsAsync(string[] ids);

        Task<IEnumerable<Pricelist>> EvaluatePriceListsAsync(PriceEvaluationContext evalContext);
        Task<IEnumerable<Price>> EvaluateProductPricesAsync(PriceEvaluationContext evalContext);
    }
}
