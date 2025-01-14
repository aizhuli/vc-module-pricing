using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.ExportImport;
using VirtoCommerce.PricingModule.Core;
using VirtoCommerce.PricingModule.Core.Model;
using VirtoCommerce.PricingModule.Core.Model.Search;
using VirtoCommerce.PricingModule.Core.Services;

namespace VirtoCommerce.PricingModule.Data.ExportImport
{
    public sealed class PricingExportImport
    {
        private readonly ISettingsManager _settingsManager;
        private readonly JsonSerializer _jsonSerializer;

        private readonly ICrudService<Price> _priceService;
        private readonly ICrudService<Pricelist> _pricelistService;
        private readonly ICrudService<PricelistAssignment> _pricelistAssignmentService;
        private readonly ISearchService<PricesSearchCriteria, PriceSearchResult, Price> _priceSearchService;
        private readonly ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist> _pricelistSearchService;
        private readonly ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment> _pricelistAssignmentSearchService;

        private int? _batchSize;

        public PricingExportImport(IPriceService priceService
            , IPriceSearchService priceSearchService
            , IPricelistService pricelistService
            , IPricelistSearchService pricelistSearchService
            , IPricelistAssignmentService pricelistAssignmentService
            , IPricelistAssignmentSearchService pricelistAssignmentSearchService
            , ISettingsManager settingsManager
            , JsonSerializer jsonSerializer)
        {
            _settingsManager = settingsManager;

            _jsonSerializer = jsonSerializer;

            _priceSearchService = (ISearchService<PricesSearchCriteria, PriceSearchResult, Price>)priceSearchService;
            _pricelistSearchService = (ISearchService<PricelistSearchCriteria, PricelistSearchResult, Pricelist>)pricelistSearchService;
            _pricelistAssignmentSearchService = (ISearchService<PricelistAssignmentsSearchCriteria, PricelistAssignmentSearchResult, PricelistAssignment>)pricelistAssignmentSearchService;
            _priceService = (ICrudService<Price>)priceService;
            _pricelistService = (ICrudService<Pricelist>)pricelistService;
            _pricelistAssignmentService = (ICrudService<PricelistAssignment>)pricelistAssignmentService;
        }

        private int BatchSize
        {
            get
            {
                if (_batchSize == null)
                {
                    _batchSize = _settingsManager.GetValue(ModuleConstants.Settings.General.ExportImportPageSize.Name, 50);
                }

                return (int)_batchSize;
            }
        }

        public async Task DoExportAsync(Stream backupStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo { Description = "loading data..." };
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(backupStream, Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();

                progressInfo.Description = "Price lists exporting...";
                progressCallback(progressInfo);

                #region Export price lists

                await writer.WritePropertyNameAsync("Pricelists");

                await writer.SerializeJsonArrayWithPagingAsync(_jsonSerializer, BatchSize, async (skip, take) =>
                    (GenericSearchResult<Pricelist>)await _pricelistSearchService.SearchAsync(new PricelistSearchCriteria { Skip = skip, Take = take })
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{ processedCount } of { totalCount } pricelits have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion

                #region Export price list assignments

                await writer.WritePropertyNameAsync("Assignments");

                await writer.SerializeJsonArrayWithPagingAsync(_jsonSerializer, BatchSize, async (skip, take) =>
                    (GenericSearchResult<PricelistAssignment>)await _pricelistAssignmentSearchService.SearchAsync(new PricelistAssignmentsSearchCriteria { Skip = skip, Take = take })
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{ processedCount } of { totalCount } pricelits assignments have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion

                #region Export prices

                await writer.WritePropertyNameAsync("Prices");

                await writer.SerializeJsonArrayWithPagingAsync(_jsonSerializer, BatchSize, async (skip, take) =>
                    (GenericSearchResult<Price>)await _priceSearchService.SearchAsync(new PricesSearchCriteria { Skip = skip, Take = take })
                , (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{ processedCount } of { totalCount } prices have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                #endregion

                await writer.WriteEndObjectAsync();
                await writer.FlushAsync();
            }
        }

        public async Task DoImportAsync(Stream stream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo();

            using (var streamReader = new StreamReader(stream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var readerValue = reader.Value.ToString();

                        if (readerValue == "Pricelists")
                        {
                            await reader.DeserializeJsonArrayWithPagingAsync<Pricelist>(_jsonSerializer, BatchSize, items => _pricelistService.SaveChangesAsync(items.ToArray()), processedCount =>
                            {
                                progressInfo.Description = $"{ processedCount } price lists have been imported";
                                progressCallback(progressInfo);
                            }, cancellationToken);
                        }
                        else if (readerValue == "Prices")
                        {
                            await reader.DeserializeJsonArrayWithPagingAsync<Price>(_jsonSerializer, BatchSize, items => _priceService.SaveChangesAsync(items.ToArray()), processedCount =>
                            {
                                progressInfo.Description = $"Prices: {progressInfo.ProcessedCount} have been imported";
                                progressCallback(progressInfo);
                            }, cancellationToken);
                        }
                        else if (readerValue == "Assignments")
                        {
                            await reader.DeserializeJsonArrayWithPagingAsync<PricelistAssignment>(_jsonSerializer, BatchSize, items => _pricelistAssignmentService.SaveChangesAsync(items.ToArray()), processedCount =>
                            {
                                progressInfo.Description = $"{progressInfo.ProcessedCount} assignments have been imported";
                                progressCallback(progressInfo);
                            }, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
