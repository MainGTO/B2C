using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using RestSharp;
using HtmlAgilityPack;
using static Nop.Web.Areas.Admin.Controllers.ProductController;
using System.Net.Http.Headers;
using DeepL;
using DeepL.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Diagnostics;
using FluentMigrator.Runner;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Category = Nop.Core.Domain.Catalog.Category;
using DocumentFormat.OpenXml.EMMA;
using LinqToDB.Linq.Builder;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ProductController : BaseAdminController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly ICategoryService _categoryService;
        private readonly ICopyProductService _copyProductService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IExportManager _exportManager;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IImportManager _importManager;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INopFileProvider _fileProvider;
        private readonly INotificationService _notificationService;
        private readonly IPdfService _pdfService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVideoService _videoService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public ProductController(IAclService aclService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICategoryService categoryService,
            ICopyProductService copyProductService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IExportManager exportManager,
            IGenericAttributeService genericAttributeService,
            IHttpClientFactory httpClientFactory,
            IImportManager importManager,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IManufacturerService manufacturerService,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            ISettingService settingService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IVideoService videoService,
            IWebHelper webHelper,
            IWorkContext workContext,
            VendorSettings vendorSettings)
        {
            _aclService = aclService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _categoryService = categoryService;
            _copyProductService = copyProductService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _downloadService = downloadService;
            _exportManager = exportManager;
            _genericAttributeService = genericAttributeService;
            _httpClientFactory = httpClientFactory;
            _importManager = importManager;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _manufacturerService = manufacturerService;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
            _pdfService = pdfService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _settingService = settingService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _specificationAttributeService = specificationAttributeService;
            _storeContext = storeContext;
            _urlRecordService = urlRecordService;
            _videoService = videoService;
            _webHelper = webHelper;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
        }

        #endregion

        #region Utilities


        #region  CategoryEditMapping

        [HttpPost]
        public async Task<IActionResult> UpdateProductCategories(int[] productIds, int[] categoryIds)
        {
            if (productIds == null || categoryIds == null)
                return Json(new { Success = false, Message = "상품 또는 카테고리 정보가 제공되지 않았습니다." });

            foreach (var productId in productIds)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                    continue;

                var model = new ProductModel
                {
                    Id = productId,
                    SelectedCategoryIds = categoryIds
                };
                await SaveCategoryMappingsAsync(product, model);
            }

            /*#region Remove Category

            // 전체 카테고리 가져오기
            var categories = await _categoryService.GetAllCategoriesAsync();

            // "미분류 카테고리" 이름을 가진 카테고리를 찾기
            var unclassifiedCategoryName = "미분류 카테고리";
            var unclassifiedCategory = categories.FirstOrDefault(cat => cat.Name == unclassifiedCategoryName);

            // "미분류 카테고리"의 ID를 가져옴
            int unclassifiedCategoryId = unclassifiedCategory.Id;

            // 하위 카테고리를 가져오기
            var subCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(unclassifiedCategoryId);

            foreach (var subCategory in subCategories)
            {
                // 각 하위 카테고리에 속한 상품 가져오기
                var productsInSubCategory = await _categoryService.GetProductCategoriesByCategoryIdAsync(subCategory.Id);

                if (!productsInSubCategory.Any())
                {
                    // 하위 카테고리에 속한 상품이 없는 경우, 해당 카테고리 삭제
                    await _categoryService.DeleteCategoryAsync(subCategory);
                }
            }

            #endregion*/

            return Json(new { Success = true, Message = "상품의 카테고리가 성공적으로 업데이트되었습니다." });
        }

        #endregion

        protected virtual async Task UpdateLocalesAsync(Product product, ProductModel model, bool api = false)
        {
            model.Locales = model.Locales.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            // 번역이 필요한 언어 ID 목록을 확인
            var requiredLanguages = new List<int> { 1, 2, 3 }; // 1: 영어, 2: 한국어, 3: 중국어 

            // model.Locales에 필요한 번역이 모두 있는지 확인
            var existingLanguagesWithName = model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)).Select(l => l.LanguageId).ToList();
            //var existingLanguagesWithShortDescription = model.Locales.Where(l => !string.IsNullOrEmpty(l.ShortDescription)).Select(l => l.LanguageId).ToList();

            // && requiredLanguages.All(r => existingLanguagesWithShortDescription.Contains(r))
            // 이런식으로 번역할 필드 추가가 가능함

            if (requiredLanguages.All(r => existingLanguagesWithName.Contains(r)))
            {
                var isUnchanged = true;
                foreach (var locale in model.Locales)
                {
                    var currentLocalizedValue = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, product.Id, "Product", "Name");
                    if (currentLocalizedValue != locale.Name)
                    {
                        isUnchanged = false;
                        api = true;
                        break;
                    }
                }

                if (isUnchanged)
                    return; // 모든 번역이 있고 변경되지 않았다면 메서드 종료
            }

            // api가 true라면 번역만 건너뛰고 나머지 작업을 수행
            if (!api)
            {
                var allLanguages = new Dictionary<string, int>
                {
                    { "en-US", 1 },
                    { "ko", 2 },
                    { "zh", 3 }
                };

                // 기본 번역값으로 모든 언어에 대해 model.Name을 설정
                var translations = new Dictionary<string, string>
                {
                    { "en-US", model.Name },
                    { "ko", model.Name },
                    { "zh", model.Name }
                };

                // model.Locales에서 Name이 설정되어 있는 항목에 대한 언어 코드를 allLanguages에서 제거
                foreach (var locale in model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)))
                {
                    var langCode = ConvertToLanguageCode(locale.LanguageId.ToString());
                    if (allLanguages.ContainsKey(langCode))
                    {
                        allLanguages.Remove(langCode);
                    }
                }

                // allLanguages에 남아있는 언어 코드에 대해 번역 수행
                foreach (var langCode in allLanguages.Keys)
                {
                    if (langCode == "ko")
                    {
                        translations[langCode] = model.Name; // 한국어를 한국어로 번역하는 대신 기존 값을 사용
                    }
                    else if (int.TryParse(model.Name, out _))
                    {
                        translations[langCode] = model.Name; // model.Name이 숫자로만 구성되어 있다면 그대로 사용
                    }
                    else
                    {
                        translations[langCode] = await DeppLTranslateTextAsync(model.Name, "ko", langCode);
                    }
                }

                // 번역된 결과를 model.Locales에 추가
                foreach (var entry in translations)
                {
                    if (allLanguages.ContainsKey(entry.Key))
                    {
                        model.Locales.Add(new ProductLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            Name = entry.Value ?? $"ProductLocalizedModel Name 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }
            }

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.ShortDescription,
                    localized.ShortDescription,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.FullDescription,
                    localized.FullDescription,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.MetaKeywords,
                    localized.MetaKeywords,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.MetaDescription,
                    localized.MetaDescription,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(product,
                    x => x.MetaTitle,
                    localized.MetaTitle,
                    localized.LanguageId);

                //search engine name
                var seName = await _urlRecordService.ValidateSeNameAsync(product, localized.SeName, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(product, seName, localized.LanguageId);
            }
        }

        private static string ConvertToLanguageCode(string languageId)
        {
            switch (languageId)
            {
                case "1":
                    return "en-US";
                case "2":
                    return "ko";
                case "3":
                    return "zh";
                default:
                    return languageId;
            }
        }

        protected virtual async Task UpdateLocalesAsync(ProductTag productTag, ProductTagModel model , bool api = false)
        {
            // 번역이 필요한 언어 ID 목록을 확인
            var requiredLanguages = new List<int> { 1, 2, 3 }; // 1: 영어, 2: 한국어, 3: 중국어 

            // model.Locales에 필요한 번역이 모두 있는지 확인
            var existingLanguagesWithName = model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)).Select(l => l.LanguageId).ToList();

            // && requiredLanguages.All(r => existingLanguagesWithShortDescription.Contains(r)
            // 이런식으로 번역할 필드 추가가 가능함

            if (requiredLanguages.All(r => existingLanguagesWithName.Contains(r)))
            {
                var isUnchanged = true;
                foreach (var locale in model.Locales)
                {
                    var currentLocalizedValue = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, productTag.Id, "ProductTag", "Name");
                    if (currentLocalizedValue != locale.Name)
                    {
                        isUnchanged = false;
                        api = true;
                        break;
                    }
                }

                if (isUnchanged)
                    return; // 모든 번역이 있고 변경되지 않았다면 메서드 종료
            }

            // api가 true라면 번역만 건너뛰고 나머지 작업을 수행
            if (!api)
            {
                var allLanguages = new Dictionary<string, int>
                {
                    { "en-US", 1 },
                    { "ko", 2 },
                    { "zh", 3 }
                };

                // 기본 번역값으로 모든 언어에 대해 model.Name을 설정
                var translations = new Dictionary<string, string>
                {
                    { "en-US", model.Name },
                    { "ko", model.Name },
                    { "zh", model.Name }
                };

                // model.Locales에서 Name이 설정되어 있는 항목에 대한 언어 코드를 allLanguages에서 제거
                foreach (var locale in model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)))
                {
                    var langCode = ConvertToLanguageCode(locale.LanguageId.ToString());
                    if (allLanguages.ContainsKey(langCode))
                    {
                        allLanguages.Remove(langCode);
                    }
                }

                // allLanguages에 남아있는 언어 코드에 대해 번역 수행 (단, 'ko'는 제외)
                foreach (var langCode in allLanguages.Keys.Except(new[] { "ko" }))
                {
                    translations[langCode] = await DeppLTranslateTextAsync(model.Name, "ko", langCode);
                }

                // 번역된 결과를 model.Locales에 추가
                foreach (var entry in translations)
                {
                    if (allLanguages.ContainsKey(entry.Key))
                    {
                        model.Locales.Add(new ProductTagLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            Name = entry.Value ?? $"ProductTagLocalizedModel Name 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }
            }

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(productTag,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                var seName = await _urlRecordService.ValidateSeNameAsync(productTag, string.Empty, localized.Name, false);
                await _urlRecordService.SaveSlugAsync(productTag, seName, localized.LanguageId);
            }
        }

        protected virtual async Task UpdateLocalesAsync(ProductAttributeMapping pam, ProductAttributeMappingModel model , bool api = false)
        {
            // 번역이 필요한 언어 ID 목록을 확인
            var requiredLanguages = new List<int> { 1, 2, 3 }; // 1: 영어, 2: 한국어, 3: 중국어 

            // model.Locales에 필요한 번역이 모두 있는지 확인
            var existingLanguagesWithName = model.Locales.Where(l => !string.IsNullOrEmpty(l.TextPrompt)).Select(l => l.LanguageId).ToList();
            var existingLanguagesWithDefaultValue = model.Locales.Where(l => !string.IsNullOrEmpty(l.DefaultValue)).Select(l => l.LanguageId).ToList();

            // 이런식으로 번역할 필드 추가가 가능함

            if (requiredLanguages.All(r => existingLanguagesWithName.Contains(r)) && requiredLanguages.All(r => existingLanguagesWithDefaultValue.Contains(r)))
            {
                var isUnchanged = true;
                foreach (var locale in model.Locales)
                {
                    var currentLocalizedValueTextPrompt = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, pam.Id, "ProductAttributeMapping", "TextPrompt");
                    var currentLocalizedValueDefaultValue = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, pam.Id, "ProductAttributeMapping", "DefaultValue");

                    if (currentLocalizedValueTextPrompt != locale.TextPrompt || currentLocalizedValueDefaultValue != locale.DefaultValue)
                    {
                        isUnchanged = false;
                        api = true;
                        break;
                    }
                }

                if (isUnchanged)
                    return; // 모든 번역이 있고 변경되지 않았다면 메서드 종료
            }

            // api가 true라면 번역만 건너뛰고 나머지 작업을 수행
            if (!api)
            {
                var allLanguages = new Dictionary<string, int>
                {
                    { "en-US", 1 },
                    { "ko", 2 },
                    { "zh", 3 }
                };

                // 기본 번역값으로 모든 언어에 대해 model.Name을 설정
                var translations = new Dictionary<string, string>
                {
                    { "en-US", model.TextPrompt },
                    { "ko", model.TextPrompt },
                    { "zh", model.TextPrompt }
                };

                var defaultTranslations = new Dictionary<string, string>
                {
                    { "en-US", model.DefaultValue },
                    { "ko", model.DefaultValue },
                    { "zh", model.DefaultValue }
                };

                // model.Locales에서 Name이 설정되어 있는 항목에 대한 언어 코드를 allLanguages에서 제거
                foreach (var locale in model.Locales.Where(l => !string.IsNullOrEmpty(l.TextPrompt)))
                {
                    var langCode = ConvertToLanguageCode(locale.LanguageId.ToString());
                    if (allLanguages.ContainsKey(langCode))
                    {
                        allLanguages.Remove(langCode);
                    }
                }

                // allLanguages에 남아있는 언어 코드에 대해 번역 수행 (단, 'ko'는 제외)
                foreach (var langCode in allLanguages.Keys.Except(new[] { "ko" }))
                {
                    translations[langCode] = await DeppLTranslateTextAsync(model.TextPrompt, "ko", langCode);
                }

                foreach (var langCode in allLanguages.Keys.Except(new[] { "ko" }))
                {
                    defaultTranslations[langCode] = await DeppLTranslateTextAsync(model.DefaultValue, "ko", langCode);
                }
                // 번역된 결과를 model.Locales에 추가
                foreach (var entry in translations)
                {
                    if (allLanguages.ContainsKey(entry.Key))
                    {
                        model.Locales.Add(new ProductAttributeMappingLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            TextPrompt = entry.Value ?? $"ProductAttributeMappingLocalizedModel Name 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }

                foreach (var entry in defaultTranslations)
                {
                    if (allLanguages.ContainsKey(entry.Key))
                    {
                        model.Locales.Add(new ProductAttributeMappingLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            DefaultValue = entry.Value ?? $"ProductAttributeMappingLocalizedModel DefaultValue 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }
            }

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(pam,
                    x => x.TextPrompt,
                    localized.TextPrompt,
                    localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(pam,
                    x => x.DefaultValue,
                    localized.DefaultValue,
                    localized.LanguageId);
            }
        }

        protected virtual async Task UpdateLocalesAsync(ProductAttribute productAttribute, ProductAttributeModel model, bool api = false)
        {
            model.Locales = model.Locales.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            // 번역이 필요한 언어 ID 목록을 확인
            var requiredLanguages = new List<int> { 1, 2, 3 }; // 1: 영어, 2: 한국어, 3: 중국어 

            // model.Locales에 필요한 번역이 모두 있는지 확인
            var existingLanguagesWithName = model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)).Select(l => l.LanguageId).ToList();
            //var existingLanguagesWithShortDescription = model.Locales.Where(l => !string.IsNullOrEmpty(l.ShortDescription)).Select(l => l.LanguageId).ToList();

            // && requiredLanguages.All(r => existingLanguagesWithShortDescription.Contains(r)
            // 이런식으로 번역할 필드 추가가 가능함

            if (requiredLanguages.All(r => existingLanguagesWithName.Contains(r)))
            {
                var isUnchanged = true;
                foreach (var locale in model.Locales)
                {
                    var currentLocalizedValue = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, productAttribute.Id, "Product", "Name");
                    if (currentLocalizedValue != locale.Name)
                    {
                        isUnchanged = false;
                        api = true;
                        break;
                    }
                }

                if (isUnchanged)
                    return; // 모든 번역이 있고 변경되지 않았다면 메서드 종료
            }

            // api가 true라면 번역만 건너뛰고 나머지 작업을 수행
            if (!api)
            {
                var allLanguages = new Dictionary<string, int>
                {
                    { "en-US", 1 },
                    { "ko", 2 },
                    { "zh", 3 }
                };

                // 기본 번역값으로 모든 언어에 대해 model.Name을 설정
                var translations = new Dictionary<string, string>
                {
                    { "en-US", model.Name },
                    { "ko", model.Name },
                    { "zh", model.Name }
                };

                // model.Locales에서 Name이 설정되어 있는 항목에 대한 언어 코드를 allLanguages에서 제거
                foreach (var locale in model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)))
                {
                    var langCode = ConvertToLanguageCode(locale.LanguageId.ToString());
                    if (allLanguages.ContainsKey(langCode))
                    {
                        allLanguages.Remove(langCode);
                    }
                }

                // allLanguages에 남아있는 언어 코드에 대해 번역 수행
                foreach (var langCode in allLanguages.Keys)
                {
                    if (langCode == "ko")
                    {
                        translations[langCode] = model.Name; // 한국어를 한국어로 번역하는 대신 기존 값을 사용
                    }
                    else if (int.TryParse(model.Name, out _))
                    {
                        translations[langCode] = model.Name; // model.Name이 숫자로만 구성되어 있다면 그대로 사용
                    }
                    else
                    {
                        translations[langCode] = await DeppLTranslateTextAsync(model.Name, "ko", langCode);
                    }
                }

                // 번역된 결과를 model.Locales에 추가
                foreach (var entry in translations)
                {
                    if (allLanguages.ContainsKey(entry.Key))
                    {
                        model.Locales.Add(new ProductAttributeLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            Name = entry.Value ?? $"ProductLocalizedModel Name 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }
            }

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(productAttribute,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(productAttribute,
                    x => x.Description,
                    localized.Description,
                    localized.LanguageId);
            }
        }

        protected virtual async Task UpdateLocalesAsync(ProductAttributeValue pav, ProductAttributeValueModel model, bool api = false)
        {
            model.Locales = model.Locales.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();

            // 번역이 필요한 언어 ID 목록을 확인
            var requiredLanguages = new List<int> { 1, 2, 3 }; // 1: 영어, 2: 한국어, 3: 중국어 

            // model.Locales에 필요한 번역이 모두 있는지 확인
            var existingLanguagesWithName = model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)).Select(l => l.LanguageId).ToList();
            //var existingLanguagesWithShortDescription = model.Locales.Where(l => !string.IsNullOrEmpty(l.ShortDescription)).Select(l => l.LanguageId).ToList();

            // && requiredLanguages.All(r => existingLanguagesWithShortDescription.Contains(r)
            // 이런식으로 번역할 필드 추가가 가능함

            if (requiredLanguages.All(r => existingLanguagesWithName.Contains(r)))
            {
                var isUnchanged = true;
                foreach (var locale in model.Locales)
                {
                    var currentLocalizedValue = await _localizedEntityService.GetLocalizedValueAsync(locale.LanguageId, pav.Id, "Product", "Name");
                    if (currentLocalizedValue != locale.Name)
                    {
                        isUnchanged = false;
                        api = true;
                        break;
                    }
                }

                if (isUnchanged)
                    return; // 모든 번역이 있고 변경되지 않았다면 메서드 종료
            }

            // api가 true라면 번역만 건너뛰고 나머지 작업을 수행
            if (!api)
            {
                var allLanguages = new Dictionary<string, int>
                {
                    { "en-US", 1 },
                    { "ko", 2 },
                    { "zh", 3 }
                };

                // 기본 번역값으로 모든 언어에 대해 model.Name을 설정
                var translations = new Dictionary<string, string>
                {
                    { "en-US", model.Name },
                    { "ko", model.Name },
                    { "zh", model.Name }
                };

                // model.Locales에서 Name이 설정되어 있는 항목에 대한 언어 코드를 allLanguages에서 제거
                foreach (var locale in model.Locales.Where(l => !string.IsNullOrEmpty(l.Name)))
                {
                    var langCode = ConvertToLanguageCode(locale.LanguageId.ToString());
                    if (allLanguages.ContainsKey(langCode))
                    {
                        allLanguages.Remove(langCode);
                    }
                }

                // allLanguages에 남아있는 언어 코드에 대해 번역 수행
                foreach (var langCode in allLanguages.Keys)
                {
                    if (langCode == "ko")
                    {
                        translations[langCode] = model.Name; // 한국어를 한국어로 번역하는 대신 기존 값을 사용
                    }
                    else if (int.TryParse(model.Name, out _))
                    {
                        translations[langCode] = model.Name; // model.Name이 숫자로만 구성되어 있다면 그대로 사용
                    }
                    else
                    {
                        translations[langCode] = await DeppLTranslateTextAsync(model.Name, "ko", langCode);
                    }
                }

                // 번역된 결과를 model.Locales에 추가
                foreach (var entry in translations)
                {
                    if (allLanguages.ContainsKey(entry.Key))
                    {
                        model.Locales.Add(new ProductAttributeValueLocalizedModel
                        {
                            LanguageId = allLanguages[entry.Key],
                            Name = entry.Value ?? $"ProductLocalizedModel Name 값을 불러오지 못했습니다. (Language: {entry.Key})"
                        });
                    }
                }
            }

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(pav,
                    x => x.Name,
                    localized.Name,
                    localized.LanguageId);
            }
        }

        protected virtual async Task UpdatePictureSeoNamesAsync(Product product)
        {
            foreach (var pp in await _productService.GetProductPicturesByProductIdAsync(product.Id))
                await _pictureService.SetSeoFilenameAsync(pp.PictureId, await _pictureService.GetPictureSeNameAsync(product.Name));
        }

        protected virtual async Task SaveProductAclAsync(Product product, ProductModel model)
        {
            product.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
            await _productService.UpdateProductAsync(product);

            var existingAclRecords = await _aclService.GetAclRecordsAsync(product);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (!existingAclRecords.Any(acl => acl.CustomerRoleId == customerRole.Id))
                        await _aclService.InsertAclRecordAsync(product, customerRole.Id);
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                }
            }
        }

        protected virtual async Task SaveCategoryMappingsAsync(Product product, ProductModel model)
        {
            var existingProductCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);

            //delete categories
            foreach (var existingProductCategory in existingProductCategories)
                if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                    await _categoryService.DeleteProductCategoryAsync(existingProductCategory);

            //add categories
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                if (_categoryService.FindProductCategory(existingProductCategories, product.Id, categoryId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingCategoryMapping = await _categoryService.GetProductCategoriesByCategoryIdAsync(categoryId, showHidden: true);
                    if (existingCategoryMapping.Any())
                        displayOrder = existingCategoryMapping.Max(x => x.DisplayOrder) + 1;
                    await _categoryService.InsertProductCategoryAsync(new ProductCategory
                    {
                        ProductId = product.Id,
                        CategoryId = categoryId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        protected virtual async Task SaveManufacturerMappingsAsync(Product product, ProductModel model)
        {
            var existingProductManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);

            //delete manufacturers
            foreach (var existingProductManufacturer in existingProductManufacturers)
                if (!model.SelectedManufacturerIds.Contains(existingProductManufacturer.ManufacturerId))
                    await _manufacturerService.DeleteProductManufacturerAsync(existingProductManufacturer);

            //add manufacturers
            foreach (var manufacturerId in model.SelectedManufacturerIds)
            {
                if (_manufacturerService.FindProductManufacturer(existingProductManufacturers, product.Id, manufacturerId) == null)
                {
                    //find next display order
                    var displayOrder = 1;
                    var existingManufacturerMapping = await _manufacturerService.GetProductManufacturersByManufacturerIdAsync(manufacturerId, showHidden: true);
                    if (existingManufacturerMapping.Any())
                        displayOrder = existingManufacturerMapping.Max(x => x.DisplayOrder) + 1;
                    await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                    {
                        ProductId = product.Id,
                        ManufacturerId = manufacturerId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        protected virtual async Task SaveDiscountMappingsAsync(Product product, ProductModel model)
        {
            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true, isActive: null);

            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is null)
                        await _productService.InsertDiscountProductMappingAsync(new DiscountProductMapping { EntityId = product.Id, DiscountId = discount.Id });
                }
                else
                {
                    //remove discount
                    if (await _productService.GetDiscountAppliedToProductAsync(product.Id, discount.Id) is DiscountProductMapping discountProductMapping)
                        await _productService.DeleteDiscountProductMappingAsync(discountProductMapping);
                }
            }

            await _productService.UpdateProductAsync(product);
            await _productService.UpdateHasDiscountsAppliedAsync(product);
        }

        protected virtual async Task<string> GetAttributesXmlForProductAttributeCombinationAsync(IFormCollection form, List<string> warnings, int productId)
        {
            var attributesXml = string.Empty;

            //get product attribute mappings (exclude non-combinable attributes)
            var attributes = (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId))
                .Where(productAttributeMapping => !productAttributeMapping.IsNonCombinable()).ToList();

            foreach (var attribute in attributes)
            {
                var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                StringValues ctrlAttributes;

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        ctrlAttributes = form[controlId];
                        if (!string.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.Checkboxes:
                        var cblAttributes = form[controlId].ToString();
                        if (!string.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.Split(new[] { ',' },
                                StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }

                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        //load read-only (already server-side selected) values
                        var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                        }

                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        ctrlAttributes = form[controlId];
                        if (!string.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, enteredText);
                        }

                        break;
                    case AttributeControlType.Datepicker:
                        var date = form[controlId + "_day"];
                        var month = form[controlId + "_month"];
                        var year = form[controlId + "_year"];
                        DateTime? selectedDate = null;
                        try
                        {
                            selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                        }
                        catch
                        {
                            //ignore any exception
                        }

                        if (selectedDate.HasValue)
                        {
                            attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                attribute, selectedDate.Value.ToString("D"));
                        }

                        break;
                    case AttributeControlType.FileUpload:
                        var httpPostedFile = Request.Form.Files[controlId];
                        if (!string.IsNullOrEmpty(httpPostedFile?.FileName))
                        {
                            var fileSizeOk = true;
                            if (attribute.ValidationFileMaximumSize.HasValue)
                            {
                                //compare in bytes
                                var maxFileSizeBytes = attribute.ValidationFileMaximumSize.Value * 1024;
                                if (httpPostedFile.Length > maxFileSizeBytes)
                                {
                                    warnings.Add(string.Format(
                                        await _localizationService.GetResourceAsync("ShoppingCart.MaximumUploadedFileSize"),
                                        attribute.ValidationFileMaximumSize.Value));
                                    fileSizeOk = false;
                                }
                            }

                            if (fileSizeOk)
                            {
                                //save an uploaded file
                                var download = new Download
                                {
                                    DownloadGuid = Guid.NewGuid(),
                                    UseDownloadUrl = false,
                                    DownloadUrl = string.Empty,
                                    DownloadBinary = await _downloadService.GetDownloadBitsAsync(httpPostedFile),
                                    ContentType = httpPostedFile.ContentType,
                                    Filename = _fileProvider.GetFileNameWithoutExtension(httpPostedFile.FileName),
                                    Extension = _fileProvider.GetFileExtension(httpPostedFile.FileName),
                                    IsNew = true
                                };
                                await _downloadService.InsertDownloadAsync(download);

                                //save attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            //validate conditional attributes (if specified)
            foreach (var attribute in attributes)
            {
                var conditionMet = await _productAttributeParser.IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = _productAttributeParser.RemoveProductAttribute(attributesXml, attribute);
                }
            }

            return attributesXml;
        }

        protected virtual string[] ParseProductTags(string productTags)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(productTags))
                return result.ToArray();

            var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var val in values)
                if (!string.IsNullOrEmpty(val.Trim()))
                    result.Add(val.Trim());

            return result.ToArray();
        }

        protected virtual async Task SaveProductWarehouseInventoryAsync(Product product, ProductModel model)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (model.ManageInventoryMethodId != (int)ManageInventoryMethod.ManageStock)
                return;

            if (!model.UseMultipleWarehouses)
                return;

            var warehouses = await _shippingService.GetAllWarehousesAsync();

            var formData = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());

            foreach (var warehouse in warehouses)
            {
                //parse stock quantity
                var stockQuantity = 0;
                foreach (var formKey in formData.Keys)
                {
                    if (!formKey.Equals($"warehouse_qty_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    _ = int.TryParse(formData[formKey], out stockQuantity);
                    break;
                }

                //parse reserved quantity
                var reservedQuantity = 0;
                foreach (var formKey in formData.Keys)
                    if (formKey.Equals($"warehouse_reserved_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = int.TryParse(formData[formKey], out reservedQuantity);
                        break;
                    }

                //parse "used" field
                var used = false;
                foreach (var formKey in formData.Keys)
                    if (formKey.Equals($"warehouse_used_{warehouse.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _ = int.TryParse(formData[formKey], out var tmp);
                        used = tmp == warehouse.Id;
                        break;
                    }

                //quantity change history message
                var message = $"{await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} {await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit")}";

                var existingPwI = (await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id)).FirstOrDefault(x => x.WarehouseId == warehouse.Id);
                if (existingPwI != null)
                {
                    if (used)
                    {
                        var previousStockQuantity = existingPwI.StockQuantity;

                        //update existing record
                        existingPwI.StockQuantity = stockQuantity;
                        existingPwI.ReservedQuantity = reservedQuantity;
                        await _productService.UpdateProductWarehouseInventoryAsync(existingPwI);

                        //quantity change history
                        await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity - previousStockQuantity, existingPwI.StockQuantity,
                            existingPwI.WarehouseId, message);
                    }
                    else
                    {
                        //delete. no need to store record for qty 0
                        await _productService.DeleteProductWarehouseInventoryAsync(existingPwI);

                        //quantity change history
                        await _productService.AddStockQuantityHistoryEntryAsync(product, -existingPwI.StockQuantity, 0, existingPwI.WarehouseId, message);
                    }
                }
                else
                {
                    if (!used)
                        continue;

                    //no need to insert a record for qty 0
                    existingPwI = new ProductWarehouseInventory
                    {
                        WarehouseId = warehouse.Id,
                        ProductId = product.Id,
                        StockQuantity = stockQuantity,
                        ReservedQuantity = reservedQuantity
                    };

                    await _productService.InsertProductWarehouseInventoryAsync(existingPwI);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, existingPwI.StockQuantity, existingPwI.StockQuantity,
                        existingPwI.WarehouseId, message);
                }
            }
        }

        protected virtual async Task SaveConditionAttributesAsync(ProductAttributeMapping productAttributeMapping,
            ProductAttributeConditionModel model, IFormCollection form)
        {
            string attributesXml = null;
            if (model.EnableCondition)
            {
                var attribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.SelectedProductAttributeId);
                if (attribute != null)
                {
                    var controlId = $"{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                //for conditions we should empty values save even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null, attribute,
                                    selectedAttributeId > 0 ? selectedAttributeId.ToString() : string.Empty);
                            }
                            else
                            {
                                //for conditions we should empty values save even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null,
                                    attribute, string.Empty);
                            }

                            break;
                        case AttributeControlType.Checkboxes:
                            var cblAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(cblAttributes))
                            {
                                var anyValueSelected = false;
                                foreach (var item in cblAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId <= 0)
                                        continue;

                                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                                    anyValueSelected = true;
                                }

                                if (!anyValueSelected)
                                {
                                    //for conditions we should save empty values even when nothing is selected
                                    //otherwise "attributesXml" will be empty
                                    //hence we won't be able to find a selected attribute
                                    attributesXml = _productAttributeParser.AddProductAttribute(null,
                                        attribute, string.Empty);
                                }
                            }
                            else
                            {
                                //for conditions we should save empty values even when nothing is selected
                                //otherwise "attributesXml" will be empty
                                //hence we won't be able to find a selected attribute
                                attributesXml = _productAttributeParser.AddProductAttribute(null,
                                    attribute, string.Empty);
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are supported as conditions
                            break;
                    }
                }
            }

            productAttributeMapping.ConditionAttributeXml = attributesXml;
            await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);
        }

        protected virtual async Task GenerateAttributeCombinationsAsync(Product product, IList<int> allowedAttributeIds = null)
        {
            var allAttributesXml = await _productAttributeParser.GenerateAllCombinationsAsync(product, true, allowedAttributeIds);
            foreach (var attributesXml in allAttributesXml)
            {
                var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);

                //already exists?
                if (existingCombination != null)
                    continue;

                //new one
                var warnings = new List<string>();
                warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
                    ShoppingCartType.ShoppingCart, product, 1, attributesXml, true, true, true));
                if (warnings.Count != 0)
                    continue;

                //save combination
                var combination = new ProductAttributeCombination
                {
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    StockQuantity = 0,
                    AllowOutOfStockOrders = false,
                    Sku = null,
                    ManufacturerPartNumber = null,
                    Gtin = null,
                    OverriddenPrice = null,
                    NotifyAdminForQuantityBelow = 1,
                    PictureId = 0
                };
                await _productAttributeService.InsertProductAttributeCombinationAsync(combination);
            }
        }

        protected virtual async Task PingVideoUrlAsync(string videoUrl)
        {
            var path = videoUrl.StartsWith("/") ? $"{_webHelper.GetStoreLocation()}{videoUrl.TrimStart('/')}" : videoUrl;

            var client = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
            await client.GetStringAsync(path);
        }

        #endregion

        #region Methods

        #region SaveImages

        public class ImageData
        {
            public string Url { get; set; }
            public string Base64String { get; set; }
        }

        public class Base64ImageObject
        {
            public string Url { get; set; }
            public string Base64String { get; set; } // 추가
        }

        // URL에서 확장자를 가져와 MIME 타입을 반환하는 함수
        async Task<string> GetMimeTypeFromUrl(string url)
        {
            var extension = System.IO.Path.GetExtension(url).ToLowerInvariant(); // System.IO.Path 사용

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                case ".tiff":
                case ".tif":
                    return "image/tiff";
                case ".ico":
                    return "image/x-icon";
                case ".svg":
                    return "image/svg+xml";
                case ".heif":
                case ".heic":
                    return "image/heif";
                // 필요한 경우 추가 확장자를 포함시킬 수 있습니다.
                default:
                    return await GetMimeTypeFromUrlAsync(url);
            }
        }

        async Task<string> GetMimeTypeFromUrlAsync(string url)
        {
            using (var httpClient = new HttpClient())
            {
                // HEAD 요청을 사용하여 리소스의 헤더만 가져옴 (전체 콘텐츠를 가져오지 않음)
                var request = new HttpRequestMessage(HttpMethod.Head, url);

                try
                {
                    var response = await httpClient.SendAsync(request);

                    // 응답이 성공적이고 Content-Type 헤더가 있으면 해당 값을 반환
                    if (response.IsSuccessStatusCode && response.Content.Headers.Contains("Content-Type"))
                    {
                        return response.Content.Headers.GetValues("Content-Type").FirstOrDefault();
                    }
                    else
                    {
                        // 성공적인 응답이 아니거나 Content-Type 헤더가 없는 경우 기본값으로 반환
                        return "application/octet-stream";
                    }
                }
                catch
                {
                    // 요청 중 오류가 발생한 경우 기본값으로 반환
                    return "application/octet-stream";
                }
            }
        }

        private async Task<List<Base64ImageObject>> ConvertImagesToBase64Async(List<string> imageUrlList)
        {
            var imageBase64List = new List<Base64ImageObject>();
            var httpClient = _httpClientFactory.CreateClient();
            int maxAttempts = 3;

            foreach (var imageUrl in imageUrlList)
            {
                int attempts = 0;
                while (attempts < maxAttempts)
                {
                    try
                    {
                        var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                        var base64String = Convert.ToBase64String(imageBytes);

                        imageBase64List.Add(new Base64ImageObject
                        {
                            Url = imageUrl,
                            Base64String = base64String
                        });
                        break;
                    }
                    catch (Exception ex)
                    {
                        attempts++;
                        if (attempts == maxAttempts)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to fetch image from {imageUrl} after {maxAttempts} attempts: {ex.Message}");
                        }
                    }
                }
            }

            return imageBase64List;
        }

        #endregion

        #region Translate Google

        public async Task<string> DetectLanguageAsync(string text)
        {
            try
            {
                var apiKey = "AIzaSyD4CI-ZD19kRHdzp-8Ag9hC_sEdNc6JZnY";  // 추후에 환경 변수나 구성 파일에서 읽어오는 것으로 구현
                var url = $"https://translation.googleapis.com/language/translate/v2/detect?key={apiKey}&q={text}";

                using var client = new HttpClient();
                System.Diagnostics.Debug.WriteLine($"Detecting language for text: {text}");

                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonObject = JObject.Parse(jsonResponse);

                var detectedLanguage = jsonObject["data"]["detections"][0][0]["language"].ToString();

                // 만약 감지된 언어가 "zh-"로 시작하면 "zh-CN"으로 강제 설정
                if (detectedLanguage.StartsWith("zh-"))
                {
                    detectedLanguage = "zh-CN";
                }
                // 만약 감지된 언어가 "ar-Latn"이면 "ar"로 강제 설정
                else if (detectedLanguage == "ar-Latn")
                {
                    detectedLanguage = "ar";
                }

                System.Diagnostics.Debug.WriteLine($"Detected language for detectedLanguage: {detectedLanguage}");

                return detectedLanguage;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Google Translate API 호출 중 오류 발생: {ex.Message}");
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"JSON 파싱 중 오류 발생: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"서버 내부 오류: {ex.Message}");
            }
        }

        #endregion

        #region Translate DeppL

        public async Task<string> DeppLTranslateTextAsync(string text, string sourceLanguage, string targetLanguage)
        {
            var authKey = "67d0450a-748d-4bf2-d0ba-cf5dad9fa30f:fx";  // DeepL의 Auth Key
            System.Diagnostics.Debug.WriteLine($"Translating text: '{text}' from {sourceLanguage} to {targetLanguage}");

            var translator = new Translator(authKey);

            // sourceLanguage가 null이거나 빈 문자열일 경우 null을 전달
            var actualSourceLanguage = string.IsNullOrWhiteSpace(sourceLanguage) ? null : sourceLanguage;
            var translatedText = await translator.TranslateTextAsync(text, actualSourceLanguage, targetLanguage);

            System.Diagnostics.Debug.WriteLine($"Translated text: '{translatedText.Text}'");
            return translatedText.Text;
        }

        #endregion

        #region ApiJd

        public async Task<List<ProductInfo>> GetProductsFromJdAsync(
            string categoryName,
            bool hasDiscount = true,
            bool inStock = true,
            int page = 1,
            int size = 200,
            string sort = "volume_desc",
            int minVendorRating = 3,
            int maxVendorRating = 5,
            string queryLanguage = "en",
            string targetLanguage = "en")
        {
            const string API_KEY = "e9e7dd9c85msh4c01ab54707ebc8p120a38jsn0ab00d00a6cf";
            const string API_HOST = "jingdong-Jing-Dong-data-service.p.rapidapi.com";

            var maxRetryCount = 3; // 최대 재시도 횟수
            var retryCount = 0;

            while (retryCount < maxRetryCount)
            {
                try
                {
                    var client = new RestClient("https://jingdong-jing-dong-data-service.p.rapidapi.com/search/searchItems");
                    var request = new RestRequest(Method.GET);

                    request.AddQueryParameter("query", categoryName);
                    request.AddQueryParameter("hasDiscount", hasDiscount.ToString());
                    request.AddQueryParameter("inStock", inStock.ToString());
                    request.AddQueryParameter("page", page.ToString());
                    request.AddQueryParameter("size", size.ToString());
                    request.AddQueryParameter("sort", sort);
                    request.AddQueryParameter("minVendorRating", minVendorRating.ToString());
                    request.AddQueryParameter("maxVendorRating", maxVendorRating.ToString());
                    request.AddQueryParameter("query_language", queryLanguage);
                    request.AddQueryParameter("target_language", targetLanguage);

                    request.AddHeader("X-RapidAPI-Key", API_KEY);
                    request.AddHeader("X-RapidAPI-Host", API_HOST);

                    IRestResponse response = await client.ExecuteAsync(request);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"API call failed with status code: {response.StatusCode}");
                    }

                    var productDataList = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    var productList = new List<ProductInfo>();

                    if (productDataList != null && productDataList.items != null)
                    {
                        foreach (var productData in productDataList.items)
                        {
                            var url = productData.detail_url.ToString();
                            var title = productData.title.ToString();
                            var originalTitle = productData.original_title.ToString();
                            var category_id = productData.category_id.ToString();
                            var id = productData.num_iid.ToString();

                            productList.Add(new ProductInfo { Url = url, Title = title, OriginalTitle = originalTitle, Id = id });
                        }
                    }
                    else
                    {
                        throw new Exception("The response from the API was not as expected.");
                    }

                    return productList;
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetryCount)
                    {
                        System.Diagnostics.Debug.WriteLine($"GetProductsFromJdAsync Error Text: '{ex.Message}'");
                        return null;
                    }

                    await Task.Delay(1000 * retryCount);
                }
            }

            return null;
        }


        public async Task<TaobaoProductInfo> GetProductFromJdAsync(string productId)
        {
            var maxRetryCount = 3; // 최대 재시도 횟수
            var retryCount = 0;
            while (retryCount < maxRetryCount)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"productId text: '{productId}'");

                    var client = new HttpClient();
                    var request = new HttpRequestMessage(HttpMethod.Get, $"https://jingdong-jing-dong-data-service.p.rapidapi.com/item/itemFullInfo?itemId={productId}");
                    request.Headers.Add("X-RapidAPI-Key", "e9e7dd9c85msh4c01ab54707ebc8p120a38jsn0ab00d00a6cf");
                    request.Headers.Add("X-RapidAPI-Host", "jingdong-Jing-Dong-data-service.p.rapidapi.com");
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(responseContent);

                    var productInfo = new TaobaoProductInfo
                    {
                        CatId = data.ContainsKey("cat_id") ? data["cat_id"].ToString() : string.Empty,
                        CreatedTime = data.ContainsKey("created_time") ? data["created_time"].ToString() : string.Empty,
                        Description = data.ContainsKey("description") ? new Description
                        {
                            Html = data["description"]["html"]?.ToString(),
                            Images = data["description"]["images"]?.ToObject<List<string>>()
                        } : null,
                        DetailUrl = data.ContainsKey("detail_url") ? data["detail_url"].ToString() : string.Empty,
                        IsTmall = data.ContainsKey("isTmall") ? data["isTmall"].ToObject<bool>() : false,
                        ItemImgs = data.ContainsKey("item_imgs") ? data["item_imgs"].ToObject<List<ImageObject>>() : new List<ImageObject>(),
                        NumIid = data.ContainsKey("num_iid") ? data["num_iid"].ToString() : string.Empty,
                        OriginalPrice = data.ContainsKey("original_price") ? Convert.ToDecimal(data["original_price"].ToString()) : 0m,
                        OriginalTitle = data.ContainsKey("original_title") ? data["original_title"].ToString() : string.Empty,
                        PicUrl = data.ContainsKey("pic_url") ? data["pic_url"].ToString() : string.Empty,
                        Price = data.ContainsKey("price") ? Convert.ToDecimal(data["price"].ToString()) : 0m,
                        Props = data.ContainsKey("props") ? data["props"].ToObject<List<Props>>() : new List<Props>(),
                        Title = data.ContainsKey("title") ? data["title"].ToString() : string.Empty,
                    };

                    if (data["skus"] is JArray skusArray)
                    {
                        productInfo.Skus = new List<SkuInfo>();
                        foreach (var skuItem in skusArray)
                        {
                            var skuInfo = new SkuInfo
                            {
                                SkuId = skuItem["sku_id"]?.ToString(),
                                Price = skuItem["price"]?.ToString(),
                                Quantity = skuItem["quantity"]?.ToString(),
                            };

                            var propValuesData = skuItem["properties_name"]?.ToString().Split(';');
                            if (propValuesData != null)
                            {
                                skuInfo.PropValues = new List<PropValue>();
                                foreach (var propValueData in propValuesData)
                                {
                                    var propIdValueName = propValueData.Split(':');
                                    if (propIdValueName.Length == 4)
                                    {
                                        var propValue = new PropValue
                                        {
                                            PropId = propIdValueName[0],
                                            ValueId = propIdValueName[1],
                                            Name = propIdValueName[2],
                                            Value = propIdValueName[3]
                                        };
                                        skuInfo.PropValues.Add(propValue);
                                    }
                                }
                            }

                            productInfo.Skus.Add(skuInfo);
                        }
                    }

                    if (data.ContainsKey("props_imgs") && data["props_imgs"] is JArray propImagesData)
                    {
                        productInfo.PropImages = new List<PropImage>();
                        foreach (var propImageItem in propImagesData)
                        {
                            var propImage = new PropImage
                            {
                                PropId = propImageItem["properties"].ToString().Split(':')[0],
                                ValueId = propImageItem["properties"].ToString().Split(':')[1],
                                Url = propImageItem["url"].ToString()
                            };
                            productInfo.PropImages.Add(propImage);
                        }
                    }

                    return productInfo;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    System.Diagnostics.Debug.WriteLine($"GetProductFromTaobaoAsync Error Text: '{ex.Message.ToString()}'");
                    if (retryCount >= maxRetryCount)
                    {
                        return null; // 재시도 횟수를 초과하면 null을 반환
                    }

                    // 잠시 대기 후 재시도
                    await Task.Delay(1000 * retryCount);
                }
            }

            return null; // 여기에 도달할 경우는 null을 반환
        }

        #endregion

        #region ApiTaobao

        public class TaobaoProductInfo
        {
            public string CatId { get; set; }
            public string CreatedTime { get; set; }
            public Description Description { get; set; }
            public string DetailUrl { get; set; }
            public bool IsTmall { get; set; }
            public List<ImageObject> ItemImgs { get; set; }
            public List<string> ItemCategories { get; set; }
            public string NumIid { get; set; }
            public decimal OriginalPrice { get; set; }
            public string OriginalTitle { get; set; }
            public string PicUrl { get; set; }
            public decimal Price { get; set; }
            public List<SkuInfo> Skus { get; set; }
            public List<Props> Props { get; set; }
            public List<PropImage> PropImages { get; set; }
            public List<SellerItem> SellerItems { get; set; }
            public string Title { get; set; }

            public string Features { get; set; }
            public List<string> ItemRecentReviews { get; set; }
            public int ItemReviewsCount { get; set; }
            public List<string> ItemVideos { get; set; }
            public string ItemWeight { get; set; }
            public string Location { get; set; }
            public int Quantity { get; set; }
            public bool SellPermitted { get; set; }
            public SellerInfo SellerInfo { get; set; }
            public string StuffStatus { get; set; }
            public string UpdatedTime { get; set; }
        }

        public class SellerInfo
        {
            public string Name { get; set; }
            public int Rating { get; set; }
        }

        public class Description
        {
            public string Html { get; set; }
            public List<string> Images { get; set; }
        }

        public class ImageObject
        {
            public bool IsMain { get; set; }
            public Dictionary<string, ImageSize> Sizes { get; set; }
            public string Url { get; set; }
        }

        public class ImageSize
        {
            public int Height { get; set; }
            public string Url { get; set; }
            public int Width { get; set; }
        }

        public class SkuInfo
        {
            public string SkuId { get; set; }
            public string Price { get; set; }
            public string Quantity { get; set; }
            public List<PropValue> PropValues { get; set; } // 각 SKU의 속성 값을 저장
        }

        public class PropValue
        {
            public string PropId { get; set; }  // 상품 속성 ID
            public string ValueId { get; set; } // 해당 속성의 값 ID
            public string Name { get; set; }    // 속성 이름 (예: "색상")
            public string Value { get; set; }   // 속성 값 (예: "파란색")
        }

        public class Props
        {
            [JsonProperty("is_configurator")]
            public bool IsConfigurator { get; set; }

            [JsonProperty("original_property_name")]
            public string OriginalPropertyName { get; set; }

            [JsonProperty("original_value")]
            public string OriginalValue { get; set; }

            [JsonProperty("product_id")]
            public string ProductId { get; set; }

            [JsonProperty("property_name")]
            public string PropertyName { get; set; }

            public string Value { get; set; }

            [JsonProperty("variation_id")]
            public string VariationId { get; set; }
        }

        public class PropImage
        {
            public string PropId { get; set; }
            public string ValueId { get; set; }
            public string Url { get; set; }
        }

        public class SellerItem
        {
            public string CategoryId { get; set; }
            public string CreatedTime { get; set; }
            public string DetailUrl { get; set; }
            public SellerItemFeatures Features { get; set; }
            public bool IsTmall { get; set; }
            public List<ImageObject> ItemImgs { get; set; }
        }

        public class SellerItemFeatures
        {
            public string Reviews { get; set; }
            public string SalesInLastDays { get; set; }
            public string TaobaoVendorId { get; set; }
            public string TotalSales { get; set; }
        }

        public class ProductInfo
        {
            public string Url { get; set; }
            public string Title { get; set; }
            public string OriginalTitle { get; set; }
            public string CategoryId { get; set; }
            public string Id { get; set; }
        }

        public async Task<List<ProductInfo>> GetProductsFromTaobaoAsync(
            string categoryId,
            int page = 1,
            int size = 200,
            string sort = "updated_time_desc,vendor_rating_desc",
            int minSellerRating = 3,
            int maxSellerRating = 5,
            string targetLanguage = "en")
        {
            int maxRetryCount = 3; // 최대 재시도 횟수
            int retryCount = 0;

            while (retryCount < maxRetryCount)
            {
                try
                {
                    var client = new RestClient($"https://taobao-tmall-tao-bao-data-service.p.rapidapi.com/category/categoryItems?categoryId={categoryId}&page={page}&size={size}&sort={sort}&minSellerRating={minSellerRating}&maxSellerRating={maxSellerRating}&target_language={targetLanguage}");

                    var request = new RestRequest(Method.GET);
                    request.AddHeader("X-RapidAPI-Key", "e9e7dd9c85msh4c01ab54707ebc8p120a38jsn0ab00d00a6cf");
                    request.AddHeader("X-RapidAPI-Host", "taobao-tmall-Tao-Bao-data-service.p.rapidapi.com");

                    var response = await client.ExecuteAsync(request);

                    System.Diagnostics.Debug.WriteLine($"GetProductsFromTaobaoAsync response text: '{response.Content}'");

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"API call failed with status code: {response.StatusCode}");
                    }

                    var productDataList = JsonConvert.DeserializeObject<dynamic>(response.Content);

                    var productList = new List<ProductInfo>();

                    if (productDataList != null && productDataList.items != null)
                    {
                        foreach (var productData in productDataList.items)
                        {
                            var url = productData.detail_url.ToString();
                            var title = productData.title.ToString();
                            var originalTitle = productData.original_title.ToString();
                            var category_id = productData.category_id.ToString();
                            var idMatch = Regex.Match(url, @"id=(\d+)");
                            if (idMatch.Success)
                            {
                                var id = idMatch.Groups[1].Value;
                                productList.Add(new ProductInfo { Url = url, Title = title, OriginalTitle = originalTitle, Id = id, CategoryId = category_id });
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("The response from the API was not as expected.");
                    }

                    return productList;
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetryCount)
                    {
                        System.Diagnostics.Debug.WriteLine($"GetProductFromTaobaoAsync Error Text: '{ex.Message.ToString()}'");
                        return null;
                    }

                    // 잠시 대기 후 재시도합니다.
                    await Task.Delay(1000 * retryCount);
                }
            }

            return null;
        }

        public async Task<List<ProductInfo>> GetcategoryNameProductsFromTaobaoAsync(
            string categoryName,
            int page = 1,
            int size = 200,
            string sort = "volume_desc", // vendor_rating_desc, updated_time_desc, total_price_asc, total_price_desc, price_asc, price_desc, volume_desc
            int minSellerRating = 3,
            int maxSellerRating = 5,
            string queryLanguage = "en",
            string targetLanguage = "en")
        {
            var maxRetryCount = 3; // 최대 재시도 횟수
            var retryCount = 0;

            while (retryCount < maxRetryCount)
            {
                try
                {
                    var encodedCategoryName = categoryName.Replace(" ", "%20");
                    var client = new RestClient($"https://taobao-tmall-tao-bao-data-service.p.rapidapi.com/search/searchItems?query={encodedCategoryName}&page={page}&size={size}&sort={sort}&target_language={targetLanguage}&query_language={queryLanguage}");

                    var request = new RestRequest(Method.GET);
                    request.AddHeader("X-RapidAPI-Key", "e9e7dd9c85msh4c01ab54707ebc8p120a38jsn0ab00d00a6cf");
                    request.AddHeader("X-RapidAPI-Host", "taobao-tmall-Tao-Bao-data-service.p.rapidapi.com");
                    IRestResponse response = await client.ExecuteAsync(request); // 비동기 호출

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new Exception($"API call failed with status code: {response.StatusCode}");
                    }

                    var productDataList = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    var productList = new List<ProductInfo>();

                    if (productDataList != null && productDataList.items != null)
                    {
                        foreach (var productData in productDataList.items)
                        {
                            var url = productData.detail_url.ToString();
                            var title = productData.title.ToString();
                            var originalTitle = productData.original_title.ToString();
                            var id = productData.num_iid.ToString();
                            productList.Add(new ProductInfo { Url = url, Title = title, OriginalTitle = originalTitle, Id = id });
                        }
                    }
                    else
                    {
                        throw new Exception("The response from the API was not as expected.");
                    }

                    return productList;
                }
                catch (Exception ex)
                {
                    retryCount++;

                    if (retryCount >= maxRetryCount)
                    {
                        System.Diagnostics.Debug.WriteLine($"GetcategoryNameProductsFromTaobaoAsync Error Text: '{ex.Message}'");
                        return null; // 재시도 횟수를 초과하면 null을 반환
                    }

                    // 잠시 대기 후 재시도
                    await Task.Delay(1000 * retryCount);
                }
            }

            return null; // 여기에 도달할 경우는 null을 반환
        }

        public async Task<TaobaoProductInfo> GetProductFromTaobaoAsync(string productId)
        {
            int maxRetryCount = 3; // 최대 재시도 횟수
            int retryCount = 0;

            while (retryCount < maxRetryCount)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"productId text: '{productId}'");

                    var client = new RestClient($"https://taobao-tmall-tao-bao-data-service.p.rapidapi.com/item/itemFullInfo?itemId={productId}");
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("X-RapidAPI-Key", "e9e7dd9c85msh4c01ab54707ebc8p120a38jsn0ab00d00a6cf");
                    request.AddHeader("X-RapidAPI-Host", "taobao-tmall-Tao-Bao-data-service.p.rapidapi.com");
                    var response = await client.ExecuteAsync(request);

                    var data = JObject.Parse(response.Content);

                    var productInfo = new TaobaoProductInfo
                    {
                        CatId = data.ContainsKey("cat_id") ? data["cat_id"].ToString() : string.Empty,
                        CreatedTime = data.ContainsKey("created_time") ? data["created_time"].ToString() : string.Empty,
                        Description = data.ContainsKey("description") ? new Description
                        {
                            Html = data["description"]["html"]?.ToString(),
                            Images = data["description"]["images"]?.ToObject<List<string>>()
                        } : null,
                        DetailUrl = data.ContainsKey("detail_url") ? data["detail_url"].ToString() : string.Empty,
                        IsTmall = data.ContainsKey("isTmall") ? data["isTmall"].ToObject<bool>() : false,
                        ItemImgs = data.ContainsKey("item_imgs") ? data["item_imgs"].ToObject<List<ImageObject>>() : new List<ImageObject>(),
                        NumIid = data.ContainsKey("num_iid") ? data["num_iid"].ToString() : string.Empty,
                        OriginalPrice = data.ContainsKey("original_price") ? Convert.ToDecimal(data["original_price"].ToString()) : 0m,
                        OriginalTitle = data.ContainsKey("original_title") ? data["original_title"].ToString() : string.Empty,
                        PicUrl = data.ContainsKey("pic_url") ? data["pic_url"].ToString() : string.Empty,
                        Price = data.ContainsKey("price") ? Convert.ToDecimal(data["price"].ToString()) : 0m,
                        Props = data.ContainsKey("props") ? data["props"].ToObject<List<Props>>() : new List<Props>(),
                        Title = data.ContainsKey("title") ? data["title"].ToString() : string.Empty,
                    };

                    if (data["skus"] is JArray skusArray)
                    {
                        productInfo.Skus = new List<SkuInfo>();
                        foreach (var skuItem in skusArray)
                        {
                            var skuInfo = new SkuInfo
                            {
                                SkuId = skuItem["sku_id"]?.ToString(),
                                Price = skuItem["price"]?.ToString(),
                                Quantity = skuItem["quantity"]?.ToString(),
                            };

                            var propValuesData = skuItem["properties_name"]?.ToString().Split(';');
                            if (propValuesData != null)
                            {
                                skuInfo.PropValues = new List<PropValue>();
                                foreach (var propValueData in propValuesData)
                                {
                                    var propIdValueName = propValueData.Split(':');
                                    if (propIdValueName.Length == 4)
                                    {
                                        var propValue = new PropValue
                                        {
                                            PropId = propIdValueName[0],
                                            ValueId = propIdValueName[1],
                                            Name = propIdValueName[2],
                                            Value = propIdValueName[3]
                                        };
                                        skuInfo.PropValues.Add(propValue);
                                    }
                                }
                            }

                            productInfo.Skus.Add(skuInfo);
                        }
                    }

                    if (data.ContainsKey("props_imgs") && data["props_imgs"] is JArray propImagesData)
                    {
                        productInfo.PropImages = new List<PropImage>();
                        foreach (var propImageItem in propImagesData)
                        {
                            var propImage = new PropImage
                            {
                                PropId = propImageItem["properties"].ToString().Split(':')[0],
                                ValueId = propImageItem["properties"].ToString().Split(':')[1],
                                Url = propImageItem["url"].ToString()
                            };
                            productInfo.PropImages.Add(propImage);
                        }
                    }

                    return productInfo;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    System.Diagnostics.Debug.WriteLine($"GetProductFromTaobaoAsync Error Text: '{ex.Message.ToString()}'");
                    if (retryCount >= maxRetryCount)
                    {
                        return null; // 재시도 횟수를 초과하면 null을 반환
                    }

                    // 잠시 대기 후 재시도
                    await Task.Delay(1000 * retryCount);
                }
            }

            return null; // 여기에 도달할 경우는 null을 반환
        }

        public class CategorizedCategory
        {
            public Nop.Core.Domain.Catalog.Category Parent { get; set; }
            public string ParentName { get; set; }
            public int SecondCategoryId { get; set; }  // 이 속성을 추가합니다.
            public List<CategoryWithTranslation> Children { get; set; } = new List<CategoryWithTranslation>();
        }

        public class CategoryWithTranslation
        {
            public Nop.Core.Domain.Catalog.Category CategoryData { get; set; }
            public string TranslatedName { get; set; }
        }

        public virtual async Task<IActionResult> ApiCreate(bool continueEditing = false)
        {
            // 현재 날짜를 yyyyMMdd 형식으로 가져옴.
            var today = DateTime.Now.ToString("yyyyMMdd");

            // 카테고리 작업 소요시간 시작
            var swCategories = Stopwatch.StartNew();
            swCategories.Start();

            #region Remove Category

            // 전체 카테고리 가져오기
            var categories = await _categoryService.GetAllCategoriesAsync();

            // "미분류 카테고리" 이름을 가진 카테고리를 찾기
            var unclassifiedCategoryName = "미분류 카테고리";
            var unclassifiedCategory = categories.FirstOrDefault(cat => cat.Name == unclassifiedCategoryName);

            // "미분류 카테고리"의 ID를 가져옴
            int unclassifiedCategoryId = unclassifiedCategory.Id;

            // 하위 카테고리를 가져오기
            var subCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(unclassifiedCategoryId);

            foreach (var subCategory in subCategories)
            {
                if (subCategory.Name != today)
                {
                    // 각 하위 카테고리에 속한 상품 가져오기
                    var productsInSubCategory = await _categoryService.GetProductCategoriesByCategoryIdAsync(subCategory.Id);

                    if (!productsInSubCategory.Any())
                    {
                        // 하위 카테고리에 속한 상품이 없는 경우, 해당 카테고리 삭제
                        await _categoryService.DeleteCategoryAsync(subCategory);
                    }
                }
   
            }

            #endregion

            #region Create or use category

            int categoryId;

            // 하위 카테고리에서 현재 날짜와 같은 이름의 카테고리를 찾기
            var todayCategory = subCategories.FirstOrDefault(cat => cat.Name == today);

            if (todayCategory == null)
            {
                // 해당 날짜의 카테고리가 존재하지 않는 경우, 새 카테고리를 생성

                // Category 설정
                var newCategory = new Category
                {
                    Name = today,
                    Description = today,
                    MetaKeywords = null,
                    MetaDescription = null,
                    MetaTitle = null,
                    PageSizeOptions = "50, 100, 150",
                    ParentCategoryId = unclassifiedCategory?.Id ?? 0, // "미분류 카테고리"의 ID를 사용하거나 기본값으로 0을 사용
                    CategoryTemplateId = 1,
                    PictureId = 0,
                    PageSize = 5,
                    AllowCustomersToSelectPageSize = true,
                    ShowOnHomepage = false,
                    IncludeInTopMenu = true,
                    Published = true,
                    Deleted = false,
                    DisplayOrder = 0,
                    PriceRangeFiltering = true,
                    PriceTo = 10000,
                    ManuallyPriceRange = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                };

                // 새로운 카테고리를 데이터베이스에 저장
                await _categoryService.InsertCategoryAsync(newCategory);

                // 새로 생성된 카테고리의 ID를 가져옴
                categoryId = newCategory.Id;
            }
            else
            {
                // 해당 날짜의 카테고리가 이미 존재하는 경우, 해당 카테고리의 ID를 가져옴
                categoryId = todayCategory.Id;
            }

            #endregion

            #region Search Category language settings

            var languageId = 1; //var languageId = 1; // 영어에 대한 ID
                                //var languageId = 3; // 중국어에 대한 ID

            var categorizedCategories = new List<CategorizedCategory>();

            foreach (var category in categories)
            {
                var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId);
                if (parentCategory == null || int.TryParse(parentCategory.Name, out _) || parentCategory.Name == "미분류 카테고리")
                {
                    continue;
                }

                var parentName = await _localizedEntityService.GetLocalizedValueAsync(languageId, parentCategory.Id, "Category", "Name");
                if (string.IsNullOrEmpty(parentName))
                {
                    parentName = await DeppLTranslateTextAsync(parentCategory.Name, "", LanguageCode.Chinese);
                }

                var childTranslatedName = await _localizedEntityService.GetLocalizedValueAsync(languageId, category.Id, "Category", "Name");
                if (string.IsNullOrEmpty(childTranslatedName))
                {
                    childTranslatedName = await DeppLTranslateTextAsync(category.Name, "", LanguageCode.Chinese);
                }

                var childNames = childTranslatedName.Split('/');

                var grandChildCategoryIds = await _categoryService.GetChildCategoryIdsAsync(category.Id);

                foreach (var grandChildCategoryId in grandChildCategoryIds)
                {
                    var grandChildCategory = await _categoryService.GetCategoryByIdAsync(grandChildCategoryId);
                    var grandChildTranslatedName = await _localizedEntityService.GetLocalizedValueAsync(languageId, grandChildCategory.Id, "Category", "Name");
                    if (string.IsNullOrEmpty(grandChildTranslatedName))
                    {
                        grandChildTranslatedName = await DeppLTranslateTextAsync(grandChildCategory.Name, "", LanguageCode.Chinese);
                    }
                    var grandChildNames = grandChildTranslatedName.Split('/');

                    foreach (var childName in childNames)
                    {
                        foreach (var grandChildName in grandChildNames)
                        {
                            // 2차와 3차의 이름이 같으면 3차만 사용
                            var combinedName = childName == grandChildName ? grandChildName : $"{childName} {grandChildName}";

                            var categorizedCategory = new CategorizedCategory
                            {
                                Parent = parentCategory,
                                ParentName = childTranslatedName,
                                SecondCategoryId = category.Id,
                                Children = new List<CategoryWithTranslation>
                                {
                                    new CategoryWithTranslation
                                    {
                                        CategoryData = grandChildCategory,
                                        TranslatedName = combinedName
                                    }
                                }
                            };

                            categorizedCategories.Add(categorizedCategory);
                        }
                    }
                }
            }

            if (categorizedCategories.Count < 1)
            {
                foreach (var category in categories)
                {
                    if (category.ParentCategoryId == 0)
                    {
                        continue;
                    }

                    var localizedName = await _localizedEntityService.GetLocalizedValueAsync(languageId, category.Id, "Category", "Name");

                    var categorizedCategory = new CategorizedCategory
                    {
                        ParentName = localizedName,
                        SecondCategoryId = category.Id,
                        Children = new List<CategoryWithTranslation>
                                {
                                    new CategoryWithTranslation
                                    {
                                        CategoryData = category,
                                        TranslatedName = localizedName
                                    }
                                }
                    };

                    categorizedCategories.Add(categorizedCategory);
                }
            }

            #endregion


            // 카테고리 작업 소요시간 종료
            swCategories.Stop();
            var ts = swCategories.Elapsed;
            var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            System.Diagnostics.Debug.WriteLine($"swCategories : '{elapsedTime}'");

            var randomizedCategories = new List<CategorizedCategory>();
            var random = new Random();

            while (categorizedCategories.Count > 0)
            {
                var index = random.Next(categorizedCategories.Count); // 랜덤한 인덱스 선택
                var category = categorizedCategories[index];
                randomizedCategories.Add(category);
                categorizedCategories.RemoveAt(index);
            }

            foreach (var categorizedCategory in randomizedCategories)
            {
                var parentName = categorizedCategory.ParentName;
                foreach (var childCategory in categorizedCategory.Children)
                {
                    var searchQuery = childCategory.TranslatedName.Contains(parentName)
                                      ? childCategory.TranslatedName
                                      : $"{parentName} {childCategory.TranslatedName}";
                    System.Diagnostics.Debug.WriteLine($"GetcategoryNameProductsFromTaobaoAsync : '{searchQuery}'");

                    // 2023.10.18 상품정보 JD사이트로 변경
                    // var nameProductInfoList = await GetcategoryNameProductsFromTaobaoAsync(searchQuery, size: 1);

                    var nameProductInfoList = await GetProductsFromJdAsync(searchQuery, size: 5);

                    if (nameProductInfoList == null)
                    {
                        continue;
                    }

                    foreach (var productInfo in nameProductInfoList)
                    {
                        var productId = productInfo.Id;
                        var productUrl = productInfo.Url;
                        var productBySku = await _productService.GetProductBySkuAsync(productUrl);
                        if (productBySku != null)
                        {
                            // 동일 SKU일경우 저장하지않는다.
                            System.Diagnostics.Debug.WriteLine($"Skip the product : '{productUrl}'");
                            continue;
                        }

                        var productTitle = productInfo.Title;
                        var productOriginalTitle = productInfo.OriginalTitle;
                        var translatedValue = await DeppLTranslateTextAsync(productOriginalTitle, LanguageCode.Chinese, LanguageCode.Korean);

                        // var productData = await GetProductFromTaobaoAsync(productId);
                        var productData = await GetProductFromJdAsync(productId);
                        if (productData == null)
                        {
                            // 상품정보가 API에 없으면 건너뛴다
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine($"categorizedCategory : '{childCategory.CategoryData.Name}'");
                        var model = new ProductModel
                        {
                            Id = 0,
                            Name = string.IsNullOrEmpty(translatedValue) ? "ProductModel Name 값을 불러오지못했습니다." : translatedValue,
                            Price = productData.Price,
                            FullDescription = productData.Description?.Html != null
                                                      ? $"<p>{productData.Description.Html}</p>"
                                                      : "<p>FullDescription 값을 불러오지못했습니다.</p>",
                            Sku = string.IsNullOrEmpty(productUrl) ? "Sku 값을 불러오지못했습니다." : productUrl,
                            AvailableStartDateTimeUtc = DateTime.UtcNow,
                            Published = true,
                            ProductTypeId = 5,
                            BasepriceUnitId = 3,
                            BasepriceBaseUnitId = 3,
                            VisibleIndividually = true,
                            SelectedCategoryIds = new List<int>
                            {
                                categoryId
                            },
                            IsTaxExempt = false,
                            NotifyAdminForQuantityBelow = 1,
                            BackorderModeId = 0,
                            AllowBackInStockSubscriptions = false,
                            OrderMinimumQuantity = 1,
                            OrderMaximumQuantity = 99999,
                            ProductTemplateId = 1,
                            Locales = new List<ProductLocalizedModel>
                            {
                                new ProductLocalizedModel
                                {
                                    // en
                                    LanguageId = 1,
                                    Name = string.IsNullOrEmpty(productTitle)
                                           ? "ProductLocalizedModel Name 값을 불러오지못했습니다."
                                           : productTitle
                                },
                                new ProductLocalizedModel
                                {
                                    // ko
                                    LanguageId = 2,
                                    Name = string.IsNullOrEmpty(translatedValue)
                                           ? "ProductLocalizedModel Name 값을 불러오지못했습니다."
                                           : translatedValue
                                },
                                new ProductLocalizedModel
                                {
                                    // cn
                                    LanguageId = 3,
                                    Name = string.IsNullOrEmpty(productOriginalTitle)
                                           ? "ProductLocalizedModel Name 값을 불러오지못했습니다."
                                           : productOriginalTitle
                                }
                            }
                        };

                        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                            return AccessDeniedView();

                        var currentVendor = await _workContext.GetCurrentVendorAsync();
                        if (_vendorSettings.MaximumProductNumber > 0 && currentVendor != null
                            && await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
                        {
                            _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                                _vendorSettings.MaximumProductNumber));
                            return RedirectToAction("List");
                        }

                        if (ModelState.IsValid)
                        {
                            if (currentVendor != null)
                            {
                                model.VendorId = currentVendor.Id;
                            }

                            if (currentVendor != null && model.ShowOnHomepage)
                            {
                                model.ShowOnHomepage = false;
                            }

                            var product = model.ToEntity<Product>();
                            product.CreatedOnUtc = DateTime.UtcNow;
                            product.UpdatedOnUtc = DateTime.UtcNow;

                            var imageUrlList = productData.ItemImgs.Select(i => i.Url).ToList();
                            var imageBase64List = await ConvertImagesToBase64Async(imageUrlList);

                            if (imageBase64List.Count == 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"No images found for product. Skipping the product insertion.");
                                continue;
                            }

                            await _productService.InsertProductAsync(product);

                            for (var i = 0; i < imageBase64List.Count; i++)
                            {
                                var imageData = imageBase64List[i];
                                var pictureBinary = Convert.FromBase64String(imageData.Base64String);
                                var mimeType = await GetMimeTypeFromUrl(imageData.Url);
                                var seoFilename = product.Name + System.IO.Path.GetExtension(imageData.Url);
                                var picture = await _pictureService.InsertPictureAsync(pictureBinary, mimeType, seoFilename);
                                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));
                                await _productService.InsertProductPictureAsync(new ProductPicture
                                {
                                    PictureId = picture.Id,
                                    ProductId = product.Id,
                                    DisplayOrder = i
                                });
                            }

                            // 상품 특성 매핑 메서드 호출
                            // await MapTaobaoProductAttributesAsync(product.Id, productData);

                            model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                            await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                            await UpdateLocalesAsync(product, model, true);
                            await SaveCategoryMappingsAsync(product, model);
                            await SaveManufacturerMappingsAsync(product, model);
                            await SaveProductAclAsync(product, model);
                            await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);
                            await SaveDiscountMappingsAsync(product, model);
                            await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));
                            await SaveProductWarehouseInventoryAsync(product, model);
                            await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                                await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
                            await _customerActivityService.InsertActivityAsync("AddNewProduct",
                                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);
                            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));
                        }
                        else
                        {
                            model = await _productModelFactory.PrepareProductModelAsync(model, null, true);
                            return View(model);
                        }
                    }
                }
            }
            return RedirectToAction("List");
        }

        private async Task ApiEdit(ProductModel model)
        {
            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.Id);

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (currentVendor != null)
                    model.VendorId = currentVendor.Id;

                //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
                //vendors cannot edit "Show on home page" property
                if (currentVendor != null && model.ShowOnHomepage != product.ShowOnHomepage)
                    model.ShowOnHomepage = product.ShowOnHomepage;

                //some previously used values
                var prevTotalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);
                var prevDownloadId = product.DownloadId;
                var prevSampleDownloadId = product.SampleDownloadId;
                var previousStockQuantity = product.StockQuantity;
                var previousWarehouseId = product.WarehouseId;
                var previousProductType = product.ProductType;

                //product
                product = model.ToEntity(product);

                product.UpdatedOnUtc = DateTime.UtcNow;
                await _productService.UpdateProductAsync(product);

                //remove associated products
                if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var storeId = store?.Id ?? 0;
                    var vendorId = currentVendor?.Id ?? 0;

                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, storeId, vendorId);
                    foreach (var associatedProduct in associatedProducts)
                    {
                        associatedProduct.ParentGroupedProductId = 0;
                        await _productService.UpdateProductAsync(associatedProduct);
                    }
                }

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(product, model);

                //tags
                await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));

                //warehouses
                await SaveProductWarehouseInventoryAsync(product, model);

                //categories
                await SaveCategoryMappingsAsync(product, model);

                //manufacturers
                await SaveManufacturerMappingsAsync(product, model);

                //ACL (customer roles)
                await SaveProductAclAsync(product, model);

                //stores
                await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);

                //discounts
                await SaveDiscountMappingsAsync(product, model);

                //picture seo names
                await UpdatePictureSeoNamesAsync(product);

                //back in stock notifications
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.BackorderMode == BackorderMode.NoBackorders &&
                    product.AllowBackInStockSubscriptions &&
                    await _productService.GetTotalStockQuantityAsync(product) > 0 &&
                    prevTotalStockQuantity <= 0 &&
                    product.Published &&
                    !product.Deleted)
                {
                    await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);
                }

                //delete an old "download" file (if deleted or updated)
                if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
                {
                    var prevDownload = await _downloadService.GetDownloadByIdAsync(prevDownloadId);
                    if (prevDownload != null)
                        await _downloadService.DeleteDownloadAsync(prevDownload);
                }

                //delete an old "sample download" file (if deleted or updated)
                if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
                {
                    var prevSampleDownload = await _downloadService.GetDownloadByIdAsync(prevSampleDownloadId);
                    if (prevSampleDownload != null)
                        await _downloadService.DeleteDownloadAsync(prevSampleDownload);
                }

                //quantity change history
                if (previousWarehouseId != product.WarehouseId)
                {
                    //warehouse is changed 
                    //compose a message
                    var oldWarehouseMessage = string.Empty;
                    if (previousWarehouseId > 0)
                    {
                        var oldWarehouse = await _shippingService.GetWarehouseByIdAsync(previousWarehouseId);
                        if (oldWarehouse != null)
                            oldWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                    }

                    var newWarehouseMessage = string.Empty;
                    if (product.WarehouseId > 0)
                    {
                        var newWarehouse = await _shippingService.GetWarehouseByIdAsync(product.WarehouseId);
                        if (newWarehouse != null)
                            newWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                    }

                    var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                    //record history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
                }
                else
                {
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                        product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("EditProduct",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));

            }

            //prepare model
            model = await _productModelFactory.PrepareProductModelAsync(model, product, true);
        }

        public async Task MapTaobaoProductAttributesAsync(int productId, TaobaoProductInfo taobaoProduct)
        {
            // 타오바오 상품의 특성 정보 파악
            var propsFromTaobao = taobaoProduct.Props;

            foreach (var prop in propsFromTaobao)
            {
                // 해당 특성이 nopCommerce 데이터베이스에 이미 번역된 이름으로 존재하는지 확인하고, 존재한다면 해당 특성의 ID를 가져옴
                int? existingAttributeId = await _localizedEntityService.GetEntityIdByLocalizedNameAsync("ProductAttribute", "Name", prop.PropertyName);

                ProductAttribute existingAttribute = null;

                if (!existingAttributeId.HasValue)
                {
                    // 데이터베이스에 존재하지 않는 경우, 새로운 특성 추가
                    var newAttribute = new ProductAttribute
                    {
                        Name = prop.PropertyName,
                        Description = prop.OriginalPropertyName // 원래의 속성 이름을 설명으로 사용
                    };

                    // Assuming you have a method to convert the newAttribute to a model
                    var model = ConvertToProductAttributeModel(newAttribute);

                    await _productAttributeService.InsertProductAttributeAsync(newAttribute);

                    // Assuming you have all necessary parameters for UpdateLocalesAsync
                    await UpdateLocalesAsync(newAttribute, model);

                    //activity log
                    await _customerActivityService.InsertActivityAsync("AddNewProductAttribute",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProductAttribute"), newAttribute.Name), newAttribute);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Attributes.ProductAttributes.Added"));
                    existingAttribute = newAttribute;
                }
                else
                {
                    // 만약 존재한다면, 해당 특성을 검색하여 existingAttribute를 설정
                    existingAttribute = await _productAttributeService.GetProductAttributeByIdAsync(existingAttributeId.Value);
                }

                // 해당 특성이 이미 상품에 매핑되어 있는지 확인
                if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productId))
                    .Any(x => x.ProductAttributeId == existingAttribute.Id))
                {
                    continue; // 특성이 이미 매핑되어 있다면 다음으로 넘어감
                }

                // 상품을 특성에 매핑
                var productAttributeMapping = new ProductAttributeMapping
                {
                    ProductId = productId,
                    ProductAttributeId = existingAttribute.Id
                };

                await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMapping);
            }
        }

        public ProductAttributeModel ConvertToProductAttributeModel(ProductAttribute productAttribute)
        {
            if (productAttribute == null)
                throw new ArgumentNullException(nameof(productAttribute));

            var model = new ProductAttributeModel
            {
                Id = productAttribute.Id,
                Name = productAttribute.Name,
                Description = productAttribute.Description,
                Locales = new List<ProductAttributeLocalizedModel>() // 추가적인 로컬라이제이션 정보가 필요한 경우 여기에 추가
            };

            return model;
        }


        [HttpPost, ParameterBasedOnFormName("save-api", "callApi")]
        public virtual async Task<IActionResult> ApiCreate(string query, bool continueEditing = false)
        {
            // Call the method to get the product data from Taobao API
            var productInfoList = await GetProductsFromTaobaoAsync(query);

            foreach (var productInfo in productInfoList)
            {
                var productId = productInfo.Id;
                var productUrl = productInfo.Url;
                var productTitle = productInfo.Title;
                var productData = await GetProductFromTaobaoAsync(productId);
                // Fill in the ProductModel with the data from Taobao API
                var model = new ProductModel
                {
                    Id = 0,
                    Name = productData.Title,
                    Price = productData.Price,
                    FullDescription = $"<p>{productData.Description.Html}</p>",
                    Sku = productUrl,
                    AvailableStartDateTimeUtc = DateTime.UtcNow,
                    Published = true,
                    VisibleIndividually = true,
                    SelectedCategoryIds = new List<int> { 14091 },
                    IsTaxExempt = false,
                    NotifyAdminForQuantityBelow = 1,
                    BackorderModeId = 0,
                    AllowBackInStockSubscriptions = false,
                    OrderMinimumQuantity = 1,

                };

                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                    return AccessDeniedView();

                //validate maximum number of products per vendor
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                if (_vendorSettings.MaximumProductNumber > 0 && currentVendor != null
                    && await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
                {
                    _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                        _vendorSettings.MaximumProductNumber));
                    return RedirectToAction("List");
                }

                if (ModelState.IsValid)
                {
                    //a vendor should have access only to his products
                    if (currentVendor != null)
                    {
                        model.VendorId = currentVendor.Id;
                    }

                    //vendors cannot edit "Show on home page" property
                    if (currentVendor != null && model.ShowOnHomepage)
                    {
                        model.ShowOnHomepage = false;
                    }

                    //product
                    var product = model.ToEntity<Product>();
                    product.CreatedOnUtc = DateTime.UtcNow;
                    product.UpdatedOnUtc = DateTime.UtcNow;
                    await _productService.InsertProductAsync(product);

                    // productData에서 이미지 URL 목록을 가져옵니다.
                    var imageUrlList = productData.ItemImgs.Select(i => i.Url).ToList();

                    // 이미지 URL들을 Base64로 변환합니다.
                    var imageBase64List = await ConvertImagesToBase64Async(imageUrlList);

                    // 각 이미지를 처리합니다.
                    for (int i = 0; i < imageBase64List.Count; i++)
                    {
                        var imageData = imageBase64List[i];

                        // Base64 문자열을 바이트 배열로 변환합니다.
                        var pictureBinary = Convert.FromBase64String(imageData.Base64String);

                        // 이미지 URL에서 MIME 타입을 결정합니다.
                        var mimeType = await GetMimeTypeFromUrl(imageData.Url);
                        var seoFilename = product.Name + System.IO.Path.GetExtension(imageData.Url);

                        // 사진을 삽입합니다.
                        var picture = await _pictureService.InsertPictureAsync(pictureBinary, mimeType, seoFilename);

                        // 사진의 SEO 파일명을 설정합니다.
                        await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));

                        // 제품과 사진을 연결합니다.
                        await _productService.InsertProductPictureAsync(new ProductPicture
                        {
                            PictureId = picture.Id,
                            ProductId = product.Id,
                            DisplayOrder = i
                        });
                    }

                    //search engine name
                    model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                    await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                    //locales
                    await UpdateLocalesAsync(product, model , true);

                    //categories
                    await SaveCategoryMappingsAsync(product, model);

                    //manufacturers
                    await SaveManufacturerMappingsAsync(product, model);

                    //ACL (customer roles)
                    await SaveProductAclAsync(product, model);

                    //stores
                    await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);

                    //discounts
                    await SaveDiscountMappingsAsync(product, model);

                    //tags
                    await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));

                    //warehouses
                    await SaveProductWarehouseInventoryAsync(product, model);

                    //quantity change history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                        await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

                    //activity log
                    await _customerActivityService.InsertActivityAsync("AddNewProduct",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));
                }
                else
                {
                    //prepare model
                    model = await _productModelFactory.PrepareProductModelAsync(model, null, true);

                    //if we got this far, something failed, redisplay form
                    return View(model);
                }
            }

            return RedirectToAction("List");
        }

        #endregion

        #region Product list / create / edit / delete

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductList(ProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productModelFactory.PrepareProductListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public virtual async Task<IActionResult> GoToSku(ProductSearchModel searchModel)
        {
            //try to load a product entity, if not found, then try to load a product attribute combination
            var productId = (await _productService.GetProductBySkuAsync(searchModel.GoDirectlyToSku))?.Id
                ?? (await _productAttributeService.GetProductAttributeCombinationBySkuAsync(searchModel.GoDirectlyToSku))?.ProductId;

            if (productId != null)
                return RedirectToAction("Edit", "Product", new { id = productId });

            //not found
            return await List();
        }

        public virtual async Task<IActionResult> Create(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (_vendorSettings.MaximumProductNumber > 0 && currentVendor != null
                && await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            //prepare model
            var model = await _productModelFactory.PrepareProductModelAsync(new ProductModel(), null);

            //show configuration tour
            if (showtour)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.HideConfigurationStepsAttribute);
                var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.CloseConfigurationStepsAttribute);

                if (!hideCard && !closeCard)
                    ViewBag.ShowTour = true;
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProductModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (_vendorSettings.MaximumProductNumber > 0 && currentVendor != null
                && await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (currentVendor != null)
                    model.VendorId = currentVendor.Id;

                //vendors cannot edit "Show on home page" property
                if (currentVendor != null && model.ShowOnHomepage)
                    model.ShowOnHomepage = false;

                // 1. 언어 감지
                var detectedLanguage = await DetectLanguageAsync(model.Name);  // 여기서 실제 언어 감지 함수를 호출
                detectedLanguage = NormalizeLanguageCode(detectedLanguage);

                // 2. 감지된 언어가 중국어 또는 영어인 경우
                if (detectedLanguage == "zh" || detectedLanguage == "en")
                {
                    // 원래 Name 값을 Locales에 저장하기 전에 일치하는 LanguageId가 있는 항목이 있는지 확인
                    int targetLanguageId = ConvertDetectedLanguageToLanguageId(detectedLanguage);
                    var existingItem = model.Locales.FirstOrDefault(p => p.LanguageId == targetLanguageId);

                    // 일치하는 LanguageId가 있는 항목이 있다면 제거
                    if (existingItem != null)
                    {
                        model.Locales.Remove(existingItem);
                    }

                    // 원래 Name 값을 Locales에 저장
                    model.Locales.Add(new ProductLocalizedModel
                    {
                        LanguageId = targetLanguageId,
                        Name = model.Name
                    });

                    // Name을 한국어로 번역
                    var translatedName = await DeppLTranslateTextAsync(model.Name, detectedLanguage, "ko");
                    model.Name = translatedName;
                }

                //product
                var product = model.ToEntity<Product>();
                product.CreatedOnUtc = DateTime.UtcNow;
                product.UpdatedOnUtc = DateTime.UtcNow;
                await _productService.InsertProductAsync(product);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(product, model);

                //categories
                await SaveCategoryMappingsAsync(product, model);

                //manufacturers
                await SaveManufacturerMappingsAsync(product, model);

                //ACL (customer roles)
                await SaveProductAclAsync(product, model);

                //stores
                await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);

                //discounts
                await SaveDiscountMappingsAsync(product, model);

                //tags
                await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));

                //warehouses
                await SaveProductWarehouseInventoryAsync(product, model);

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                    await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewProduct",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = await _productModelFactory.PrepareProductModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        private static string NormalizeLanguageCode(string detectedLanguage)
        {
            if (detectedLanguage.StartsWith("zh-"))
                return "zh";

            return detectedLanguage;
        }

        private static int ConvertDetectedLanguageToLanguageId(string detectedLanguage)
        {
            switch (detectedLanguage)
            {
                case "en":
                case "en-US":
                    return 1; // 영어
                case "ko":
                    return 2; // 한국어
                case "zh":
                case "zh-CN":
                    return 3; // 중국어
                default:
                    // 적절한 기본값을 반환하거나 예외를 던집니다.
                    throw new ArgumentException($"Unknown language: {detectedLanguage}");
            }
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");

            //prepare model
            var model = await _productModelFactory.PrepareProductModelAsync(null, product);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProductModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.Id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");

            //check if the product quantity has been changed while we were editing the product
            //and if it has been changed then we show error notification
            //and redirect on the editing page without data saving
            if (product.StockQuantity != model.LastStockQuantity)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
                return RedirectToAction("Edit", new { id = product.Id });
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (currentVendor != null)
                    model.VendorId = currentVendor.Id;

                //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
                //vendors cannot edit "Show on home page" property
                if (currentVendor != null && model.ShowOnHomepage != product.ShowOnHomepage)
                    model.ShowOnHomepage = product.ShowOnHomepage;

                //some previously used values
                var prevTotalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);
                var prevDownloadId = product.DownloadId;
                var prevSampleDownloadId = product.SampleDownloadId;
                var previousStockQuantity = product.StockQuantity;
                var previousWarehouseId = product.WarehouseId;
                var previousProductType = product.ProductType;

                //product
                product = model.ToEntity(product);

                product.UpdatedOnUtc = DateTime.UtcNow;
                await _productService.UpdateProductAsync(product);

                //remove associated products
                if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var storeId = store?.Id ?? 0;
                    var vendorId = currentVendor?.Id ?? 0;

                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, storeId, vendorId);
                    foreach (var associatedProduct in associatedProducts)
                    {
                        associatedProduct.ParentGroupedProductId = 0;
                        await _productService.UpdateProductAsync(associatedProduct);
                    }
                }

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                // 1. 언어 감지
                var detectedLanguage = await DetectLanguageAsync(model.Name);  // 여기서 실제 언어 감지 함수를 호출
                detectedLanguage = NormalizeLanguageCode(detectedLanguage);

                // 2. 감지된 언어가 중국어 또는 영어인 경우
                if (detectedLanguage == "zh" || detectedLanguage == "en")
                {
                    // 원래 Name 값을 Locales에 저장하기 전에 일치하는 LanguageId가 있는 항목이 있는지 확인
                    int targetLanguageId = ConvertDetectedLanguageToLanguageId(detectedLanguage);
                    var existingItem = model.Locales.FirstOrDefault(p => p.LanguageId == targetLanguageId);

                    // 일치하는 LanguageId가 있는 항목이 있다면 제거
                    if (existingItem != null)
                    {
                        model.Locales.Remove(existingItem);
                    }

                    // 원래 Name 값을 Locales에 저장
                    model.Locales.Add(new ProductLocalizedModel
                    {
                        LanguageId = targetLanguageId,
                        Name = model.Name
                    });

                    // Name을 한국어로 번역
                    var translatedName = await DeppLTranslateTextAsync(model.Name, detectedLanguage, "ko");
                    model.Name = translatedName;
                }

                //locales
                await UpdateLocalesAsync(product, model);

                //tags
                await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));

                //warehouses
                await SaveProductWarehouseInventoryAsync(product, model);

                //categories
                await SaveCategoryMappingsAsync(product, model);

                //manufacturers
                await SaveManufacturerMappingsAsync(product, model);

                //ACL (customer roles)
                await SaveProductAclAsync(product, model);

                //stores
                await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);

                //discounts
                await SaveDiscountMappingsAsync(product, model);

                //picture seo names
                await UpdatePictureSeoNamesAsync(product);

                //back in stock notifications
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.BackorderMode == BackorderMode.NoBackorders &&
                    product.AllowBackInStockSubscriptions &&
                    await _productService.GetTotalStockQuantityAsync(product) > 0 &&
                    prevTotalStockQuantity <= 0 &&
                    product.Published &&
                    !product.Deleted)
                {
                    await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);
                }

                //delete an old "download" file (if deleted or updated)
                if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
                {
                    var prevDownload = await _downloadService.GetDownloadByIdAsync(prevDownloadId);
                    if (prevDownload != null)
                        await _downloadService.DeleteDownloadAsync(prevDownload);
                }

                //delete an old "sample download" file (if deleted or updated)
                if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
                {
                    var prevSampleDownload = await _downloadService.GetDownloadByIdAsync(prevSampleDownloadId);
                    if (prevSampleDownload != null)
                        await _downloadService.DeleteDownloadAsync(prevSampleDownload);
                }

                //quantity change history
                if (previousWarehouseId != product.WarehouseId)
                {
                    //warehouse is changed 
                    //compose a message
                    var oldWarehouseMessage = string.Empty;
                    if (previousWarehouseId > 0)
                    {
                        var oldWarehouse = await _shippingService.GetWarehouseByIdAsync(previousWarehouseId);
                        if (oldWarehouse != null)
                            oldWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                    }

                    var newWarehouseMessage = string.Empty;
                    if (product.WarehouseId > 0)
                    {
                        var newWarehouse = await _shippingService.GetWarehouseByIdAsync(product.WarehouseId);
                        if (newWarehouse != null)
                            newWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                    }

                    var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                    //record history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
                }
                else
                {
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                        product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("EditProduct",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = await _productModelFactory.PrepareProductModelAsync(model, product, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductName(int productId, string newName)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null)
            {
                product.Name = newName;
                await _productService.UpdateProductAsync(product);
            }
            else
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");

            await _productService.DeleteProductAsync(product);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProduct"), product.Name), product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            await _productService.DeleteProductsAsync((await _productService.GetProductsByIdsAsync(selectedIds.ToArray()))
                .Where(p => currentVendor == null || p.VendorId == currentVendor.Id).ToList());

            return Json(new { Result = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();

            // 최상위 카테고리 필터링
            var rootCategories = categories.Where(c => c.ParentCategoryId == 0).ToList();

            // 중첩된 구조로 변환
            var structuredData = rootCategories.Select(r => BuildNestedCategory(r, categories.ToList())).ToList();

            return Json(structuredData);
        }

        private object BuildNestedCategory(Category category, List<Category> allCategories)
        {
            var children = allCategories.Where(c => c.ParentCategoryId == category.Id).ToList();

            return new
            {
                id = category.Id,
                parent = category.ParentCategoryId == 0 ? "#" : category.ParentCategoryId.ToString(),
                text = category.Name,
                children = children.Select(c => BuildNestedCategory(c, allCategories)).ToList()
            };
        }

        [HttpPost]
        public virtual async Task<IActionResult> CopyProduct(ProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = await _productService.GetProductByIdAsync(copyModel.Id);

                //a vendor should have access only to his products
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                if (currentVendor != null && originalProduct.VendorId != currentVendor.Id)
                    return RedirectToAction("List");

                var newProduct = await _copyProductService.CopyProductAsync(originalProduct, copyModel.Name, copyModel.Published, copyModel.CopyMultimedia);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copied"));

                return RedirectToAction("Edit", new { id = newProduct.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        //action displaying notification (warning) to a store owner that entered SKU already exists
        public virtual async Task<IActionResult> SkuReservedWarning(int productId, string sku)
        {
            string message;

            //check whether product with passed SKU already exists
            var productBySku = await _productService.GetProductBySkuAsync(sku);
            if (productBySku != null)
            {
                if (productBySku.Id == productId)
                    return Json(new { Result = string.Empty });

                message = string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Sku.Reserved"), productBySku.Name);
                return Json(new { Result = message });
            }

            //check whether combination with passed SKU already exists
            var combinationBySku = await _productAttributeService.GetProductAttributeCombinationBySkuAsync(sku);
            if (combinationBySku == null)
                return Json(new { Result = string.Empty });

            message = string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Fields.Sku.Reserved"),
                (await _productService.GetProductByIdAsync(combinationBySku.ProductId))?.Name);

            return Json(new { Result = message });
        }

        #endregion

        #region Required products

        [HttpPost]
        public virtual async Task<IActionResult> LoadProductFriendlyNames(string productIds)
        {
            var result = string.Empty;

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return Json(new { Text = result });

            if (string.IsNullOrWhiteSpace(productIds))
                return Json(new { Text = result });

            var ids = new List<int>();
            var rangeArray = productIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            foreach (var str1 in rangeArray)
            {
                if (int.TryParse(str1, out var tmp1))
                    ids.Add(tmp1);
            }

            var products = await _productService.GetProductsByIdsAsync(ids.ToArray());
            for (var i = 0; i <= products.Count - 1; i++)
            {
                result += products[i].Name;
                if (i != products.Count - 1)
                    result += ", ";
            }

            return Json(new { Text = result });
        }

        public virtual async Task<IActionResult> RequiredProductAddPopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareAddRequiredProductSearchModelAsync(new AddRequiredProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> RequiredProductAddPopupList(AddRequiredProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productModelFactory.PrepareAddRequiredProductListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Related products

        [HttpPost]
        public virtual async Task<IActionResult> RelatedProductList(RelatedProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareRelatedProductListModelAsync(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> RelatedProductUpdate(RelatedProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a related product with the specified id
            var relatedProduct = await _productService.GetRelatedProductByIdAsync(model.Id)
                ?? throw new ArgumentException("No related product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(relatedProduct.ProductId1);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            relatedProduct.DisplayOrder = model.DisplayOrder;
            await _productService.UpdateRelatedProductAsync(relatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> RelatedProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a related product with the specified id
            var relatedProduct = await _productService.GetRelatedProductByIdAsync(id)
                ?? throw new ArgumentException("No related product found with the specified id");

            var productId = relatedProduct.ProductId1;

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            await _productService.DeleteRelatedProductAsync(relatedProduct);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> RelatedProductAddPopup(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareAddRelatedProductSearchModelAsync(new AddRelatedProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> RelatedProductAddPopupList(AddRelatedProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productModelFactory.PrepareAddRelatedProductListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> RelatedProductAddPopup(AddRelatedProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingRelatedProducts = await _productService.GetRelatedProductsByProductId1Async(model.ProductId, showHidden: true);
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                foreach (var product in selectedProducts)
                {
                    //a vendor should have access only to his products
                    if (currentVendor != null && product.VendorId != currentVendor.Id)
                        continue;

                    if (_productService.FindRelatedProduct(existingRelatedProducts, model.ProductId, product.Id) != null)
                        continue;

                    await _productService.InsertRelatedProductAsync(new RelatedProduct
                    {
                        ProductId1 = model.ProductId,
                        ProductId2 = product.Id,
                        DisplayOrder = 1
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddRelatedProductSearchModel());
        }

        #endregion

        #region Cross-sell products

        [HttpPost]
        public virtual async Task<IActionResult> CrossSellProductList(CrossSellProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareCrossSellProductListModelAsync(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CrossSellProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a cross-sell product with the specified id
            var crossSellProduct = await _productService.GetCrossSellProductByIdAsync(id)
                ?? throw new ArgumentException("No cross-sell product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(crossSellProduct.ProductId1);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            await _productService.DeleteCrossSellProductAsync(crossSellProduct);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> CrossSellProductAddPopup(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareAddCrossSellProductSearchModelAsync(new AddCrossSellProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CrossSellProductAddPopupList(AddCrossSellProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productModelFactory.PrepareAddCrossSellProductListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> CrossSellProductAddPopup(AddCrossSellProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingCrossSellProducts = await _productService.GetCrossSellProductsByProductId1Async(model.ProductId, showHidden: true);
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                foreach (var product in selectedProducts)
                {
                    //a vendor should have access only to his products
                    if (currentVendor != null && product.VendorId != currentVendor.Id)
                        continue;

                    if (_productService.FindCrossSellProduct(existingCrossSellProducts, model.ProductId, product.Id) != null)
                        continue;

                    await _productService.InsertCrossSellProductAsync(new CrossSellProduct
                    {
                        ProductId1 = model.ProductId,
                        ProductId2 = product.Id
                    });
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AddCrossSellProductSearchModel());
        }

        #endregion

        #region Associated products

        [HttpPost]
        public virtual async Task<IActionResult> AssociatedProductList(AssociatedProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareAssociatedProductListModelAsync(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AssociatedProductUpdate(AssociatedProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get an associated product with the specified id
            var associatedProduct = await _productService.GetProductByIdAsync(model.Id)
                ?? throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && associatedProduct.VendorId != currentVendor.Id)
                return Content("This is not your product");

            associatedProduct.DisplayOrder = model.DisplayOrder;
            await _productService.UpdateProductAsync(associatedProduct);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> AssociatedProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get an associated product with the specified id
            var product = await _productService.GetProductByIdAsync(id)
                ?? throw new ArgumentException("No associated product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            product.ParentGroupedProductId = 0;
            await _productService.UpdateProductAsync(product);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> AssociatedProductAddPopup(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AssociatedProductAddPopupList(AddAssociatedProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productModelFactory.PrepareAddAssociatedProductListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> AssociatedProductAddPopup(AddAssociatedProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());

            var tryToAddSelfGroupedProduct = selectedProducts
                .Select(p => p.Id)
                .Contains(model.ProductId);

            if (selectedProducts.Any())
            {
                foreach (var product in selectedProducts)
                {
                    if (product.Id == model.ProductId)
                        continue;

                    //a vendor should have access only to his products
                    var currentVendor = await _workContext.GetCurrentVendorAsync();
                    if (currentVendor != null && product.VendorId != currentVendor.Id)
                        continue;

                    product.ParentGroupedProductId = model.ProductId;
                    await _productService.UpdateProductAsync(product);
                }
            }

            if (tryToAddSelfGroupedProduct)
            {
                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.AssociatedProducts.TryToAddSelfGroupedProduct"));

                var addAssociatedProductSearchModel = await _productModelFactory.PrepareAddAssociatedProductSearchModelAsync(new AddAssociatedProductSearchModel());
                //set current product id
                addAssociatedProductSearchModel.ProductId = model.ProductId;

                ViewBag.RefreshPage = true;

                return View(addAssociatedProductSearchModel);
            }

            ViewBag.RefreshPage = true;

            ViewBag.ClosePage = true;

            return View(new AddAssociatedProductSearchModel());
        }

        #endregion

        #region Product pictures

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> ProductPictureAdd(int productId, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (productId == 0)
                throw new ArgumentException();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            var files = form.Files.ToList();
            if (!files.Any())
                return Json(new { success = false });

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");
            try
            {
                foreach (var file in files)
                {
                    //insert picture
                    var picture = await _pictureService.InsertPictureAsync(file);

                    await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));

                    await _productService.InsertProductPictureAsync(new ProductPicture
                    {
                        PictureId = picture.Id,
                        ProductId = product.Id,
                        DisplayOrder = 0
                    });
                }
            }
            catch (Exception exc)
            {
                return Json(new
                {
                    success = false,
                    message = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Pictures.Alert.PictureAdd")} {exc.Message}",
                });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductPictureList(ProductPictureSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareProductPictureListModelAsync(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductPictureUpdate(ProductPictureModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productPicture = await _productService.GetProductPictureByIdAsync(model.Id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.UpdatePictureAsync(picture.Id,
                await _pictureService.LoadPictureBinaryAsync(picture),
                picture.MimeType,
                picture.SeoFilename,
                model.OverrideAltAttribute,
                model.OverrideTitleAttribute);

            productPicture.DisplayOrder = model.DisplayOrder;
            await _productService.UpdateProductPictureAsync(productPicture);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductPictureDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productPicture = await _productService.GetProductPictureByIdAsync(id)
                ?? throw new ArgumentException("No product picture found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(productPicture.ProductId);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            var pictureId = productPicture.PictureId;
            await _productService.DeleteProductPictureAsync(productPicture);

            //try to get a picture with the specified id
            var picture = await _pictureService.GetPictureByIdAsync(pictureId)
                ?? throw new ArgumentException("No picture found with the specified id");

            await _pictureService.DeletePictureAsync(picture);

            return new NullJsonResult();
        }

        #endregion

        #region Product videos

        [HttpPost]
        public virtual async Task<IActionResult> ProductVideoAdd(int productId, [Validate] ProductVideoModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (productId == 0)
                throw new ArgumentException();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            var videoUrl = model.VideoUrl.TrimStart('~');

            try
            {
                await PingVideoUrlAsync(videoUrl);
            }
            catch (Exception exc)
            {
                return Json(new
                {
                    success = false,
                    error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd")} {exc.Message}",
                });
            }

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");
            try
            {
                var video = new Video
                {
                    VideoUrl = videoUrl
                };

                //insert video
                await _videoService.InsertVideoAsync(video);

                await _productService.InsertProductVideoAsync(new ProductVideo
                {
                    VideoId = video.Id,
                    ProductId = product.Id,
                    DisplayOrder = model.DisplayOrder
                });
            }
            catch (Exception exc)
            {
                return Json(new
                {
                    success = false,
                    error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoAdd")} {exc.Message}",
                });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductVideoList(ProductVideoSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareProductVideoListModelAsync(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductVideoUpdate([Validate] ProductVideoModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product picture with the specified id
            var productVideo = await _productService.GetProductVideoByIdAsync(model.Id)
                ?? throw new ArgumentException("No product video found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(productVideo.ProductId);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            //try to get a video with the specified id
            var video = await _videoService.GetVideoByIdAsync(productVideo.VideoId)
                ?? throw new ArgumentException("No video found with the specified id");

            var videoUrl = model.VideoUrl.TrimStart('~');

            try
            {
                await PingVideoUrlAsync(videoUrl);
            }
            catch (Exception exc)
            {
                return Json(new
                {
                    success = false,
                    error = $"{await _localizationService.GetResourceAsync("Admin.Catalog.Products.Multimedia.Videos.Alert.VideoUpdate")} {exc.Message}",
                });
            }

            video.VideoUrl = videoUrl;

            await _videoService.UpdateVideoAsync(video);

            productVideo.DisplayOrder = model.DisplayOrder;
            await _productService.UpdateProductVideoAsync(productVideo);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductVideoDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product video with the specified id
            var productVideo = await _productService.GetProductVideoByIdAsync(id)
                ?? throw new ArgumentException("No product video found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                var product = await _productService.GetProductByIdAsync(productVideo.ProductId);
                if (product != null && product.VendorId != currentVendor.Id)
                    return Content("This is not your product");
            }

            var videoId = productVideo.VideoId;
            await _productService.DeleteProductVideoAsync(productVideo);

            //try to get a video with the specified id
            var video = await _videoService.GetVideoByIdAsync(videoId)
                ?? throw new ArgumentException("No video found with the specified id");

            await _videoService.DeleteVideoAsync(video);

            return new NullJsonResult();
        }

        #endregion

        #region Product specification attributes

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ProductSpecificationAttributeAdd(AddSpecificationAttributeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null)
            {
                _notificationService.ErrorNotification("No product found with the specified id");
                return RedirectToAction("List");
            }

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
            {
                return RedirectToAction("List");
            }

            //we allow filtering only for "Option" attribute type
            if (model.AttributeTypeId != (int)SpecificationAttributeType.Option)
                model.AllowFiltering = false;

            //we don't allow CustomValue for "Option" attribute type
            if (model.AttributeTypeId == (int)SpecificationAttributeType.Option)
                model.ValueRaw = null;

            //store raw html if field allow this
            if (model.AttributeTypeId == (int)SpecificationAttributeType.CustomText
                || model.AttributeTypeId == (int)SpecificationAttributeType.Hyperlink)
                model.ValueRaw = model.Value;

            var psa = model.ToEntity<ProductSpecificationAttribute>();
            psa.CustomValue = model.ValueRaw;
            await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psa);

            switch (psa.AttributeType)
            {
                case SpecificationAttributeType.CustomText:
                    foreach (var localized in model.Locales)
                    {
                        await _localizedEntityService.SaveLocalizedValueAsync(psa,
                            x => x.CustomValue,
                            localized.Value,
                            localized.LanguageId);
                    }

                    break;
                case SpecificationAttributeType.CustomHtmlText:
                    foreach (var localized in model.Locales)
                    {
                        await _localizedEntityService.SaveLocalizedValueAsync(psa,
                            x => x.CustomValue,
                            localized.ValueRaw,
                            localized.LanguageId);
                    }

                    break;
                case SpecificationAttributeType.Option:
                    break;
                case SpecificationAttributeType.Hyperlink:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (continueEditing)
                return RedirectToAction("ProductSpecAttributeAddOrEdit",
                    new { productId = psa.ProductId, specificationId = psa.Id });

            //select an appropriate card
            SaveSelectedCardName("product-specification-attributes");
            return RedirectToAction("Edit", new { id = model.ProductId });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductSpecAttrList(ProductSpecificationAttributeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareProductSpecificationAttributeListModelAsync(searchModel, product);

            return Json(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ProductSpecAttrUpdate(AddSpecificationAttributeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product specification attribute with the specified id
            var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.SpecificationId);
            if (psa == null)
            {
                //select an appropriate card
                SaveSelectedCardName("product-specification-attributes");
                _notificationService.ErrorNotification("No product specification attribute found with the specified id");

                return RedirectToAction("Edit", new { id = model.ProductId });
            }

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null
                && (await _productService.GetProductByIdAsync(psa.ProductId)).VendorId != currentVendor.Id)
            {
                _notificationService.ErrorNotification("This is not your product");

                return RedirectToAction("List");
            }

            //we allow filtering and change option only for "Option" attribute type
            //save localized values for CustomHtmlText and CustomText
            switch (model.AttributeTypeId)
            {
                case (int)SpecificationAttributeType.Option:
                    psa.AllowFiltering = model.AllowFiltering;
                    psa.SpecificationAttributeOptionId = model.SpecificationAttributeOptionId;

                    break;
                case (int)SpecificationAttributeType.CustomHtmlText:
                    psa.CustomValue = model.ValueRaw;
                    foreach (var localized in model.Locales)
                    {
                        await _localizedEntityService.SaveLocalizedValueAsync(psa,
                            x => x.CustomValue,
                            localized.ValueRaw,
                            localized.LanguageId);
                    }

                    break;
                case (int)SpecificationAttributeType.CustomText:
                    psa.CustomValue = model.Value;
                    foreach (var localized in model.Locales)
                    {
                        await _localizedEntityService.SaveLocalizedValueAsync(psa,
                            x => x.CustomValue,
                            localized.Value,
                            localized.LanguageId);
                    }

                    break;
                default:
                    psa.CustomValue = model.Value;

                    break;
            }

            psa.ShowOnProductPage = model.ShowOnProductPage;
            psa.DisplayOrder = model.DisplayOrder;
            await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(psa);

            if (continueEditing)
            {
                return RedirectToAction("ProductSpecAttributeAddOrEdit",
                    new { productId = psa.ProductId, specificationId = model.SpecificationId });
            }

            //select an appropriate card
            SaveSelectedCardName("product-specification-attributes");

            return RedirectToAction("Edit", new { id = psa.ProductId });
        }

        public virtual async Task<IActionResult> ProductSpecAttributeAddOrEdit(int productId, int? specificationId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (await _productService.GetProductByIdAsync(productId) == null)
            {
                _notificationService.ErrorNotification("No product found with the specified id");
                return RedirectToAction("List");
            }

            //try to get a product specification attribute with the specified id
            try
            {
                var model = await _productModelFactory.PrepareAddSpecificationAttributeModelAsync(productId, specificationId);
                return View(model);
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);

                //select an appropriate card
                SaveSelectedCardName("product-specification-attributes");
                return RedirectToAction("Edit", new { id = productId });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductSpecAttrDelete(AddSpecificationAttributeModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product specification attribute with the specified id
            var psa = await _specificationAttributeService.GetProductSpecificationAttributeByIdAsync(model.SpecificationId);
            if (psa == null)
            {
                //select an appropriate card
                SaveSelectedCardName("product-specification-attributes");
                _notificationService.ErrorNotification("No product specification attribute found with the specified id");
                return RedirectToAction("Edit", new { id = model.ProductId });
            }

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && (await _productService.GetProductByIdAsync(psa.ProductId)).VendorId != currentVendor.Id)
            {
                _notificationService.ErrorNotification("This is not your product");
                return RedirectToAction("List", new { id = model.ProductId });
            }

            await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(psa);

            //select an appropriate card
            SaveSelectedCardName("product-specification-attributes");

            return RedirectToAction("Edit", new { id = psa.ProductId });
        }

        #endregion

        #region Product tags

        public virtual async Task<IActionResult> ProductTags()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareProductTagSearchModelAsync(new ProductTagSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductTags(ProductTagSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productModelFactory.PrepareProductTagListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductTagDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            //try to get a product tag with the specified id
            var tag = await _productTagService.GetProductTagByIdAsync(id)
                ?? throw new ArgumentException("No product tag found with the specified id");

            await _productTagService.DeleteProductTagAsync(tag);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags.Deleted"));

            return RedirectToAction("ProductTags");
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductTagsDelete(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var tags = await _productTagService.GetProductTagsByIdsAsync(selectedIds.ToArray());
            await _productTagService.DeleteProductTagsAsync(tags);

            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> EditProductTag(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            //try to get a product tag with the specified id
            var productTag = await _productTagService.GetProductTagByIdAsync(id);
            if (productTag == null)
                return RedirectToAction("List");

            //prepare tag model
            var model = await _productModelFactory.PrepareProductTagModelAsync(null, productTag);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> EditProductTag(ProductTagModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductTags))
                return AccessDeniedView();

            //try to get a product tag with the specified id
            var productTag = await _productTagService.GetProductTagByIdAsync(model.Id);
            if (productTag == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productTag.Name = model.Name;
                await _productTagService.UpdateProductTagAsync(productTag);

                //locales
                await UpdateLocalesAsync(productTag, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.ProductTags.Updated"));

                return continueEditing ? RedirectToAction("EditProductTag", new { id = productTag.Id }) : RedirectToAction("ProductTags");
            }

            //prepare model
            model = await _productModelFactory.PrepareProductTagModelAsync(model, productTag, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Purchased with order

        [HttpPost]
        public virtual async Task<IActionResult> PurchasedWithOrders(ProductOrderSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareProductOrderListModelAsync(searchModel, product);

            return Json(model);
        }

        #endregion

        #region Export / Import

        [HttpPost, ActionName("DownloadCatalogPDF")]
        [FormValueRequired("download-catalog-pdf")]
        public virtual async Task<IActionResult> DownloadCatalogAsPdf(ProductSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _productService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished);

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintProductsToPdfAsync(stream, products);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "pdfcatalog.pdf");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("ExportToXml")]
        [FormValueRequired("exportxml-all")]
        public virtual async Task<IActionResult> ExportXmlAll(ProductSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _productService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished);

            try
            {
                var xml = await _exportManager.ExportProductsToXmlAsync(products);

                return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ExportXmlSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(await _productService.GetProductsByIdsAsync(ids));
            }
            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
            }

            try
            {
                var xml = await _exportManager.ExportProductsToXmlAsync(products);
                return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("ExportToExcel")]
        [FormValueRequired("exportexcel-all")]
        public virtual async Task<IActionResult> ExportExcelAll(ProductSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _productService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished);

            try
            {
                var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

                return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(await _productService.GetProductsByIdsAsync(ids));
            }
            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
            }

            try
            {
                var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

                return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _importManager.ImportProductsFromXlsxAsync(importexcelfile.OpenReadStream());
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

                    return RedirectToAction("List");
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Imported"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("List");
            }
        }

        #endregion

        #region Tier prices

        [HttpPost]
        public virtual async Task<IActionResult> TierPriceList(TierPriceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareTierPriceListModelAsync(searchModel, product);

            return Json(model);
        }

        public virtual async Task<IActionResult> TierPriceCreatePopup(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            //prepare model
            var model = await _productModelFactory.PrepareTierPriceModelAsync(new TierPriceModel(), product, null);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> TierPriceCreatePopup(TierPriceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                //fill entity from model
                var tierPrice = model.ToEntity<TierPrice>();
                tierPrice.ProductId = product.Id;
                tierPrice.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null;

                await _productService.InsertTierPriceAsync(tierPrice);

                //update "HasTierPrices" property
                await _productService.UpdateHasTierPricesPropertyAsync(product);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _productModelFactory.PrepareTierPriceModelAsync(model, product, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> TierPriceEditPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a tier price with the specified id
            var tierPrice = await _productService.GetTierPriceByIdAsync(id);
            if (tierPrice == null)
                return RedirectToAction("List", "Product");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(tierPrice.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //prepare model
            var model = await _productModelFactory.PrepareTierPriceModelAsync(null, product, tierPrice);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> TierPriceEditPopup(TierPriceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a tier price with the specified id
            var tierPrice = await _productService.GetTierPriceByIdAsync(model.Id);
            if (tierPrice == null)
                return RedirectToAction("List", "Product");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(tierPrice.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                //fill entity from model
                tierPrice = model.ToEntity(tierPrice);
                tierPrice.CustomerRoleId = model.CustomerRoleId > 0 ? model.CustomerRoleId : (int?)null;
                await _productService.UpdateTierPriceAsync(tierPrice);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _productModelFactory.PrepareTierPriceModelAsync(model, product, tierPrice, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> TierPriceDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a tier price with the specified id
            var tierPrice = await _productService.GetTierPriceByIdAsync(id)
                ?? throw new ArgumentException("No tier price found with the specified id");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(tierPrice.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            await _productService.DeleteTierPriceAsync(tierPrice);

            //update "HasTierPrices" property
            await _productService.UpdateHasTierPricesPropertyAsync(product);

            return new NullJsonResult();
        }

        #endregion

        #region Product attributes

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeMappingList(ProductAttributeMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeMappingListModelAsync(searchModel, product);

            return Json(model);
        }

        public virtual async Task<IActionResult> ProductAttributeMappingCreate(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
                return RedirectToAction("List");
            }

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(new ProductAttributeMappingModel(), product, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ProductAttributeMappingCreate(ProductAttributeMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
                return RedirectToAction("List");
            }

            //ensure this attribute is not mapped yet
            if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
                .Any(x => x.ProductAttributeId == model.ProductAttributeId))
            {
                //redisplay form
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));

                model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, null, true);

                return View(model);
            }

            //insert mapping
            var productAttributeMapping = model.ToEntity<ProductAttributeMapping>();

            await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMapping);
            await UpdateLocalesAsync(productAttributeMapping, model);

            //predefined values
            var predefinedValues = await _productAttributeService.GetPredefinedProductAttributeValuesAsync(model.ProductAttributeId);
            foreach (var predefinedValue in predefinedValues)
            {
                var pav = new ProductAttributeValue
                {
                    ProductAttributeMappingId = productAttributeMapping.Id,
                    AttributeValueType = AttributeValueType.Simple,
                    Name = predefinedValue.Name,
                    PriceAdjustment = predefinedValue.PriceAdjustment,
                    PriceAdjustmentUsePercentage = predefinedValue.PriceAdjustmentUsePercentage,
                    WeightAdjustment = predefinedValue.WeightAdjustment,
                    Cost = predefinedValue.Cost,
                    IsPreSelected = predefinedValue.IsPreSelected,
                    DisplayOrder = predefinedValue.DisplayOrder
                };
                await _productAttributeService.InsertProductAttributeValueAsync(pav);

                //locales
                var languages = await _languageService.GetAllLanguagesAsync(true);

                //localization
                foreach (var lang in languages)
                {
                    var name = await _localizationService.GetLocalizedAsync(predefinedValue, x => x.Name, lang.Id, false, false);
                    if (!string.IsNullOrEmpty(name))
                        await _localizedEntityService.SaveLocalizedValueAsync(pav, x => x.Name, name, lang.Id);
                }
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Added"));

            if (!continueEditing)
            {
                //select an appropriate card
                SaveSelectedCardName("product-product-attributes");
                return RedirectToAction("Edit", new { id = product.Id });
            }

            return RedirectToAction("ProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
        }

        public virtual async Task<IActionResult> ProductAttributeMappingEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
                return RedirectToAction("List");
            }

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(null, product, productAttributeMapping);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ProductAttributeMappingEdit(ProductAttributeMappingModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.Id)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("This is not your product"));
                return RedirectToAction("List");
            }

            //ensure this attribute is not mapped yet
            if ((await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
                .Any(x => x.ProductAttributeId == model.ProductAttributeId && x.Id != productAttributeMapping.Id))
            {
                //redisplay form
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExists"));

                model = await _productModelFactory.PrepareProductAttributeMappingModelAsync(model, product, productAttributeMapping, true);

                return View(model);
            }

            //fill entity from model
            productAttributeMapping = model.ToEntity(productAttributeMapping);
            await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMapping);

            await UpdateLocalesAsync(productAttributeMapping, model);

            await SaveConditionAttributesAsync(productAttributeMapping, model.ConditionModel, form);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Updated"));

            if (!continueEditing)
            {
                //select an appropriate card
                SaveSelectedCardName("product-product-attributes");
                return RedirectToAction("Edit", new { id = product.Id });
            }

            return RedirectToAction("ProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeMappingDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(id)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //check if existed combinations contains the specified attribute
            var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
            if (existedCombinations?.Any() == true)
            {
                foreach (var combination in existedCombinations)
                {
                    var mappings = await _productAttributeParser
                        .ParseProductAttributeMappingsAsync(combination.AttributesXml);

                    if (mappings?.Any(m => m.Id == productAttributeMapping.Id) == true)
                    {
                        _notificationService.ErrorNotification(
                            string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.AlreadyExistsInCombination"),
                                await _productAttributeFormatter.FormatAttributesAsync(product, combination.AttributesXml, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), ", ")));

                        return RedirectToAction("ProductAttributeMappingEdit", new { id = productAttributeMapping.Id });
                    }
                }
            }

            await _productAttributeService.DeleteProductAttributeMappingAsync(productAttributeMapping);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Deleted"));

            //select an appropriate card
            SaveSelectedCardName("product-product-attributes");
            return RedirectToAction("Edit", new { id = productAttributeMapping.ProductId });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeValueList(ProductAttributeValueSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(searchModel.ProductAttributeMappingId)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeValueListModelAsync(searchModel, productAttributeMapping);

            return Json(model);
        }

        public virtual async Task<IActionResult> ProductAttributeValueCreatePopup(int productAttributeMappingId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeMappingId)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(new ProductAttributeValueModel(), productAttributeMapping, null);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeValueCreatePopup(ProductAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(model.ProductAttributeMappingId);
            if (productAttributeMapping == null)
                return RedirectToAction("List", "Product");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError(string.Empty, "Color is required");
                try
                {
                    //ensure color is valid (can be instantiated)
                    System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.Message);
                }
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
            {
                ModelState.AddModelError(string.Empty, "Image is required");
            }

            if (ModelState.IsValid)
            {
                //fill entity from model
                var pav = model.ToEntity<ProductAttributeValue>();

                pav.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;

                await _productAttributeService.InsertProductAttributeValueAsync(pav);
                await UpdateLocalesAsync(pav, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _productModelFactory.PrepareProductAttributeValueModelAsync(model, productAttributeMapping, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> ProductAttributeValueEditPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute value with the specified id
            var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id);
            if (productAttributeValue == null)
                return RedirectToAction("List", "Product");

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
            if (productAttributeMapping == null)
                return RedirectToAction("List", "Product");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeValueModelAsync(null, productAttributeMapping, productAttributeValue);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeValueEditPopup(ProductAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute value with the specified id
            var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(model.Id);
            if (productAttributeValue == null)
                return RedirectToAction("List", "Product");

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId);
            if (productAttributeMapping == null)
                return RedirectToAction("List", "Product");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            if (productAttributeMapping.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (string.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError(string.Empty, "Color is required");
                try
                {
                    //ensure color is valid (can be instantiated)
                    System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.Message);
                }
            }

            //ensure a picture is uploaded
            if (productAttributeMapping.AttributeControlType == AttributeControlType.ImageSquares && model.ImageSquaresPictureId == 0)
            {
                ModelState.AddModelError(string.Empty, "Image is required");
            }

            if (ModelState.IsValid)
            {
                //fill entity from model
                productAttributeValue = model.ToEntity(productAttributeValue);
                productAttributeValue.Quantity = model.CustomerEntersQty ? 1 : model.Quantity;
                await _productAttributeService.UpdateProductAttributeValueAsync(productAttributeValue);

                await UpdateLocalesAsync(productAttributeValue, model);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _productModelFactory.PrepareProductAttributeValueModelAsync(model, productAttributeMapping, productAttributeValue, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeValueDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product attribute value with the specified id
            var productAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(id)
                ?? throw new ArgumentException("No product attribute value found with the specified id");

            //try to get a product attribute mapping with the specified id
            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(productAttributeValue.ProductAttributeMappingId)
                ?? throw new ArgumentException("No product attribute mapping found with the specified id");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //check if existed combinations contains the specified attribute value
            var existedCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id);
            if (existedCombinations?.Any() == true)
            {
                foreach (var combination in existedCombinations)
                {
                    var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(combination.AttributesXml);

                    if (attributeValues.Where(attribute => attribute.Id == id).Any())
                    {
                        return Conflict(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.AlreadyExistsInCombination"),
                            await _productAttributeFormatter.FormatAttributesAsync(product, combination.AttributesXml, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), ", ")));
                    }
                }
            }

            await _productAttributeService.DeleteProductAttributeValueAsync(productAttributeValue);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> AssociateProductToAttributeValuePopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareAssociateProductToAttributeValueSearchModelAsync(new AssociateProductToAttributeValueSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AssociateProductToAttributeValuePopupList(AssociateProductToAttributeValueSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productModelFactory.PrepareAssociateProductToAttributeValueListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> AssociateProductToAttributeValuePopup([Bind(Prefix = nameof(AssociateProductToAttributeValueModel))] AssociateProductToAttributeValueModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var associatedProduct = await _productService.GetProductByIdAsync(model.AssociatedToProductId);
            if (associatedProduct == null)
                return Content("Cannot load a product");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && associatedProduct.VendorId != currentVendor.Id)
                return Content("This is not your product");

            ViewBag.RefreshPage = true;
            ViewBag.productId = associatedProduct.Id;
            ViewBag.productName = associatedProduct.Name;

            return View(new AssociateProductToAttributeValueSearchModel());
        }

        //action displaying notification (warning) to a store owner when associating some product
        public virtual async Task<IActionResult> AssociatedProductGetWarnings(int productId)
        {
            var associatedProduct = await _productService.GetProductByIdAsync(productId);
            if (associatedProduct == null)
                return Json(new { Result = string.Empty });

            //attributes
            if (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(associatedProduct.Id) is IList<ProductAttributeMapping> mapping && mapping.Any())
            {
                if (mapping.Any(attribute => attribute.IsRequired))
                    return Json(new { Result = await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.HasRequiredAttributes") });

                return Json(new { Result = await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.HasAttributes") });
            }

            //gift card
            if (associatedProduct.IsGiftCard)
            {
                return Json(new { Result = await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.GiftCard") });
            }

            //downloadable product
            if (associatedProduct.IsDownload)
            {
                return Json(new { Result = await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.Attributes.Values.Fields.AssociatedProduct.Downloadable") });
            }

            return Json(new { Result = string.Empty });
        }

        #endregion

        #region Product attribute combinations

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeCombinationList(ProductAttributeCombinationSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeCombinationListModelAsync(searchModel, product);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeCombinationDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a combination with the specified id
            var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id)
                ?? throw new ArgumentException("No product attribute combination found with the specified id");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(combination.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            await _productAttributeService.DeleteProductAttributeCombinationAsync(combination);

            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductAttributeCombinationCreatePopup(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeCombinationCreatePopup(int productId, ProductAttributeCombinationModel model, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //attributes
            var warnings = new List<string>();
            var attributesXml = await GetAttributesXmlForProductAttributeCombinationAsync(form, warnings, product.Id);

            //check whether the attribute value is specified
            if (string.IsNullOrEmpty(attributesXml))
                warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Alert.FailedValue"));

            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));

            //check whether the same attribute combination already exists
            var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
            if (existingCombination != null)
                warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.AlreadyExists"));

            if (!warnings.Any())
            {
                //save combination
                var combination = model.ToEntity<ProductAttributeCombination>();

                //fill attributes
                combination.AttributesXml = attributesXml;

                await _productAttributeService.InsertProductAttributeCombinationAsync(combination);

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity, combination.StockQuantity,
                    message: await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Combination.Edit"), combinationId: combination.Id);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, null, true);
            model.Warnings = warnings;

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> ProductAttributeCombinationGeneratePopup(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(new ProductAttributeCombinationModel(), product, null);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeCombinationGeneratePopup(IFormCollection form, ProductAttributeCombinationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null)
                return RedirectToAction("List", "Product");

            var allowedAttributeIds = form.Keys.Where(key => key.Contains("attribute_value_"))
                .Select(key => int.TryParse(form[key], out var id) ? id : 0).Where(id => id > 0).ToList();

            var requiredAttributeNames = await (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
                .Where(pam => pam.IsRequired)
                .Where(pam => !pam.IsNonCombinable())
                .WhereAwait(async pam => !(await _productAttributeService.GetProductAttributeValuesAsync(pam.Id)).Any(v => allowedAttributeIds.Any(id => id == v.Id)))
                .SelectAwait(async pam => (await _productAttributeService.GetProductAttributeByIdAsync(pam.ProductAttributeId)).Name).ToListAsync();

            if (requiredAttributeNames.Any())
            {
                model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, null, true);
                var pavModels = model.ProductAttributes.SelectMany(pa => pa.Values)
                    .Where(v => allowedAttributeIds.Any(id => id == v.Id))
                    .ToList();
                foreach (var pavModel in pavModels)
                {
                    pavModel.Checked = "checked";
                }

                model.Warnings.Add(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.SelectRequiredAttributes"), string.Join(", ", requiredAttributeNames)));

                return View(model);
            }

            await GenerateAttributeCombinationsAsync(product, allowedAttributeIds);

            ViewBag.RefreshPage = true;

            return View(new ProductAttributeCombinationModel());
        }

        public virtual async Task<IActionResult> ProductAttributeCombinationEditPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a combination with the specified id
            var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id);
            if (combination == null)
                return RedirectToAction("List", "Product");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(combination.ProductId);
            if (product == null)
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //prepare model
            var model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(null, product, combination);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAttributeCombinationEditPopup(ProductAttributeCombinationModel model, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a combination with the specified id
            var combination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(model.Id);
            if (combination == null)
                return RedirectToAction("List", "Product");

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(combination.ProductId);
            if (product == null)
                return RedirectToAction("List", "Product");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List", "Product");

            //attributes
            var warnings = new List<string>();
            var attributesXml = await GetAttributesXmlForProductAttributeCombinationAsync(form, warnings, product.Id);

            //check whether the attribute value is specified
            if (string.IsNullOrEmpty(attributesXml))
                warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.Alert.FailedValue"));

            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart, product, 1, attributesXml, true));

            //check whether the same attribute combination already exists
            var existingCombination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
            if (existingCombination != null && existingCombination.Id != model.Id && existingCombination.AttributesXml.Equals(attributesXml))
                warnings.Add(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ProductAttributes.AttributeCombinations.AlreadyExists"));

            if (!warnings.Any() && ModelState.IsValid)
            {
                var previousStockQuantity = combination.StockQuantity;

                //save combination
                //fill entity from model
                combination = model.ToEntity(combination);
                combination.AttributesXml = attributesXml;

                await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(product, combination.StockQuantity - previousStockQuantity, combination.StockQuantity,
                    message: await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Combination.Edit"), combinationId: combination.Id);

                ViewBag.RefreshPage = true;

                return View(model);
            }

            //prepare model
            model = await _productModelFactory.PrepareProductAttributeCombinationModelAsync(model, product, combination, true);
            model.Warnings = warnings;

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> GenerateAllAttributeCombinations(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(productId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            await GenerateAttributeCombinationsAsync(product);

            return Json(new { Success = true });
        }

        #endregion

        #region Product editor settings

        [HttpPost]
        public virtual async Task<IActionResult> SaveProductEditorSettings(ProductModel model, string returnUrl = "")
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //vendors cannot manage these settings
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("List");

            var productEditorSettings = await _settingService.LoadSettingAsync<ProductEditorSettings>();
            productEditorSettings = model.ProductEditorSettingsModel.ToSettings(productEditorSettings);
            await _settingService.SaveSettingAsync(productEditorSettings);

            //product list
            if (string.IsNullOrEmpty(returnUrl))
                return RedirectToAction("List");

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("List");

            return Redirect(returnUrl);
        }

        #endregion

        #region Stock quantity history

        [HttpPost]
        public virtual async Task<IActionResult> StockQuantityHistory(StockQuantityHistorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return await AccessDeniedDataTablesJson();

            var product = await _productService.GetProductByIdAsync(searchModel.ProductId)
                ?? throw new ArgumentException("No product found with the specified id");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return Content("This is not your product");

            //prepare model
            var model = await _productModelFactory.PrepareStockQuantityHistoryListModelAsync(searchModel, product);

            return Json(model);
        }

        #endregion

        #endregion
    }
}