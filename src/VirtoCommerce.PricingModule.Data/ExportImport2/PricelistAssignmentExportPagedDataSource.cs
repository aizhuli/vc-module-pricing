using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.ExportModule.Core.Model;
using VirtoCommerce.ExportModule.Data.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public class PricelistAssignmentExportPagedDataSource : ExportPagedDataSource<PricelistAssignmentExportDataQuery, PricelistAssignmentsSearchCriteria>
    {
        private readonly ICatalogService _catalogService;
        private readonly PricelistAssignmentExportDataQuery _dataQuery;

        private readonly ICrudService<PricelistAssignment> _pricelistAssignmentService;
        private readonly ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment> _pricelistAssignmentSearchService;
        private readonly ICrudService<Pricelist> _pricelistService;

        public PricelistAssignmentExportPagedDataSource(IPricelistAssignmentService pricelistAssignmentService
            , IPricelistAssignmentSearchService pricelistAssignmentSearchService
            , IPricelistService pricelistService
            , ICatalogService catalogService
            , PricelistAssignmentExportDataQuery dataQuery) : base(dataQuery)
        {
            _pricelistAssignmentService = (ICrudService<PricelistAssignment>)pricelistAssignmentService;
            _pricelistAssignmentSearchService = (ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment>)pricelistAssignmentSearchService;
            _pricelistService = (ICrudService<Pricelist>)pricelistService;
            _catalogService = catalogService;
            _dataQuery = dataQuery;
        }

        protected override PricelistAssignmentsSearchCriteria BuildSearchCriteria(PricelistAssignmentExportDataQuery exportDataQuery)
        {
            var result = base.BuildSearchCriteria(exportDataQuery);

            result.PriceListIds = _dataQuery.PriceListIds;
            result.CatalogIds = _dataQuery.CatalogIds;
            result.Keyword = _dataQuery.Keyword;

            return result;
        }

        protected override ExportableSearchResult FetchData(PricelistAssignmentsSearchCriteria searchCriteria)
        {
            PricelistAssignment[] result;
            int totalCount;

            if (searchCriteria.ObjectIds.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                result = _pricelistAssignmentService.GetByIdsAsync(searchCriteria.ObjectIds).GetAwaiter().GetResult().ToArray();
                totalCount = result.Length;
            }
            else
            {
                var pricelistAssignmentSearchResult = _pricelistAssignmentSearchService.SearchAsync(searchCriteria).GetAwaiter().GetResult();
                result = pricelistAssignmentSearchResult.Results.ToArray();
                totalCount = pricelistAssignmentSearchResult.TotalCount;
            }

            return new ExportableSearchResult()
            {
                Results = ToExportable(result).ToList(),
                TotalCount = totalCount,
            };
        }

        protected virtual IEnumerable<IExportable> ToExportable(IEnumerable<ICloneable> objects)
        {
            var models = objects.Cast<PricelistAssignment>();
            var viewableMap = models.ToDictionary(x => x, x => AbstractTypeFactory<ExportablePricelistAssignment>.TryCreateInstance().FromModel(x));

            FillAdditionalProperties(viewableMap);

            var modelIds = models.Select(x => x.Id).ToList();
            var result = viewableMap.Values.OrderBy(x => modelIds.IndexOf(x.Id));

            return result;
        }

        protected virtual void FillAdditionalProperties(Dictionary<PricelistAssignment, ExportablePricelistAssignment> viewableMap)
        {
            var models = viewableMap.Keys;
            var catalogIds = models.Select(x => x.CatalogId).Distinct().ToArray();
            var pricelistIds = models.Select(x => x.PricelistId).Distinct().ToArray();
            var catalogs = _catalogService.GetByIdsAsync(catalogIds, CatalogResponseGroup.Info.ToString()).GetAwaiter().GetResult();
            var pricelists = _pricelistService.GetByIdsAsync(pricelistIds).GetAwaiter().GetResult();

            foreach (var kvp in viewableMap)
            {
                var model = kvp.Key;
                var viewableEntity = kvp.Value;
                var catalog = catalogs.FirstOrDefault(x => x.Id == model.CatalogId);
                var pricelist = pricelists.FirstOrDefault(x => x.Id == model.PricelistId);

                viewableEntity.CatalogName = catalog?.Name;
                viewableEntity.PricelistName = pricelist?.Name;
            }
        }
    }
}
